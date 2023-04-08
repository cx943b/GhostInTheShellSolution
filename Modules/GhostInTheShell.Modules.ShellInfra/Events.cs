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
        public Point CurrentPosition { get; init; }
        public Point ChangedLength { get; init; }

        public ShellPositionChangedEventArgs(string shellName, Point currentPosition, Point changedLength) : base(shellName)
        {
            CurrentPosition = currentPosition;
            ChangedLength = changedLength;
        }
    }
    public class MaterialCollectionChangedEventArgs : ShellEventArgs
    {
        public MemoryStream Stream { get; init; }

        public MaterialCollectionChangedEventArgs(string shellName, MemoryStream stream) : base(shellName)
        {
            Stream = stream ?? throw new ArgumentNullException(nameof(stream));
        }
    }
    public class ShellSizeChangedEventArgs : ShellEventArgs
    {
        public Size Size { get; init; }

        public ShellSizeChangedEventArgs(string shellName, Size size) : base(shellName)
        {
            Size = size;
        }
    }


    public class ShellPositionChangedEvent : PubSubEvent<ShellPositionChangedEventArgs> { }
    public class ShellSizeChangedEvent : PubSubEvent<ShellSizeChangedEventArgs> { }
    public class MaterialCollectionChangedEvent : PubSubEvent<MaterialCollectionChangedEventArgs> { }
}
