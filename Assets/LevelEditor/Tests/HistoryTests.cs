using System;
using System.Collections.Generic;
using NUnit.Framework;
using LevelEditor.Core;

public class HistoryTests
{
    FakeFootprints _footprints;
    LevelData _level;
    CommandHistory _history;

    [SetUp]
    public void Setup()
    {
        _footprints = new FakeFootprints();
        _footprints.Register("crate", Footprint.Single());       // 1 cell
        _footprints.Register("table", Footprint.Box(2, 1, 1));   // 2 cells
        _level = new LevelData(_footprints);
        _history = new CommandHistory();
    }

    /// <summary>A command that places a crate at (x, 0, 0).</summary>
    static IEditorCommand Crate(int x) => new PlaceCommand(PlacedObject.Create("crate", new Cell(x, 0, 0)));

    // =========================================================================
    // BASICS
    // =========================================================================

    [Test]
    public void NewHistory_IsEmptyAndClean()
    {
        Assert.IsFalse(_history.CanUndo);
        Assert.IsFalse(_history.CanRedo);
        Assert.IsFalse(_history.IsDirty);
        Assert.AreEqual(0, _history.Count);
    }

    [Test]
    public void Push_AppliesAndRecordsTheCommand()
    {
        _history.Push(Crate(0), _level);

        Assert.AreEqual(1, _level.Count);
        Assert.IsTrue(_history.CanUndo);
        Assert.IsTrue(_history.IsDirty);
        Assert.AreEqual(1, _history.Position);
    }

    [Test]
    public void PushThenUndo_RestoresLevelAndIsClean()
    {
        _history.Push(Crate(0), _level);
        _history.Undo(_level);

        Assert.AreEqual(0, _level.Count);
        Assert.IsTrue(_history.CanRedo);
        Assert.IsFalse(_history.IsDirty);
    }

    [Test]
    public void PushUndoRedo_ObjectIsBack()
    {
        _history.Push(Crate(3), _level);
        _history.Undo(_level);
        _history.Redo(_level);

        Assert.IsTrue(_level.IsOccupied(new Cell(3, 0, 0)));
        Assert.IsFalse(_history.CanRedo);
    }

    [Test]
    public void SeveralUndos_RemoveEverythingInOrder()
    {
        _history.Push(Crate(0), _level);
        _history.Push(Crate(1), _level);
        _history.Push(Crate(2), _level);

        _history.Undo(_level);
        _history.Undo(_level);
        _history.Undo(_level);

        Assert.AreEqual(0, _level.Count);
        Assert.AreEqual(0, _history.Position);
        Assert.AreEqual(3, _history.Count);   // still there, ready to be redone
    }

    // =========================================================================
    // EDGE CASES
    // =========================================================================

    [Test]
    public void Undo_OnEmptyHistory_DoesNothing()
    {
        int changes = 0;
        _history.OnChanged += () => changes++;

        Assert.DoesNotThrow(() => _history.Undo(_level));
        Assert.AreEqual(0, changes, "Nothing happened, so Changed should not fire.");
    }

    [Test]
    public void Redo_WithNothingToRedo_DoesNothing()
    {
        _history.Push(Crate(0), _level);

        Assert.DoesNotThrow(() => _history.Redo(_level));
        Assert.AreEqual(1, _level.Count);
    }

    [Test]
    public void PushAfterUndo_DiscardsTheRedoBranch()
    {
        _history.Push(Crate(0), _level);   // A
        _history.Push(Crate(1), _level);   // B
        _history.Undo(_level);             // B undone
        _history.Push(Crate(2), _level);   // C replaces B

        Assert.IsFalse(_history.CanRedo);
        Assert.AreEqual(2, _history.Count);

        _history.Redo(_level);             // must not bring B back
        Assert.IsFalse(_level.IsOccupied(new Cell(1, 0, 0)));
    }

