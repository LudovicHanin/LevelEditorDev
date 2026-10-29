using LevelEditor.Core;
using NUnit.Framework;
using System;

public class CommandTests
{
    LevelData _level;

    [SetUp]
    public void Setup()
    {
        var fake = new FakeFootprints();
        fake.Register("crate", Footprint.Single());       // 1 cell
        fake.Register("table", Footprint.Box(2, 1, 1));   // (0,0,0) and (1,0,0)
        _level = new LevelData(fake);
    }

    /// <summary>Checks that the level holds nothing: no objects AND no occupied cells.</summary>
    void AssertLevelIsEmpty()
    {
        Assert.AreEqual(0, _level.Count, "Objects are still in the level.");
        Assert.AreEqual(0, _level.OccupiedCellCount, "Cells are still marked as occupied.");
    }

    // =========================================================================
    // PLACE COMMAND
    // =========================================================================

    [Test]
    public void Place_Do_AddsObjectAndCells()
    {
        var table = PlacedObject.Create("table", new Cell(3, 0, 3));
        var cmd = new PlaceCommand(table);

        cmd.Do(_level);

        Assert.IsTrue(_level.Contains(table.InstanceId));
        Assert.IsTrue(_level.IsOccupied(new Cell(3, 0, 3)));
        Assert.IsTrue(_level.IsOccupied(new Cell(4, 0, 3)));
    }

    [Test]
    public void Place_Undo_LeavesLevelEmpty()
    {
        var cmd = new PlaceCommand(PlacedObject.Create("table", Cell.Zero));

        cmd.Do(_level);
        cmd.Undo(_level);

        AssertLevelIsEmpty();
    }

    [Test]
    public void Place_Redo_AddsTheSameObjectAgain()
    {
        var crate = PlacedObject.Create("crate", new Cell(1, 0, 1));
        var cmd = new PlaceCommand(crate);

        cmd.Do(_level);
        cmd.Undo(_level);
        cmd.Do(_level);   // redo

        Assert.IsTrue(_level.Contains(crate.InstanceId));
        Assert.IsTrue(_level.IsOccupied(new Cell(1, 0, 1)));
    }

    [Test]
    public void Place_OnOccupiedCell_ThrowsAndChangesNothing()
    {
        _level.Add(PlacedObject.Create("crate", Cell.Zero));
        var cmd = new PlaceCommand(PlacedObject.Create("crate", Cell.Zero));

        Assert.Throws<InvalidOperationException>(() => cmd.Do(_level));

        Assert.AreEqual(1, _level.Count);
        Assert.AreEqual(1, _level.OccupiedCellCount);
    }

    [Test]
    public void Place_Label_NamesTheAsset()
    {
        var cmd = new PlaceCommand(PlacedObject.Create("crate", Cell.Zero));
        Assert.AreEqual("Add crate", cmd.Label);
    }

    // =========================================================================
    // REMOVE COMMAND
    // =========================================================================

    [Test]
    public void Remove_Do_RemovesObjectAndFreesCells()
    {
        var table = PlacedObject.Create("table", Cell.Zero);
        _level.Add(table);

        new RemoveCommand(table).Do(_level);

        AssertLevelIsEmpty();
    }

    [Test]
    public void Remove_Undo_RestoresSameObject()
    {
        var table = PlacedObject.Create("table", new Cell(2, 0, 2), 1);   // (2,0,2) and (2,0,1)
        _level.Add(table);
        var cmd = new RemoveCommand(table);

        cmd.Do(_level);
        cmd.Undo(_level);

        Assert.IsTrue(_level.TryGet(table.InstanceId, out var restored));
        Assert.AreEqual(new Cell(2, 0, 2), restored.Anchor);
        Assert.AreEqual(1, restored.Rotation);
        Assert.IsTrue(_level.IsOccupied(new Cell(2, 0, 1)));
        Assert.AreEqual(2, _level.OccupiedCellCount);
    }

    [Test]
    public void Remove_Redo_RemovesAgain()
    {
        var crate = PlacedObject.Create("crate", Cell.Zero);
        _level.Add(crate);
        var cmd = new RemoveCommand(crate);

        cmd.Do(_level);
        cmd.Undo(_level);
        cmd.Do(_level);   // redo: finds the object by its InstanceId

        AssertLevelIsEmpty();
    }

    [Test]
    public void Remove_Label_NamesTheAsset()
    {
        var crate = PlacedObject.Create("crate", Cell.Zero);
        Assert.AreEqual("Remove crate", new RemoveCommand(crate).Label);
    }

    // =========================================================================
    // MOVE COMMAND
    // =========================================================================

    [Test]
    public void Move_DoThenUndo_RestoresOriginalPosition()
    {
        var crate = PlacedObject.Create("crate", new Cell(1, 0, 1));
        _level.Add(crate);
        var cmd = new MoveCommand(crate, new Cell(5, 0, 5), 0);

        cmd.Do(_level);
        Assert.IsTrue(_level.IsOccupied(new Cell(5, 0, 5)));
        Assert.IsFalse(_level.IsOccupied(new Cell(1, 0, 1)));

        cmd.Undo(_level);
        Assert.IsTrue(_level.IsOccupied(new Cell(1, 0, 1)));
        Assert.IsFalse(_level.IsOccupied(new Cell(5, 0, 5)));
    }

