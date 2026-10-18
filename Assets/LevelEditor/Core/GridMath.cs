/// <summary>
/// Pure grid formulas. No state, no Unity.
/// Rotation convention: r = number of quarter turns around the vertical Y axis,
/// positive = clockwise seen from above. This matches Unity's Quaternion.Euler(0, 90 * r, 0),
/// which the view will use later to orient the prefab.
/// </summary>

namespace LevelEditor.Core
{
    public static class GridMath
    {
        /// <summary>Brings any integer into 0..3. Handles negatives: -1 becomes 3.</summary>
        public static int NormalizeRotation(int r)
        {
            return ((r % 4) + 4) % 4;
        }

        /// <summary>Rotates an offset by r quarter turns around Y. Y never changes.</summary>
        public static Cell Rotate(Cell c, int r)
        {
            switch (NormalizeRotation(r))
            {
                case 1: return new Cell(c.z, c.y, -c.x);
                case 2: return new Cell(-c.x, c.y, -c.z);
                case 3: return new Cell(-c.z, c.y, c.x);
                default: return c;
            }
        }
    }
}
