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

    public bool Equals(Cell other) => x == other.x && y == other.y && z == other.z;
    public override bool Equals(object obj) => obj is Cell other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(x, y, z);
    public override string ToString() => $"Cell({x}, {y}, {z})";
}
