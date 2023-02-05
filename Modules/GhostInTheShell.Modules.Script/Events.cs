using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GhostInTheShell.Modules.Script
{
    public class ScriptExecuteCompletedEvent : PubSubEvent { }
    public class ClearWordsScriptCommandEvent : PubSubEvent { }
    public class PrintWordScriptCommandEvent : PubSubEvent<PrintWordScriptCommandEventArgs> { }
    public class TalkerChangeScriptCommandEvent : PubSubEvent<TalkerChangeScriptCommandEventArgs> { }
    public class ShellChangeScriptCommandEvent : PubSubEvent<ShellChangeScriptCommandEventArgs> { }

    // --

    public class PrintWordScriptCommandEventArgs : EventArgs
    {
        public string PrintWord { get; init; }

        public PrintWordScriptCommandEventArgs(string printWord) => PrintWord = printWord;
    }
    public class TalkerChangeScriptCommandEventArgs : EventArgs
    {
        public int TalkerIndex { get; init; }

        public TalkerChangeScriptCommandEventArgs(int talkerIndex) => TalkerIndex = talkerIndex;
    }
    public class ShellChangeScriptCommandEventArgs : EventArgs
    {
        public string HeadLabel { get; init; }
        public string EyeLabel { get; init; }
        public string FaceLabel { get; init; }

        public ShellChangeScriptCommandEventArgs(string headLabel, string eyeLabel, string faceLabel)
        {
            HeadLabel = headLabel;
            EyeLabel = eyeLabel;
            FaceLabel = faceLabel;
        }
    }
}