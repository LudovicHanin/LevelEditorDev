using UnityEngine;
using LevelEditor.Core;

namespace LevelEditor.Runtime
{
    public static class GridConversion
    {
        public static Vector3Int ToVector3Int(this Cell cell) => new Vector3Int(cell.x, cell.y, cell.z);
        public static Cell ToCell(this Vector3Int v) => new Cell(v.x, v.y, v.z);
        public static Vector3 CellToWorld(Cell cell, Vector3 origin, Vector3 cellSize)
        {
            return origin + new Vector3(cell.x * cellSize.x,
                                        cell.y * cellSize.y,
                                        cell.z * cellSize.z);
        }

        public static Quaternion RotationToQuaternion(int rotation)
        {
            return Quaternion.Euler(0f, 90f * GridMath.NormalizeRotation(rotation), 0f);
        }

        public static Cell WorldToCell(Vector3 world, Vector3 origin, Vector3 cellSize)
        {
            Vector3 local = world - origin;
            return new Cell(Mathf.FloorToInt(local.x / cellSize.x + 0.5f),
                            Mathf.FloorToInt(local.y / cellSize.y),
                            Mathf.FloorToInt(local.z / cellSize.z + 0.5f));
        }
    }
}