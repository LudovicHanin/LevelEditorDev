using System;

namespace LevelEditor.Core
{
    public sealed class PlacedObject
    {
        public string InstanceId { get; }
        public string AssetId { get; }

        // 'internal set': only code inside the Core assembly (LevelData) can move an object.
        // This protects the occupancy index: nobody can change Anchor behind LevelData's back.
        public Cell Anchor { get; internal set; }
        public int Rotation { get; internal set; }

        public PlacedObject(string instanceId, string assetId, Cell anchor, int rotation)
        {
            if (string.IsNullOrEmpty(instanceId)) throw new ArgumentException("InstanceId is required.", nameof(instanceId));
            if (string.IsNullOrEmpty(assetId)) throw new ArgumentException("AssetId is required.", nameof(assetId));

            InstanceId = instanceId;
            AssetId = assetId;
            Anchor = anchor;
            Rotation = GridMath.NormalizeRotation(rotation);
        }

        /// <summary>Creates a brand-new object with a fresh unique id.</summary>
        public static PlacedObject Create(string assetId, Cell anchor, int rotation = 0)
            => new PlacedObject(Guid.NewGuid().ToString("N"), assetId, anchor, rotation);

        /// <summary>
        /// An independent copy with the SAME InstanceId. Used by RemoveCommand
        /// to resurrect exactly the same object on undo.
        /// </summary>
        public PlacedObject Clone() => new PlacedObject(InstanceId, AssetId, Anchor, Rotation);

        public override string ToString() => $"{AssetId} [{InstanceId}] at {Anchor} r{Rotation}";

    }
}