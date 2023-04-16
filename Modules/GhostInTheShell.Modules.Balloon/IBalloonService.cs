using System;
using System.Windows;

namespace GhostInTheShell.Modules.Balloon
{
    public interface IBalloonService
    {
        void AddImage(string charName, Uri imgUri, Size imgSize);
        void AddText(string charName, string text);
        void ChangePosition(string charName, Point pos);
        void Clear();
    }
}