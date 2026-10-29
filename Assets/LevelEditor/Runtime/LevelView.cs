using System.Collections.Generic;
using UnityEngine;
using LevelEditor.Core;

namespace LevelEditor.Runtime
{
    public class LevelView : MonoBehaviour
    {
        [SerializeField] PlaceableCatalog catalog;
        [SerializeField] GridSettings grid;

        [Header("Debug")]
        [Tooltip("Size of the grid drawn in the Scene view (cells per side). 0 = no grid.")]
        [SerializeField] int gizmoGridSize = 10;

        // instanceId → the GameObject showing that object.
        readonly Dictionary<string, GameObject> _views = new Dictionary<string, GameObject>();

        LevelData _level;

        public void Bind(LevelData level)
        {
            if (level == null) 
                throw new System.ArgumentNullException(nameof(level));

            if (catalog == null || grid == null)
            {
                Debug.LogError("[LevelView] Assign the Catalog and Grid fields in the Inspector.", this);
                return;
            }

            // Safety: if this view was already bound to another level, stop listening to it.
            Unbind();

            _level = level;
            _level.ObjectAdded += HandleAdded;
            _level.ObjectRemoved += HandleRemoved;
            _level.ObjectModified += HandleModified;

            Rebuild();
        }

        public void Unbind()
        {
            if (_level == null) 
                return;

            _level.ObjectAdded -= HandleAdded;
            _level.ObjectRemoved -= HandleRemoved;
            _level.ObjectModified -= HandleModified;
            _level = null;

            DestroyAllViews();
        }

        public void Rebuild()
        {
            DestroyAllViews();
            if (_level == null) 
                return;

            foreach (var obj in _level.Objects)
                CreateView(obj);
        }

        void OnDestroy() => Unbind();

        void HandleAdded(PlacedObject obj) => CreateView(obj);
        void HandleRemoved(PlacedObject obj) => DestroyView(obj.InstanceId);

        void HandleModified(PlacedObject obj)
        {
            if (_views.TryGetValue(obj.InstanceId, out var go))
                ApplyTransform(go, obj);
        }

        void CreateView(PlacedObject obj)
        {
            DestroyView(obj.InstanceId);   // safety: never two GameObjects for one object

            GameObject go;
            if (catalog.TryGet(obj.AssetId, out var definition) && definition.Prefab != null)
            {
                go = Instantiate(definition.Prefab, transform);
            }
            else
            {
                Debug.LogWarning($"[LevelView] No prefab for '{obj.AssetId}'. Showing an empty placeholder.", this);
                go = new GameObject();
                go.transform.SetParent(transform, false);
            }

            string shortId = obj.InstanceId.Length > 6 ? obj.InstanceId.Substring(0, 6) : obj.InstanceId;
            go.name = $"{obj.AssetId} [{shortId}]";

            ApplyTransform(go, obj);
            _views.Add(obj.InstanceId, go);
        }

        void DestroyView(string instanceId)
        {
            if (_views.TryGetValue(instanceId, out var go))
            {
                Destroy(go);
                _views.Remove(instanceId);
            }
        }

        void DestroyAllViews()
        {
            foreach (var go in _views.Values)
            {
                if (go != null)
                    Destroy(go);
            }

            _views.Clear();
        }

        void ApplyTransform(GameObject go, PlacedObject obj)
        {
            go.transform.SetPositionAndRotation(grid.CellToWorld(obj.Anchor),
                                                grid.RotationToWorld(obj.Rotation));
        }

        void OnDrawGizmos()
        {
            if (grid == null || gizmoGridSize <= 0) return;

            Gizmos.color = new Color(1f, 1f, 1f, 0.3f);
            Vector3 size = grid.CellSize;
            Vector3 start = grid.CellToWorld(Cell.Zero) - new Vector3(size.x, 0f, size.z) * 0.5f + Vector3.up * 0.01f;
            float lengthX = gizmoGridSize * size.x;
            float lengthZ = gizmoGridSize * size.z;

            for (int i = 0; i <= gizmoGridSize; i++)
            {
                Gizmos.DrawLine(start + new Vector3(i * size.x, 0, 0), start + new Vector3(i * size.x, 0, lengthZ));
                Gizmos.DrawLine(start + new Vector3(0, 0, i * size.z), start + new Vector3(lengthX, 0, i * size.z));
            }
        }
    }
}