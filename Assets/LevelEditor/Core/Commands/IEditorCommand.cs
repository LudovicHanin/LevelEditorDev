namespace LevelEditor.Core
{
    public interface IEditorCommand
    {
        public string Label { get; }
        public void Do(LevelData level);
        public void Undo(LevelData level);
    }
}
