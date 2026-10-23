namespace LevelEditor.Core.Commands
{
    public sealed class PlaceCommand : IEditorCommand
    {
        readonly PlacedObject _snapshot;
        public string Label => $"Add {_snapshot.AssetId}";

        public PlaceCommand(PlacedObject obj)
        {
            if (obj == null) 
                throw new System.ArgumentNullException(nameof(obj));
            _snapshot = obj.Clone();
        }

        public void Do(LevelData level) => level.Add(_snapshot);

        public void Undo(LevelData level) => level.Remove(_snapshot.InstanceId);
    }
}