    [Test]
    public void FailedPush_ThrowsAndLeavesHistoryUnchanged()
    {
        _history.Push(Crate(0), _level);
        int changes = 0;
        _history.OnChanged += () => changes++;

        Assert.Throws<InvalidOperationException>(() => _history.Push(Crate(0), _level));   // same cell

        Assert.AreEqual(1, _history.Count);
        Assert.AreEqual(1, _history.Position);
        Assert.AreEqual(1, _level.Count);
        Assert.AreEqual(0, changes);
    }

    [Test]
    public void FailedPush_DoesNotEraseTheRedoBranch()
    {
        _history.Push(Crate(0), _level);
        _history.Push(Crate(1), _level);
        _history.Undo(_level);

        Assert.Throws<InvalidOperationException>(() => _history.Push(Crate(0), _level));

        Assert.IsTrue(_history.CanRedo, "The failed push must not have truncated the history.");
    }

    // =========================================================================
    // DIRTY STATE
    // =========================================================================

    [Test]
    public void MarkSaved_ThenPush_IsDirty_UndoBack_IsClean()
    {
        _history.Push(Crate(0), _level);
        _history.MarkSaved();
        Assert.IsFalse(_history.IsDirty);

        _history.Push(Crate(1), _level);
        Assert.IsTrue(_history.IsDirty);

        _history.Undo(_level);
        Assert.IsFalse(_history.IsDirty, "Back at the saved point: the level matches the file.");
    }

    [Test]
    public void SavedStateErased_StaysDirtyForever()
    {
        _history.Push(Crate(0), _level);   // A
        _history.MarkSaved();              // saved with A
        _history.Undo(_level);             // A undone
        _history.Push(Crate(1), _level);   // B: the "A" state is gone

        Assert.IsTrue(_history.IsDirty);
        _history.Undo(_level);
        Assert.IsTrue(_history.IsDirty, "Empty level is not the saved state (which had A).");
    }

    [Test]
    public void PushAtSavedPoint_KeepsSavedStateReachable()
    {
        _history.Push(Crate(0), _level);
        _history.MarkSaved();
        _history.Push(Crate(1), _level);   // nothing erased: cursor was at the end

        _history.Undo(_level);
        Assert.IsFalse(_history.IsDirty);
    }

    // =========================================================================
    // EVENTS & LABELS
    // =========================================================================

    [Test]
    public void Changed_FiresOncePerOperation()
    {
        int changes = 0;
        _history.OnChanged += () => changes++;

        _history.Push(Crate(0), _level);
        _history.Undo(_level);
        _history.Redo(_level);
        _history.MarkSaved();
        _history.Clear();

        Assert.AreEqual(5, changes);
    }

    [Test]
    public void Labels_FollowTheCursor()
    {
        Assert.IsNull(_history.UndoLabel);

        _history.Push(Crate(0), _level);
        Assert.AreEqual("Add crate", _history.UndoLabel);
        Assert.IsNull(_history.RedoLabel);

        _history.Undo(_level);
        Assert.IsNull(_history.UndoLabel);
        Assert.AreEqual("Add crate", _history.RedoLabel);
    }

