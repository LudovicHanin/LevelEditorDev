using NUnit.Framework;
using UnityEngine;
using LevelEditor.Core;
using LevelEditor.Runtime;

public class GridConversionTests
{
    const float Tolerance = 0.0001f;

    static void AssertNear(Vector3 expected, Vector3 actual)
    {
        Assert.AreEqual(expected.x, actual.x, Tolerance, "x");
        Assert.AreEqual(expected.y, actual.y, Tolerance, "y");
        Assert.AreEqual(expected.z, actual.z, Tolerance, "z");
    }

    [Test]
    public void CellToWorld_UnitGrid_SameNumbers()
    {
        var world = GridConversion.CellToWorld(new Cell(2, 0, 3), Vector3.zero, Vector3.one);
        AssertNear(new Vector3(2, 0, 3), world);
    }

    [Test]
    public void CellToWorld_UsesOriginAndCellSize()
    {
        var world = GridConversion.CellToWorld(new Cell(1, 1, 1), new Vector3(10, 0, 0), new Vector3(2, 3, 2));
        AssertNear(new Vector3(12, 3, 2), world);
    }


    [TestCase(0.49f, 0)]
    [TestCase(0.51f, 1)]
    [TestCase(-0.49f, 0)]
    [TestCase(-0.51f, -1)]
    [TestCase(1.5f, 2)]   
    [TestCase(2.5f, 3)]   
    public void WorldToCell_X_RoundsToNearestCenter(float worldX, int expectedX)
    {
        var cell = GridConversion.WorldToCell(new Vector3(worldX, 0, 0), Vector3.zero, Vector3.one);
        Assert.AreEqual(expectedX, cell.x);
    }

    [TestCase(0.0f, 0)]
    [TestCase(0.7f, 0)]
    [TestCase(1.0f, 1)]  
    [TestCase(1.9f, 1)]
    public void WorldToCell_Y_RoundsDownToFloor(float worldY, int expectedY)
    {
        var cell = GridConversion.WorldToCell(new Vector3(0, worldY, 0), Vector3.zero, Vector3.one);
        Assert.AreEqual(expectedY, cell.y);
    }

    [Test]
    public void RoundTrip_CellToWorldToCell_GivesBackSameCell()
    {
        var origin = new Vector3(5.3f, -2f, 1.7f);
        var cellSize = new Vector3(1.5f, 2.5f, 0.75f);

        for (int x = -3; x <= 3; x++)
            for (int y = -2; y <= 2; y++)
                for (int z = -3; z <= 3; z++)
                {
                    var cell = new Cell(x, y, z);
                    var world = GridConversion.CellToWorld(cell, origin, cellSize);
                    Assert.AreEqual(cell, GridConversion.WorldToCell(world, origin, cellSize));
                }
    }

    [Test]
    public void Rotation_UnityQuaternion_MatchesGridMath()
    {
        var offsets = new[] { new Cell(1, 0, 0), new Cell(0, 0, 1), new Cell(2, 1, 1), new Cell(-1, 0, 3) };

        for (int r = 0; r < 4; r++)
            foreach (var offset in offsets)
            {
                Vector3 unityResult = GridConversion.RotationToQuaternion(r) * offset.ToVector3Int();
                Vector3 coreResult = GridMath.Rotate(offset, r).ToVector3Int();

                AssertNear(coreResult, unityResult);
            }
    }

    [Test]
    public void Extensions_CellAndVector3Int_RoundTrip()
    {
        var cell = new Cell(4, -2, 7);
        Assert.AreEqual(cell, cell.ToVector3Int().ToCell());
    }
}