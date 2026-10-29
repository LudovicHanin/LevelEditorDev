using UnityEngine;
using LevelEditor.Core;

namespace LevelEditor.Runtime
{
    [CreateAssetMenu(fileName = "NewPlaceable", menuName = "Level Editor/Placeable Definition")]
    public class PlaceableDefinition : ScriptableObject
    {
        [Tooltip("Unique id saved in level files, e.g. \"prop.crate\". Never change it once levels use it.")]
        [SerializeField] string id;

        [Tooltip("Name shown to the player in the palette.")]
        [SerializeField] string displayName;

        [Tooltip("What gets instantiated in the scene. Pivot = center of the anchor cell, at floor level.")]
        [SerializeField] GameObject prefab;

        [Tooltip("How many cells the piece covers, starting from its anchor toward +X, +Y, +Z.")]
        [SerializeField] Vector3Int size = Vector3Int.one;

        [SerializeField] Sprite thumbnail;
        [SerializeField] string category = "Props";

        Footprint _footprint;

        public string Id => id;
        public string DisplayName => string.IsNullOrEmpty(displayName) ? id : displayName;
        public GameObject Prefab => prefab;
        public Vector3Int Size => size;
        public Sprite Thumbnail => thumbnail;
        public string Category => category;

        public Footprint GetFootprint()
        {
            if (_footprint == null)
                _footprint = Footprint.Box(size.x, size.y, size.z);
            return _footprint;
        }

        void OnValidate()
        {
            size = Vector3Int.Max(size, Vector3Int.one);   
            _footprint = null;                             
        }
    }
}