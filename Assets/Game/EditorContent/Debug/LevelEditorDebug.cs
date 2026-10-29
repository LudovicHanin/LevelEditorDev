using LevelEditor.Core;
using LevelEditor.Runtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// TEMPORARY: drives the editor with the keyboard until real tools exist (step 3).
/// C = crate, T = table, V = move, X = remove, U = undo, Y = redo, R = rebuild view, N = new level.
/// </summary>
public class LevelEditorDebug : MonoBehaviour
{
    [SerializeField] PlaceableCatalog catalog;
    [SerializeField] LevelView view;
    [Tooltip("Random positions are picked in a square of this many cells.")]
    [SerializeField] int areaSize = 10;

    EditorSession _session;
    readonly System.Random _random = new System.Random();

    void Start()
    {
        _session = new EditorSession(catalog);
        view.Bind(_session.Level);
        _session.History.OnChanged += LogHistory;

        Debug.Log("[Debug] C crate | T table | V move | X remove | U undo | Y redo | R rebuild | N new level");
    }

    void OnDestroy()
    {
        if (_session != null)
            _session.History.OnChanged -= LogHistory;
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.cKey.wasPressedThisFrame) PlaceRandom("prop.crate");
        if (keyboard.tKey.wasPressedThisFrame) PlaceRandom("prop.table");
        if (keyboard.vKey.wasPressedThisFrame) MoveRandom();
        if (keyboard.xKey.wasPressedThisFrame) RemoveRandom();
        if (keyboard.uKey.wasPressedThisFrame) _session.Undo();
        if (keyboard.yKey.wasPressedThisFrame) _session.Redo();
        if (keyboard.rKey.wasPressedThisFrame) view.Rebuild();
        if (keyboard.nKey.wasPressedThisFrame) _session.NewLevel();
    }

    // -------------------------------------------------------------------------
    // ACTIONS — each one builds a command and gives it to the session
    // -------------------------------------------------------------------------
    void PlaceRandom(string assetId)
    {
        for (int attempt = 0; attempt < 30; attempt++)
        {
            var cell = RandomCell();
            int rotation = _random.Next(4);
            if (_session.Level.CanPlace(assetId, cell, rotation))
            {
                _session.Execute(new PlaceCommand(PlacedObject.Create(assetId, cell, rotation)));
                return;
            }
        }
        Debug.LogWarning($"[Debug] No free space found for {assetId}.");
    }

    void MoveRandom()
    {
        var obj = PickRandomObject();
        if (obj == null) return;

        for (int attempt = 0; attempt < 30; attempt++)
        {
            var cell = RandomCell();
            int rotation = _random.Next(4);
            var command = new MoveCommand(obj, cell, rotation);
            if (!command.IsNoOp && _session.Level.CanPlace(obj.AssetId, cell, rotation, ignoreId: obj.InstanceId))
            {
                _session.Execute(command);
                return;
            }
        }
        Debug.LogWarning("[Debug] No free space found to move an object.");
    }

    void RemoveRandom()
    {
        var obj = PickRandomObject();
        if (obj != null)
            _session.Execute(new RemoveCommand(obj));
    }

    // -------------------------------------------------------------------------
    // HELPERS
    // -------------------------------------------------------------------------
    Cell RandomCell() => new Cell(_random.Next(areaSize), 0, _random.Next(areaSize));

    PlacedObject PickRandomObject()
    {
        var objects = new List<PlacedObject>(_session.Level.Objects);
        return objects.Count == 0 ? null : objects[_random.Next(objects.Count)];
    }

    void LogHistory()
    {
        var history = _session.History;
        Debug.Log($"[History] {history.Position}/{history.Count} | next undo: {history.UndoLabel ?? "-"} | dirty: {_session.IsDirty}");
    }
}