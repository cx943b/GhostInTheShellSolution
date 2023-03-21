using System;
using System.Windows;

namespace GhostInTheShell.Modules.Balloon
{
    public interface IBalloonService
    {
        void AddImage(Uri imgUri, Size imgSize);
        void AddText(string text);
        void Clear();
    }
}