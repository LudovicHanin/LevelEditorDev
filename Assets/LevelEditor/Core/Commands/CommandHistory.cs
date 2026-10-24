using System.Collections.Generic;

namespace LevelEditor.Core
{
    public sealed class CommandHistory
    {
        public event System.Action OnChanged;

        private List<IEditorCommand> _history = new List<IEditorCommand>();
        private int _position = 0;
        private int _savedPosition = 0;

        bool canUndo => _position > 0;
        bool canRedo => _position < _history.Count;
        bool isDirty => _position != _savedPosition;

        public int Count => _history.Count;
        public int Position => _position;
        public string UndoLabel => canUndo ? _history[_position - 1].Label : null;
        public string RedoLabel => canRedo ? _history[_position].Label : null;

        public void Push(IEditorCommand command, LevelData level)
        {
            if (command == null) 
                throw new System.ArgumentNullException(nameof(command));

            command.Do(level);
            
            if(_position < _history.Count)
                _history.RemoveRange(_position, _history.Count - _position);

            if(_savedPosition > _position)
                _savedPosition = -1;

            _history.Add(command);
            _position++;

            OnChanged?.Invoke();
        }

        public void Undo(LevelData level)
        {
            if (!canUndo) 
                return;
            _history[_position-1].Undo(level);
            _position--;
            OnChanged?.Invoke();
        }

        public void Redo(LevelData level)
        {
            if (!canRedo) 
                return;
            _history[_position].Do(level);
            _position++;
            OnChanged?.Invoke();
        }

        public void MarkSaved()
        {
            _savedPosition = _position;
            OnChanged?.Invoke();
        }

        public void Clear()
        {
            _history.Clear();
            _position = 0;
            _savedPosition = 0;
            OnChanged?.Invoke();
        }

        public string GetLabel(int index)
        {
            if (index < 0 || index >= _history.Count)
                throw new System.ArgumentOutOfRangeException(nameof(index));

            return _history[index].Label;
        }
    }
}