    [Test]
    public void Move_Redo_GoesBackToDestination()
    {
        var crate = PlacedObject.Create("crate", Cell.Zero);
        _level.Add(crate);
        var cmd = new MoveCommand(crate, new Cell(3, 0, 0), 0);

        cmd.Do(_level);
        cmd.Undo(_level);
        cmd.Do(_level);   // redo

        Assert.AreEqual(new Cell(3, 0, 0), crate.Anchor);
    }

    [Test]
    public void Move_RotateInPlace_UndoRestoresRotationAndCells()
    {
        var table = PlacedObject.Create("table", Cell.Zero);   // (0,0,0) (1,0,0)
        _level.Add(table);
        var cmd = new MoveCommand(table, Cell.Zero, 1);         // (0,0,0) (0,0,-1)

        cmd.Do(_level);
        Assert.IsTrue(_level.IsOccupied(new Cell(0, 0, -1)));

        cmd.Undo(_level);
        Assert.AreEqual(0, table.Rotation);
        Assert.IsTrue(_level.IsOccupied(new Cell(1, 0, 0)));
        Assert.IsFalse(_level.IsOccupied(new Cell(0, 0, -1)));
        Assert.AreEqual(2, _level.OccupiedCellCount);
    }

    [Test]
    public void Move_Label_DescribesWhatChanged()
    {
        var crate = PlacedObject.Create("crate", Cell.Zero);
        Assert.AreEqual("Rotate crate", new MoveCommand(crate, Cell.Zero, 1).Label);
        Assert.AreEqual("Move crate", new MoveCommand(crate, new Cell(1, 0, 0), 0).Label);
        Assert.AreEqual("Move and rotate crate", new MoveCommand(crate, new Cell(1, 0, 0), 2).Label);
    }

    [Test]
    public void Move_SamePositionAndRotation_IsNoOp()
    {
        var crate = PlacedObject.Create("crate", new Cell(2, 0, 2), 1);
        Assert.IsTrue(new MoveCommand(crate, new Cell(2, 0, 2), 5).IsNoOp);    // 5 normalizes to 1
        Assert.IsFalse(new MoveCommand(crate, new Cell(2, 0, 3), 1).IsNoOp);
    }

    // =========================================================================
    // COMPOSITE COMMAND
    // =========================================================================

    [Test]
    public void Composite_Do_RunsAllCommands()
    {
        var composite = new CompositeCommand("Three crates", new IEditorCommand[]
        {
            new PlaceCommand(PlacedObject.Create("crate", new Cell(0, 0, 0))),
            new PlaceCommand(PlacedObject.Create("crate", new Cell(1, 0, 0))),
            new PlaceCommand(PlacedObject.Create("crate", new Cell(2, 0, 0))),
        });

        composite.Do(_level);

        Assert.AreEqual(3, _level.Count);
        Assert.AreEqual(3, composite.Count);
    }

    [Test]
    public void Composite_Undo_RunsInReverseOrder()
    {
        var crate = PlacedObject.Create("crate", Cell.Zero);
        var composite = new CompositeCommand("Place and move", new IEditorCommand[]
        {
            new PlaceCommand(crate),
            new MoveCommand(crate, new Cell(1, 0, 0), 0),
        });

        composite.Do(_level);
        Assert.IsTrue(_level.IsOccupied(new Cell(1, 0, 0)));

        // In the wrong order, this would throw: the crate would be removed before being moved back.
        Assert.DoesNotThrow(() => composite.Undo(_level));
        AssertLevelIsEmpty();
    }

    [Test]
    public void Composite_Redo_WorksAfterUndo()
    {
        var composite = new CompositeCommand("Two crates", new IEditorCommand[]
        {
            new PlaceCommand(PlacedObject.Create("crate", new Cell(0, 0, 0))),
            new PlaceCommand(PlacedObject.Create("crate", new Cell(5, 0, 5))),
        });

        composite.Do(_level);
        composite.Undo(_level);
        composite.Do(_level);   // redo

        Assert.AreEqual(2, _level.Count);
    }

    [Test]
    public void Composite_FailingSubCommand_RollsBackEverything()
    {
        var tableA = PlacedObject.Create("table", new Cell(0, 0, 0));   // (0,0,0) (1,0,0)
        var tableB = PlacedObject.Create("table", new Cell(1, 0, 0));   // (1,0,0) (2,0,0) → overlaps A
        var composite = new CompositeCommand("Two tables", new IEditorCommand[]
        {
            new PlaceCommand(tableA),
            new PlaceCommand(tableB),
        });

        Assert.Throws<InvalidOperationException>(() => composite.Do(_level));

        // All or nothing: table A must not stay behind as a ghost.
        AssertLevelIsEmpty();
    }

    [Test]
    public void Composite_Nested_UndoesEverything()
    {
        var inner = new CompositeCommand("Inner", new IEditorCommand[]
        {
            new PlaceCommand(PlacedObject.Create("crate", new Cell(0, 0, 0))),
            new PlaceCommand(PlacedObject.Create("crate", new Cell(1, 0, 0))),
        });
        var outer = new CompositeCommand("Outer", new IEditorCommand[]
        {
            inner,
            new PlaceCommand(PlacedObject.Create("table", new Cell(0, 0, 5))),
        });

        outer.Do(_level);
        Assert.AreEqual(3, _level.Count);

        outer.Undo(_level);
        AssertLevelIsEmpty();
    }

    [Test]
    public void Composite_EmptyLabel_UsesDefault()
    {
        var composite = new CompositeCommand("", new IEditorCommand[0]);
        Assert.AreEqual("Multiple changes", composite.Label);
        Assert.IsTrue(composite.IsEmpty);
    }
}