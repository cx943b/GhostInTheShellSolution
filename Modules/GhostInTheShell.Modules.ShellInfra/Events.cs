using GhostInTheShell.Modules.InfraStructure;
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
    public class ShellPositionChangedEventArgs : GhostEventBase
    {
        public Point CurrentPosition { get; init; }
        public Point ChangedLength { get; init; }

        public ShellPositionChangedEventArgs(string shellName, Point currentPosition, Point changedLength) : base(shellName)
        {
            CurrentPosition = currentPosition;
            ChangedLength = changedLength;
        }
    }
    public class MaterialCollectionChangedEventArgs : GhostEventBase
    {
        public MemoryStream Stream { get; init; }

        public MaterialCollectionChangedEventArgs(string shellName, MemoryStream stream) : base(shellName)
        {
            Stream = stream ?? throw new ArgumentNullException(nameof(stream));
        }
    }
    public class ShellSizeChangedEventArgs : GhostEventBase
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
