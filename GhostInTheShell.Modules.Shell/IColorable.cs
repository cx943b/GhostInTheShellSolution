namespace GhostInTheShell.Modules.Shell
{
    public interface IColorable
    {
        void ChangeColor(float h, float s, float l);
        void ChangeDefaultColor();
    }
}
