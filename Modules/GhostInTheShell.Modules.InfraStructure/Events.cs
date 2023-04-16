using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GhostInTheShell.Modules.InfraStructure
{
    public abstract class GhostEventBase : EventArgs
    {
        public string Identifier { get; init; }

        public GhostEventBase(string identifier) => Identifier = identifier;
    }



    public delegate void GhostIdentifierChangedEventHandler(object sender, GhostIdentifierChangedEventArgs e);
    public class GhostIdentifierChangedEventArgs : EventArgs
    {
        public string Identifier { get; init; }
        public GhostIdentifierChangedEventArgs(string identifier)
        {
            Identifier = identifier;
        }
    }
}
