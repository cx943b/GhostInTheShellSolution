using System.Data;

namespace GhostInTheShell.Modules.Script
{
    public interface IScriptCommand
    {
        ScriptCommandType CommandType { get; }
    }

    public abstract class ScriptCommandBase : IScriptCommand
    {
        public ScriptCommandType CommandType { get; init; }

        public ScriptCommandBase(ScriptCommandType commandType)
        {
            CommandType = commandType;
        }
    }

    public sealed class PrintWordScriptCommand : ScriptCommandBase
    {
        public string Word { get; init; }

        public PrintWordScriptCommand(string word) : base(ScriptCommandType.PrintWord) => Word = word;
    }
    public sealed class ClearWordsScriptCommand : ScriptCommandBase
    {
        public const char ParseTargetChar = 'c';

        public ClearWordsScriptCommand() : base(ScriptCommandType.ClearWords) { }
    }

    public sealed class WaitScriptCommand : ScriptCommandBase
    {
        public int Interval { get; init; }

        public WaitScriptCommand(int interval) : base(ScriptCommandType.Wait) => Interval = interval;
    }

    public sealed class TalkerChangeScriptCommand : ScriptCommandBase
    {
        public int TalkerIndex { get; init; }

        public TalkerChangeScriptCommand(int talkerIndex) : base(ScriptCommandType.ChangeTalker) => TalkerIndex = talkerIndex;
    }

    public sealed class ShellChangeScriptCommand : ScriptCommandBase
    {
        public int ImageIndex { get; init; }

        public ShellChangeScriptCommand(int imgIndex) : base(ScriptCommandType.ChangeShell)
        {
            ImageIndex = imgIndex;
        }
    }
    //public sealed class ShellChangeScriptCommand : ScriptCommandBase
    //{
    //    public string HeadLabel { get; init; }
    //    public string EyeLabel { get; init; }
    //    public string FaceLabel { get; init; }

    //    public ShellChangeScriptCommand(string headLabel, string eyeLabel, string faceLabel) : base(ScriptCommandType.ChangeShell)
    //    {
    //        FaceLabel = faceLabel;
    //        EyeLabel = eyeLabel;
    //        HeadLabel = headLabel;
    //    }
    //}
}