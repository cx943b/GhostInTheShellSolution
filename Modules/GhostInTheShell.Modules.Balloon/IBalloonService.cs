using System;

namespace GhostInTheShell.Modules.Balloon
{
    public interface IBalloonService
    {
        void AddImage(Uri imageUri);
        void AddText(string text);
    }
}