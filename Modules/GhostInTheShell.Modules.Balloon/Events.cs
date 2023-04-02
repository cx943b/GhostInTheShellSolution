using GhostInTheShell.Modules.Balloon.Controls;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GhostInTheShell.Modules.Balloon
{
    public class BalloonPositionChangeEventArgs : EventArgs
    {
        public Point Position { get; set; }
    }
    public class BalloonTailDirectionChangeEvent : PubSubEvent<BalloonTailDirection?> { }
    public class BalloonPositionChangeEvent : PubSubEvent<BalloonPositionChangeEventArgs> { }
}