    [Test]
    public void GetLabel_OutOfRange_Throws()
    {
        _history.Push(Crate(0), _level);

        Assert.AreEqual("Add crate", _history.GetLabel(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => _history.GetLabel(1));
        Assert.Throws<ArgumentOutOfRangeException>(() => _history.GetLabel(-1));
    }

    // =========================================================================
    // CLEAR
    // =========================================================================

    [Test]
    public void Clear_ResetsHistoryButNotTheLevel()
    {
        _history.Push(Crate(0), _level);
        _history.Push(Crate(1), _level);

        _history.Clear();

        Assert.AreEqual(0, _history.Count);
        Assert.IsFalse(_history.CanUndo);
        Assert.IsFalse(_history.IsDirty);
        Assert.AreEqual(2, _level.Count, "Clearing the history must not touch the level.");
    }

    // =========================================================================
    // STRESS TEST — the final exam of the Core
    // =========================================================================

    [Test]
    public void Regression_RedoPlace_AfterMoveAndRemove_UsesOriginalPosition()
    {
        var crate = PlacedObject.Create("crate", Cell.Zero);
        _history.Push(new PlaceCommand(crate), _level);

        _level.TryGet(crate.InstanceId, out var live);
        _history.Push(new MoveCommand(live, new Cell(5, 0, 5), 0), _level);

        _level.TryGet(crate.InstanceId, out live);
        _history.Push(new RemoveCommand(live), _level);

        while (_history.CanUndo) _history.Undo(_level);

        _history.Redo(_level);   // redo Place only

        Assert.IsTrue(_level.IsOccupied(Cell.Zero), "Redo of Place must restore the ORIGINAL position.");
        Assert.IsFalse(_level.IsOccupied(new Cell(5, 0, 5)));
    }

    [Test]
    public void Stress_RandomCommands_UndoAll_RedoAll()
    {
        var random = new Random(12345);   // fixed seed: same sequence every run, so failures are reproducible
        int pushed = 0;

        for (int step = 0; step < 200; step++)
        {
            var objects = new List<PlacedObject>(_level.Objects);
            int action = objects.Count == 0 ? 0 : random.Next(3);

            if (action == 0)        // PLACE
            {
                string asset = random.Next(2) == 0 ? "crate" : "table";
                var cell = RandomCell(random);
                int rot = random.Next(4);
                if (_level.CanPlace(asset, cell, rot))
                {
                    _history.Push(new PlaceCommand(PlacedObject.Create(asset, cell, rot)), _level);
                    pushed++;
                }
            }
            else if (action == 1)   // MOVE
            {
                var obj = objects[random.Next(objects.Count)];
                var cell = RandomCell(random);
                int rot = random.Next(4);
                var cmd = new MoveCommand(obj, cell, rot);
                if (!cmd.IsNoOp && _level.CanPlace(obj.AssetId, cell, rot, ignoreId: obj.InstanceId))
                {
                    _history.Push(cmd, _level);
                    pushed++;
                }
            }
            else                    // REMOVE
            {
                var obj = objects[random.Next(objects.Count)];
                _history.Push(new RemoveCommand(obj), _level);
                pushed++;
            }

            AssertIndexIsConsistent();
        }

        Assert.Greater(pushed, 100, "The test should actually exercise many commands.");
        var finalState = Snapshot();

        // Undo everything: the level must be perfectly empty.
        while (_history.CanUndo)
        {
            _history.Undo(_level);
            AssertIndexIsConsistent();
        }
        Assert.AreEqual(0, _level.Count);
        Assert.AreEqual(0, _level.OccupiedCellCount);

        // Redo everything: the level must be exactly as before.
        while (_history.CanRedo)
            _history.Redo(_level);
        CollectionAssert.AreEqual(finalState, Snapshot());
    }

    /// <summary>A small 10×10 area, so objects collide often and the edge cases get tested.</summary>
    static Cell RandomCell(Random random) => new Cell(random.Next(10), 0, random.Next(10));

    /// <summary>The index must hold exactly as many cells as the objects' footprints cover.</summary>
    void AssertIndexIsConsistent()
    {
        int expected = 0;
        foreach (var obj in _level.Objects)
            expected += _footprints.GetFootprint(obj.AssetId).Offsets.Count;

        Assert.AreEqual(expected, _level.OccupiedCellCount, "Occupancy index out of sync with objects.");
    }

    /// <summary>A sorted text description of every object: id, type, position, rotation.</summary>
    List<string> Snapshot()
    {
        var lines = new List<string>();
        foreach (var obj in _level.Objects)
            lines.Add(obj.ToString());
        lines.Sort();
        return lines;
    }
}