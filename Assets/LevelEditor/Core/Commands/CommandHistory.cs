using System;
using System.Collections.Generic;

namespace LevelEditor.Core
{
    public sealed class CommandHistory
    {
        public event Action OnChanged;

        private List<IEditorCommand> _commands = new List<IEditorCommand>();
        private int _position;
        private int _savedPosition;

        public bool CanUndo => _position > 0;
        public bool CanRedo => _position < _commands.Count;
        public bool IsDirty => _position != _savedPosition;

        public int Count => _commands.Count;
        public int Position => _position;
        public string UndoLabel => CanUndo ? _commands[_position - 1].Label : null;
        public string RedoLabel => CanRedo ? _commands[_position].Label : null;

        public void Push(IEditorCommand command, LevelData level)
        {
            if (command == null) 
                throw new System.ArgumentNullException(nameof(command));
            RequireLevel(level);

            command.Do(level);
            
            if(_position < _commands.Count)
            { 
                if(_savedPosition > _position)
                    _savedPosition = -1;
                
                _commands.RemoveRange(_position, _commands.Count - _position); 
            }


            _commands.Add(command);
            _position++;

            OnChanged?.Invoke();
        }

        public void Undo(LevelData level)
        {
            RequireLevel(level);
            if (!CanUndo) 
                return;
            _commands[_position-1].Undo(level);
            _position--;
            OnChanged?.Invoke();
        }

        public void Redo(LevelData level)
        {
            RequireLevel(level);
            if (!CanRedo) 
                return;
            _commands[_position].Do(level);
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
            _commands.Clear();
            _position = 0;
            _savedPosition = 0;
            OnChanged?.Invoke();
        }

        public string GetLabel(int index)
        {
            if (index < 0 || index >= _commands.Count)
                throw new System.ArgumentOutOfRangeException(nameof(index));

            return _commands[index].Label;
        }

        static void RequireLevel(LevelData level)
        {
            if (level == null) throw new ArgumentNullException(nameof(level));
        }
    }
}
