using System;
using System.Collections.Generic;
using NUnit.Framework;
using LevelEditor.Core;

public class FootprintTests
{
    [Test]
    public void GetWorldCells_LShapeRotatedOnce_MatchesDiagram()
    {
        var l = new Footprint(new[] { new Cell(0, 0, 0), new Cell(1, 0, 0), new Cell(2, 0, 0), new Cell(0, 0, 1) });

        var cells = new List<Cell>(l.GetWorldCells(Cell.Zero, 1));

        // AreEquivalent: same elements, order doesn't matter.
        CollectionAssert.AreEquivalent(
            new[] { new Cell(0, 0, 0), new Cell(0, 0, -1), new Cell(0, 0, -2), new Cell(1, 0, 0) },
            cells);
    }

    [Test]
    public void Constructor_WithoutAnchor_Throws()
    {
        Assert.Throws<ArgumentException>(() => new Footprint(new[] { new Cell(1, 0, 0) }));
    }
}

public class LevelDataTests
{
    LevelData _level;

    [SetUp]
    public void Setup()
    {
        var fake = new FakeFootprints();
        fake.Register("crate", Footprint.Single());
        fake.Register("table", Footprint.Box(2, 1, 1));   // covers (0,0,0) and (1,0,0)
        _level = new LevelData(fake);
    }

    [Test]
    public void Add_MarksAllCellsOfTheFootprint()
    {
        _level.Add(PlacedObject.Create("table", new Cell(3, 0, 3)));

        Assert.IsTrue(_level.IsOccupied(new Cell(3, 0, 3)));
        Assert.IsTrue(_level.IsOccupied(new Cell(4, 0, 3)));
        Assert.AreEqual(2, _level.OccupiedCellCount);
    }

    [Test]
    public void Add_OnOccupiedCell_Throws()
    {
        _level.Add(PlacedObject.Create("crate", new Cell(1, 0, 1)));
        var second = PlacedObject.Create("crate", new Cell(1, 0, 1));

        Assert.Throws<InvalidOperationException>(() => _level.Add(second));
    }

    [Test]
    public void Remove_FreesTheCells()
    {
        var table = PlacedObject.Create("table", Cell.Zero);
        _level.Add(table);

        _level.Remove(table.InstanceId);

        Assert.AreEqual(0, _level.Count);
        Assert.AreEqual(0, _level.OccupiedCellCount);
    }

    [Test]
    public void Move_OverlappingItsOwnOldCells_IsAllowed()
    {
        var table = PlacedObject.Create("table", Cell.Zero);        // (0,0,0) (1,0,0)
        _level.Add(table);

        _level.Move(table.InstanceId, new Cell(1, 0, 0), 0);        // (1,0,0) (2,0,0)

        Assert.IsFalse(_level.IsOccupied(new Cell(0, 0, 0)));
        Assert.IsTrue(_level.IsOccupied(new Cell(2, 0, 0)));
        Assert.AreEqual(2, _level.OccupiedCellCount);
    }

    [Test]
    public void Move_Rotation_UpdatesCells()
    {
        var table = PlacedObject.Create("table", Cell.Zero);
        _level.Add(table);

        _level.Move(table.InstanceId, Cell.Zero, 1);                // (1,0,0) becomes (0,0,-1)

        Assert.IsFalse(_level.IsOccupied(new Cell(1, 0, 0)));
        Assert.IsTrue(_level.IsOccupied(new Cell(0, 0, -1)));
    }

    [Test]
    public void Add_FiresObjectAddedOnce()
    {
        int calls = 0;
        _level.ObjectAdded += _ => calls++;

        _level.Add(PlacedObject.Create("crate", Cell.Zero));

        Assert.AreEqual(1, calls);
    }

    [Test]
    public void CanPlace_UnknownAsset_ThrowsClearError()
    {
        Assert.Throws<KeyNotFoundException>(() => _level.CanPlace("does_not_exist", Cell.Zero, 0));
    }
}