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
    public class BalloonTextContentAddedEventArgs : GhostEventBase
    {
        public string Text { get; init; }
        public BalloonTextContentAddedEventArgs(string ballName, string text) : base(ballName)
        {
            if (String.IsNullOrEmpty(text))
                throw new ArgumentNullException(nameof(text));

            Text = text;
        }
    }
    public class BalloonImageContentAddedEventArgs : GhostEventBase
    {
        public Uri ImageUri { get; init; }
        public Size ImageSize { get; init; }
        public HorizontalAlignment HorizontalAlignment { get; init; } = HorizontalAlignment.Center;

        public BalloonImageContentAddedEventArgs(string ballName, Uri imgUri, Size imgSize) : base(ballName)
        {
            if (imgUri is null)
                throw new ArgumentNullException(nameof(imgUri));

            ImageUri = imgUri;
            ImageSize = imgSize;
        }

        public BalloonImageContentAddedEventArgs(string ballName, Uri imgUri, Size imgSize, HorizontalAlignment hAlighment) : this(ballName, imgUri, imgSize)
        {
            HorizontalAlignment = hAlighment;
        }
    }
    internal class BalloonClearedEventArgs : GhostEventBase
    {
        public BalloonClearedEventArgs(string ballName) : base(ballName)
        {
        }
    }


    internal class BalloonTailDirectionChangedEvent : PubSubEvent<BalloonTailDirectionChangeEventArgs> { }
    internal class BalloonPositionChangeEvent : PubSubEvent<BalloonPositionChangeEventArgs> { }
    internal class BalloonTextContentAddedEvent : PubSubEvent<BalloonTextContentAddedEventArgs> { }
    internal class BalloonImageContentAddedEvent : PubSubEvent<BalloonImageContentAddedEventArgs> { }
    internal class BalloonClearedEvent : PubSubEvent<BalloonClearedEventArgs> { }
}
