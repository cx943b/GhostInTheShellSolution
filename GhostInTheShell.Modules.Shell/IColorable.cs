using GhostInTheShell.Modules.InfraStructure;

namespace GhostInTheShell.Modules.Shell
{
    public interface IColorable
    {
        bool ChangeColor(Hsl hslColor);
        void ChangeDefaultColor();
    }
}
