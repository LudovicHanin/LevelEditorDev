using System;

public struct Cell : IEquatable<Cell>
{
    public int x, y, z;
    
    public Cell(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public static Cell operator +(Cell a, Cell b) => new Cell(a.x + b.x, a.y + b.y, a.z + b.z);
    public static Cell Rotate(Cell cell, int rotation) => (rotation & 3) switch
    {
        0 => cell,
        1 => new Cell(cell.z, cell.y, -cell.x),
        2 => new Cell(-cell.x, cell.y, -cell.z),
        3 => new Cell(-cell.z, cell.y, cell.x),
        _ => cell
    };

    public bool Equals(Cell other) => x == other.x && y == other.y && z == other.z;
    public override bool Equals(object obj) => obj is Cell other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(x, y, z);
    public override string ToString() => $"Cell({x}, {y}, {z})";
}
