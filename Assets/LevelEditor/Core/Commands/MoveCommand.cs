namespace LevelEditor.Core
{
    public sealed class MoveCommand : IEditorCommand
    {
        readonly string _instanceId;
        readonly string _assetId;

        readonly Cell _fromAnchor;
        readonly Cell _toAnchor;

        readonly int _fromRotation;
        readonly int _toRotation;

        public bool IsNoOp => _fromAnchor == _toAnchor && _fromRotation == _toRotation;


        public string Label
        {
            get
            {
                bool moved = _fromAnchor != _toAnchor;
                bool rotated = _fromRotation != _toRotation;
                if (moved && rotated) return $"Move and rotate {_assetId}";
                if (rotated) return $"Rotate {_assetId}";
                return $"Move {_assetId}";
            }
        }

        public MoveCommand(PlacedObject obj, Cell toAnchor, int toRotation)
        {
            if (obj == null) 
                throw new System.ArgumentNullException(nameof(obj));

            _instanceId = obj.InstanceId;
            _assetId = obj.AssetId;
            _fromAnchor = obj.Anchor;
            _toAnchor = toAnchor;
            _fromRotation = obj.Rotation;
            _toRotation = GridMath.NormalizeRotation(toRotation);
        }

        public void Do(LevelData level) => level.Move(_instanceId, _toAnchor, _toRotation);
        public void Undo(LevelData level) => level.Move(_instanceId, _fromAnchor, _fromRotation);
    }
}