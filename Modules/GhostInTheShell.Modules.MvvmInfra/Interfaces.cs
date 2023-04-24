using GhostInTheShell.Modules.InfraStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GhostInTheShell.Modules.MvvmInfra
{
    public interface IGhostIdentifier
    {
        string Identifier { get; }
        event GhostIdentifierChangedEventHandler IdentifierChanged;
    }
}
