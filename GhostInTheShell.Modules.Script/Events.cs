using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GhostInTheShell.Modules.Script
{
    public delegate void PrintWordsScriptCommandEventHandler(object sender, PrintWordsScriptCommandEventArgs e);
    public class PrintWordsScriptCommandEventArgs : EventArgs
    {
        public string PrintWords { get; init; }

        public PrintWordsScriptCommandEventArgs(string printWords)
        {
            PrintWords = printWords;
        }
    }
}
