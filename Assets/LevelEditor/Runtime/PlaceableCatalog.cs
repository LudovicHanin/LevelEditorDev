using System.Collections.Generic;
using UnityEngine;
using LevelEditor.Core;

namespace LevelEditor.Runtime
{
    [CreateAssetMenu(fileName = "PlaceableCatalog", menuName = "Level Editor/Placeable Catalog")]
    public class PlaceableCatalog : ScriptableObject, IFootprintProvider
    {
        [Tooltip("Every piece the player can place. Drag your Placeable Definition assets here.")]
        [SerializeField] List<PlaceableDefinition> definitions = new List<PlaceableDefinition>();

        Dictionary<string, PlaceableDefinition> _byId;

        public IReadOnlyList<PlaceableDefinition> Definitions => definitions;

        public bool TryGet(string assetId, out PlaceableDefinition definition)
        {
            EnsureIndex();
            return _byId.TryGetValue(assetId, out definition);
        }

        public Footprint GetFootprint(string assetId)
        {
            return TryGet(assetId, out var definition) ? definition.GetFootprint() : null;
        }

        void EnsureIndex()
        {
            if (_byId != null) return;

            _byId = new Dictionary<string, PlaceableDefinition>();
            foreach (var definition in definitions)
            {
                if (definition == null)
                    continue;

                if (string.IsNullOrEmpty(definition.Id))
                {
                    Debug.LogWarning($"[Catalog] '{definition.name}' has no id and will be ignored.", definition);
                    continue;
                }

                if (_byId.ContainsKey(definition.Id))
                {
                    Debug.LogError($"[Catalog] Duplicate id '{definition.Id}' on '{definition.name}'. " +
                                   $"Only the first one is used.", definition);
                    continue;
                }

                _byId.Add(definition.Id, definition);
            }
        }

        void OnEnable() => _byId = null;
        void OnValidate() => _byId = null;

        [ContextMenu("Validate Catalog")]
        void ValidateCatalog()
        {
            _byId = null;
            EnsureIndex();
            Debug.Log($"[Catalog] {_byId.Count} valid placeable(s) in '{name}'.", this);
        }
    }
}