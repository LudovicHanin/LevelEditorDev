using UnityEngine;
using LevelEditor.Core;

namespace LevelEditor.Runtime
{
    [CreateAssetMenu(fileName = "GridSettings", menuName = "Level Editor/Grid Settings")]
    public class GridSettings : ScriptableObject
    {
        [Tooltip("World position of the center of cell (0,0,0), at floor level.")]
        [SerializeField] Vector3 origin = Vector3.zero;

        [Tooltip("Size of one cell in meters. Y is the height of one floor.")]
        [SerializeField] Vector3 cellSize = Vector3.one;

        public Vector3 Origin => origin;
        public Vector3 CellSize => cellSize;

        public Vector3 CellToWorld(Cell cell) => GridConversion.CellToWorld(cell, origin, cellSize);
        public Cell WorldToCell(Vector3 world) => GridConversion.WorldToCell(world, origin, cellSize);
        public Quaternion RotationToWorld(int rotation) => GridConversion.RotationToQuaternion(rotation);

        void OnValidate()
        {
            const float min = 0.01f;
            cellSize = new Vector3(Mathf.Max(cellSize.x, min),
                                   Mathf.Max(cellSize.y, min),
                                   Mathf.Max(cellSize.z, min));
        }
    }
}