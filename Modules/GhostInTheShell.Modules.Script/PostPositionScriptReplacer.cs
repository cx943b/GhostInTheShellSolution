using GhostInTheShell.Modules.Script.Utils;
using System.Text.RegularExpressions;

namespace GhostInTheShell.Modules.Script
{
    public sealed class PostPositionScriptReplacer : ScriptReplacerBase
    {
        public PostPositionScriptReplacer()
            : base("\\\\p\\[\\s*(.+?)\\s*;\\s*(.+?)\\s*\\]")
        {

        }

        protected override string HandleMatchEvaluator(Match match)
        {
            if (match.Index == 0)
                return match.Value;

            char befMatchChar = _lastProcessedScript![match.Index - 1];
            char[]? unbindedKorChars = KorCharBinder.Instance.UnbindChar(befMatchChar);

            if (unbindedKorChars is null)
                return match.Value;

            return match.Groups[4 - unbindedKorChars.Length].Value;
        }
    }
}