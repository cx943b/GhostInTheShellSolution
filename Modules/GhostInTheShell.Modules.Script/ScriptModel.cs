using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GhostInTheShell.Modules.Script
{
    internal class ScriptModel
    {
        readonly List<ScriptCommandBase> _lstCommand = new List<ScriptCommandBase>();

        public ScriptModel() { }

        public ScriptModel(IEnumerable<ScriptCommandBase> commands)
        {
            AddCommands(commands);
        }

        public void AddCommand(ScriptCommandBase command)
        {
            if(command == null) throw new ArgumentNullException(nameof(command));

            _lstCommand.Add(command);
        }
        public void AddCommands(IEnumerable<ScriptCommandBase> commands)
        {
            if (commands == null) throw new ArgumentNullException(nameof(commands));

            _lstCommand.AddRange(commands);
        }
    }
}
