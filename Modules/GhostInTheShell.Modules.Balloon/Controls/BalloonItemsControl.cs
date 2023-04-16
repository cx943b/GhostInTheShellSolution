using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace GhostInTheShell.Modules.Balloon.Controls
{

    internal class BalloonContentPresenter : ContentPresenter
    {
        protected override Size MeasureOverride(Size constraint)
        {
            if (Content is null)
                return new Size();

            return base.MeasureOverride(new Size(constraint.Width, Double.PositiveInfinity));
        }
    }
    internal class BalloonItemsControl : ItemsControl
    {
        protected override Size MeasureOverride(Size constraint)
        {
            if (Items.Count == 0)
                return new Size();

            return base.MeasureOverride(new Size(constraint.Width, Double.PositiveInfinity));
        }
    }
}
