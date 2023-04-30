using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace GhostInTheShell.Modules.Balloon.Controls
{
    internal class BalloonContentPanel : Panel
    {
        const int ImageContentVerticalMargin = 6;

        protected override Size ArrangeOverride(Size finalSize)
        {
            double lineMaxHeight = 0;
            Point currentPos = new Point();
            Rect childRect;

            foreach (ContentPresenter child in Children.Cast<ContentPresenter>())
            {
                if (child.Content is IBalloonTextContent)
                {
                    if (currentPos.X + child.DesiredSize.Width > finalSize.Width)
                    {
                        currentPos.X = 0;
                        currentPos.Y += lineMaxHeight;
                    }

                    childRect = new Rect(currentPos, child.DesiredSize);
                    child.Arrange(childRect);

                    currentPos.X += child.DesiredSize.Width;
                    lineMaxHeight = Math.Max(lineMaxHeight, child.DesiredSize.Height);
                }
                else if (child.Content is IBalloonImageContent)
                {
                    currentPos.X = 0;
                    currentPos.Y += lineMaxHeight + ImageContentVerticalMargin;

                    childRect = new Rect(currentPos, new Size(finalSize.Width, child.DesiredSize.Height));
                    child.Arrange(childRect);

                    currentPos.Y += child.DesiredSize.Height + ImageContentVerticalMargin;
                    lineMaxHeight = 0;
                }
                else
                {
                    throw new InvalidCastException("Child not BalloonContent");
                }
            }

            return finalSize;
        }
        protected override Size MeasureOverride(Size availableSize)
        {
            double lineMaxHeight = 0;
            Point currentPos = new Point();

            foreach (ContentPresenter child in Children.Cast<ContentPresenter>())
            {
                child.Measure(availableSize);

                if (child.Content is IBalloonTextContent)
                {
                    if (currentPos.X + child.DesiredSize.Width > availableSize.Width)
                    {
                        currentPos.X = 0;
                        currentPos.Y += lineMaxHeight;
                    }

                    currentPos.X += child.DesiredSize.Width;
                    lineMaxHeight = Math.Max(lineMaxHeight, child.DesiredSize.Height);
                }
                else if (child.Content is IBalloonImageContent)
                {
                    currentPos.X = 0;
                    currentPos.Y += lineMaxHeight + ImageContentVerticalMargin;

                    currentPos.Y += child.DesiredSize.Height + ImageContentVerticalMargin;
                    lineMaxHeight = 0;
                }
                else
                {
                    throw new InvalidCastException("Child not BalloonContent");
                }
            }

            return new Size(availableSize.Width, currentPos.Y + lineMaxHeight);
        }
    }
}