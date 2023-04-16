using GhostInTheShell.Modules.Balloon.Controls;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GhostInTheShell.Modules.InfraStructure;

namespace GhostInTheShell.Modules.Balloon
{
    internal class BalloonPositionChangeEventArgs : GhostEventBase
    {
        public Point Position { get; init; }

        public BalloonPositionChangeEventArgs(string ballName, Point position) : base(ballName)
        {
            Position = position;
        }
    }
    internal class BalloonTailDirectionChangeEventArgs : GhostEventBase
    {
        public BalloonTailDirection TailDirection { get; init; }

        public BalloonTailDirectionChangeEventArgs(string ballName, BalloonTailDirection tailDirection) : base(ballName)
        {
            TailDirection = tailDirection;
        }
    }


    internal class BalloonTailDirectionChangeEvent : PubSubEvent<BalloonTailDirectionChangeEventArgs> { }
    internal class BalloonPositionChangeEvent : PubSubEvent<BalloonPositionChangeEventArgs> { }
}
