using System;

namespace LevelEditor.Core
{
    public sealed class EditorSession
    {
        public LevelData Level { get; }

        public CommandHistory History { get; }

        public EditorSession(IFootprintProvider footprints)
        {
            if (footprints == null) 
                throw new ArgumentNullException(nameof(footprints));

            Level = new LevelData(footprints);
            History = new CommandHistory();
        }

        public bool CanUndo => History.CanUndo;
        public bool CanRedo => History.CanRedo;
        public bool IsDirty => History.IsDirty;

        public void Execute(IEditorCommand command) => History.Push(command, Level);

        public void Undo() => History.Undo(Level);
        public void Redo() => History.Redo(Level);

        public void MarkSaved() => History.MarkSaved();

        public void NewLevel()
        {
            Level.Clear();
            History.Clear();
        }
    }
}