using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LevelEditor.Core
{
    public sealed class Footprint
    {
        public IReadOnlyList<Cell> Offsets { get; }

        public Footprint(IEnumerable<Cell> offsets)
        {
            if (offsets == null) throw new ArgumentNullException(nameof(offsets));

            var list = new List<Cell>();
            var seen = new HashSet<Cell>();
            foreach (var offset in offsets)
            {
                if (!seen.Add(offset))
                    throw new ArgumentException($"Duplicate offset {offset} in footprint.", nameof(offsets));
                list.Add(offset);
            }

            if (list.Count == 0)
                throw new ArgumentException("A footprint must cover at least one cell.", nameof(offsets));
            if (!seen.Contains(Cell.Zero))
                throw new ArgumentException("A footprint must contain the anchor (0,0,0).", nameof(offsets));

            // Copy into a read-only wrapper: nobody can modify the shape after construction.
            Offsets = new ReadOnlyCollection<Cell>(list);
        }

        /// <summary>A single 1󪻑 cell.</summary>
        public static Footprint Single() => new Footprint(new[] { Cell.Zero });

        /// <summary>A full box. The anchor is the corner with the smallest coordinates.</summary>
        public static Footprint Box(int sizeX, int sizeY, int sizeZ)
        {
            if (sizeX < 1 || sizeY < 1 || sizeZ < 1)
                throw new ArgumentException("Box sizes must be at least 1.");

            var offsets = new List<Cell>(sizeX * sizeY * sizeZ);
            for (int x = 0; x < sizeX; x++)
                for (int y = 0; y < sizeY; y++)
                    for (int z = 0; z < sizeZ; z++)
                        offsets.Add(new Cell(x, y, z));

            return new Footprint(offsets);
        }

        /// <summary>
        /// The real grid squares covered by a piece of this shape,
        /// placed at 'anchor' and turned 'rotation' quarter turns.
        /// </summary>
        public IEnumerable<Cell> GetWorldCells(Cell anchor, int rotation)
        {
            foreach (var offset in Offsets)
                yield return anchor + GridMath.Rotate(offset, rotation);
        }
    }
}