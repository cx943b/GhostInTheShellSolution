using Prism.Events;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GhostInTheShell.Modules.ShellInfra
{
    public class ShellSizeChangedEvent : PubSubEvent<Size>
    {

    }
    public class MaterialCollectionChangedEvent : PubSubEvent<MemoryStream>
    {

    }
}
