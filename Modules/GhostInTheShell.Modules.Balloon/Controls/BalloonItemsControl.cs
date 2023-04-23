using GhostInTheShell.Modules.Balloon.Models;
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
    internal class BalloonItemsControlDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? ImageDataTemplate { get; set; }
        public DataTemplate? TextDataTemplate { get; set; }

        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            if(item is null)
                throw new ArgumentNullException(nameof(item));

            if(item is BalloonImageContentModel)
            {
                return ImageDataTemplate;
            }
            else if(item is BalloonTextConetntModel)
            {
                return TextDataTemplate;
            }

            throw new InvalidCastException($"InvalidType: {nameof(item)}");
        }
    }

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
