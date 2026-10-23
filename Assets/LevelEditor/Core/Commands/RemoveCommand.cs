namespace LevelEditor.Core.Commands
{
    public sealed class RemoveCommand : IEditorCommand
    {
        readonly PlacedObject _snapshot;

        public string Label => $"Remove {_snapshot.AssetId}";

        public RemoveCommand(PlacedObject obj)
        {
            if (obj == null) 
                throw new System.ArgumentNullException(nameof(obj));
            _snapshot = obj.Clone();
        }

        public void Do(LevelData level) => level.Remove(_snapshot.InstanceId);

        public void Undo(LevelData level) => level.Add(_snapshot);
    }
}
