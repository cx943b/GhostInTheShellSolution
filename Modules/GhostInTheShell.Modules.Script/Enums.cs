using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GhostInTheShell.Modules.Script
{
    public enum ScriptCommandChar
    {
        ChangeTalker = (int)'t',
        ClearWords = (int)'c',
        PrintWord = (int)' ',
        ChangeShell = (int)'s',
        Wait = (int)'w'
    }
    public enum ScriptCommandType
    {
        ChangeTalker,
        ClearWords,
        PrintWord,
        ChangeShell,
        Wait
    }
}
