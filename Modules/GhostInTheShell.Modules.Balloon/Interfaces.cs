using System;
using System.Windows;

namespace GhostInTheShell.Modules.Balloon
{
    public interface IBalloonContent
    {
        Point Position { get; set; }
        double Width { get; set; }
        double Height { get; set; }
        bool IsLastInLine { get; set; }
        double Right { get; }
        double Bottom { get; }
    }
    public interface IBalloonImageContent : IBalloonContent
    {
        Uri? ImageUri { get; set; }
        Size ImageSize { get; set; }
        HorizontalAlignment ImageHorizontalAlignment { get; set; }
    }
    public interface IBalloonTextContent : IBalloonContent
    {
        string? Text { get; set; }
    }
    public interface IBalloonService
    {
        void AddImage(string charName, Uri imgUri, Size imgSize);
        void AddText(string charName, string text);
        void ChangePosition(string charName, Point pos);
        void Clear(string charName);
    }
}