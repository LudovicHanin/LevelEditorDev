using System;
using System.Collections.Generic;

namespace LevelEditor.Core
{
    public class LevelData
    {
        readonly IFootprintProvider _footprints;

        // Source of truth: instanceId → object.
        readonly Dictionary<string, PlacedObject> _objects = new Dictionary<string, PlacedObject>();

        // Occupancy index (a cache derived from _objects): cell → instanceId standing on it.
        // INVARIANT: always modified together with _objects, in the same method. Never saved.
        readonly Dictionary<Cell, string> _cellToId = new Dictionary<Cell, string>();

        // Fired AFTER the change, when the data is fully consistent,
        // so listeners can safely query LevelData from inside the event.
        public event Action<PlacedObject> ObjectAdded;
        public event Action<PlacedObject> ObjectRemoved;
        public event Action<PlacedObject> ObjectModified;

        public LevelData(IFootprintProvider footprints)
        {
            _footprints = footprints ?? throw new ArgumentNullException(nameof(footprints));
        }

        // ---------------------------------------------------------------------
        // QUESTIONS
        // ---------------------------------------------------------------------
        public int Count => _objects.Count;
        public int OccupiedCellCount => _cellToId.Count;

        /// <summary>Live view: do not add or remove objects while iterating it.</summary>
        public IEnumerable<PlacedObject> Objects => _objects.Values;

        public bool Contains(string instanceId) => _objects.ContainsKey(instanceId);

        public bool TryGet(string instanceId, out PlacedObject obj) => _objects.TryGetValue(instanceId, out obj);

        public bool IsOccupied(Cell cell) => _cellToId.ContainsKey(cell);

        public bool TryGetOccupant(Cell cell, out string instanceId) => _cellToId.TryGetValue(cell, out instanceId);

        /// <summary>
        /// Could a piece of this kind fit here?
        /// ignoreId: squares occupied by THIS object count as free (used when moving an object).
        /// </summary>
        public bool CanPlace(string assetId, Cell anchor, int rotation, string ignoreId = null)
        {
            foreach (var cell in CellsOf(assetId, anchor, rotation))
            {
                if (_cellToId.TryGetValue(cell, out var occupant) && occupant != ignoreId)
                    return false;
            }
            return true;
        }

        // ---------------------------------------------------------------------
        // MUTATIONS — called by commands. They throw on misuse: an exception here
        // means a bug upstream (a tool forgot to check CanPlace), not a normal case.
        // ---------------------------------------------------------------------
        public void Add(PlacedObject obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            if (_objects.ContainsKey(obj.InstanceId))
                throw new InvalidOperationException($"Object {obj.InstanceId} is already in the level.");
            if (!CanPlace(obj.AssetId, obj.Anchor, obj.Rotation))
                throw new InvalidOperationException($"Cannot add {obj}: some cells are occupied.");

            _objects.Add(obj.InstanceId, obj);
            foreach (var cell in CellsOf(obj.AssetId, obj.Anchor, obj.Rotation))
                _cellToId.Add(cell, obj.InstanceId);

            ObjectAdded?.Invoke(obj);
        }

        public void Remove(string instanceId)
        {
            if (!_objects.TryGetValue(instanceId, out var obj))
                throw new InvalidOperationException($"Object {instanceId} is not in the level.");

            foreach (var cell in CellsOf(obj.AssetId, obj.Anchor, obj.Rotation))
                _cellToId.Remove(cell);
            _objects.Remove(instanceId);

            ObjectRemoved?.Invoke(obj);
        }

        /// <summary>Moves and/or rotates an object. Rotating in place = same anchor, new rotation.</summary>
        public void Move(string instanceId, Cell newAnchor, int newRotation)
        {
            if (!_objects.TryGetValue(instanceId, out var obj))
                throw new InvalidOperationException($"Object {instanceId} is not in the level.");

            newRotation = GridMath.NormalizeRotation(newRotation);
            if (!CanPlace(obj.AssetId, newAnchor, newRotation, ignoreId: instanceId))
                throw new InvalidOperationException($"Cannot move {obj} to {newAnchor} r{newRotation}: cells occupied.");

            // 1. Free the old squares.
            foreach (var cell in CellsOf(obj.AssetId, obj.Anchor, obj.Rotation))
                _cellToId.Remove(cell);

            // 2. Update the object.
            obj.Anchor = newAnchor;
            obj.Rotation = newRotation;

            // 3. Claim the new squares (all free now: CanPlace ignored our own old ones).
            foreach (var cell in CellsOf(obj.AssetId, obj.Anchor, obj.Rotation))
                _cellToId.Add(cell, obj.InstanceId);

            ObjectModified?.Invoke(obj);
        }

        /// <summary>
        /// Empties the level. Not undoable: meant for loading a new level, not for editing.
        /// Fires ObjectRemoved for each object so the view stays in sync.
        /// </summary>
        public void Clear()
        {
            var removed = new List<PlacedObject>(_objects.Values);
            _objects.Clear();
            _cellToId.Clear();

            foreach (var obj in removed)
                ObjectRemoved?.Invoke(obj);
        }

        // ---------------------------------------------------------------------
        // HELPERS
        // ---------------------------------------------------------------------
        IEnumerable<Cell> CellsOf(string assetId, Cell anchor, int rotation)
            => GetFootprint(assetId).GetWorldCells(anchor, rotation);

        Footprint GetFootprint(string assetId)
        {
            var fp = _footprints.GetFootprint(assetId);
            if (fp == null)
                throw new KeyNotFoundException($"No footprint for asset '{assetId}'. Is it registered in the provider?");
            return fp;
        }
    }
}