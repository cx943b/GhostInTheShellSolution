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
    public abstract class ShellEventArgs : EventArgs
    {
        public string ShellName { get; init; }

        public ShellEventArgs(string shellName)
        {
            ShellName = shellName ?? throw new ArgumentNullException(nameof(shellName));
        }
    }
    public class ShellPositionChangedEventArgs : ShellEventArgs
    {
        public Point CurrentPosition { get; set; }
        public Point ChangedLength { get; set; }

        public ShellPositionChangedEventArgs(string shellName) : base(shellName) { }
    }
    public class MaterialCollectionChangedEventArgs : ShellEventArgs
    {
        public MemoryStream Stream { get; init; }

        public MaterialCollectionChangedEventArgs(string shellName, MemoryStream stream) : base(shellName)
        {
            Stream = stream ?? throw new ArgumentNullException(nameof(stream));
        }
    }


    public class ShellPositionChangedEvent : PubSubEvent<ShellPositionChangedEventArgs>
    {

    }
    public class ShellSizeChangedEvent : PubSubEvent<Size?>
    {

    }
    public class MaterialCollectionChangedEvent : PubSubEvent<MaterialCollectionChangedEventArgs>
    {

    }
}
