using GhostInTheShell.Modules.InfraStructure;

namespace GhostInTheShell.Modules.Shell
{
    public interface IColorable
    {
        void ChangeColor(Hsl hslColor);
        void ChangeDefaultColor();
    }
}
