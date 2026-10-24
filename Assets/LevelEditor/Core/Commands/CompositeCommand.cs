using System;
using System.Collections.Generic;

namespace LevelEditor.Core
{
    public sealed class CompositeCommand : IEditorCommand
    {
        readonly IEditorCommand[] _commands;
        public string Label { get; }
        public int Count => _commands.Length;
        public bool IsEmpty => _commands.Length == 0;

        public CompositeCommand(string label, IEnumerable<IEditorCommand> commands)
        {
            if (commands == null) throw new ArgumentNullException(nameof(commands));

            List<IEditorCommand> commandList = new List<IEditorCommand>();
            foreach (IEditorCommand command in commands)
            {
                if (command == null) throw new ArgumentException("Commands collection contains a null command.", nameof(commands));
                commandList.Add(command);
            }

            _commands = commandList.ToArray();
            Label = string.IsNullOrEmpty(label) ? "Multiple changes" : label;
        }

        public void Do(LevelData level)
        {
            int done = 0;
            try
            {
                for (; done < _commands.Length; done++)
                {
                    _commands[done].Do(level);
                }
            }
            catch
            {
                for (int i = done - 1; i >= 0; i--)
                    _commands[i].Undo(level);

                throw;
            }
        }

        public void Undo(LevelData level)
        {
            for (int i = _commands.Length - 1; i >= 0; i--)
                _commands[i].Undo(level);
        }
    }
}
