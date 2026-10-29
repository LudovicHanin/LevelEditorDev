using NUnit.Framework;
using LevelEditor.Core;

public class EditorSessionTests
{
    EditorSession _session;

    [SetUp]
    public void Setup()
    {
        var fake = new FakeFootprints();
        fake.Register("crate", Footprint.Single());
        _session = new EditorSession(fake);
    }

    [Test]
    public void Execute_ModifiesLevelAndCanBeUndone()
    {
        _session.Execute(new PlaceCommand(PlacedObject.Create("crate", Cell.Zero)));
        Assert.AreEqual(1, _session.Level.Count);
        Assert.IsTrue(_session.CanUndo);

        _session.Undo();
        Assert.AreEqual(0, _session.Level.Count);

        _session.Redo();
        Assert.AreEqual(1, _session.Level.Count);
    }

    [Test]
    public void NewLevel_EmptiesLevelAndHistory()
    {
        _session.Execute(new PlaceCommand(PlacedObject.Create("crate", Cell.Zero)));

        _session.NewLevel();

        Assert.AreEqual(0, _session.Level.Count);
        Assert.IsFalse(_session.CanUndo);
        Assert.IsFalse(_session.IsDirty);
    }

    [Test]
    public void NewLevel_KeepsTheSameLevelInstance()
    {
        var before = _session.Level;
        _session.NewLevel();
        Assert.AreSame(before, _session.Level, "Listeners subscribed to the old level would stop working.");
    }

    [Test]
    public void NewLevel_FiresObjectRemovedForEachObject()
    {
        _session.Execute(new PlaceCommand(PlacedObject.Create("crate", new Cell(0, 0, 0))));
        _session.Execute(new PlaceCommand(PlacedObject.Create("crate", new Cell(1, 0, 0))));
        int removed = 0;
        _session.Level.ObjectRemoved += _ => removed++;

        _session.NewLevel();

        Assert.AreEqual(2, removed);
    }
}