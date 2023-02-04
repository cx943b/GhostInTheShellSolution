using System.Text.RegularExpressions;
using GhostInTheShell.Modules.Script.Utils;

namespace GhostInTheShell.Modules.Script
{
    public abstract class ScriptReplacerBase
    {
        protected readonly Regex _replaceRegex;
        protected string? _lastProcessedScript = String.Empty;
        
        public ScriptReplacerBase(Regex replaceRegex)
        {
            if(replaceRegex is null)
                throw new ArgumentNullException(nameof(replaceRegex));

            _replaceRegex = replaceRegex;
        }
        public ScriptReplacerBase(string replaceRegexStr)
            : this(new Regex(replaceRegexStr))
        {
        }

        public string Replace(string script)
        {
            _lastProcessedScript = null;

            if (String.IsNullOrEmpty(script))
                throw new ArgumentNullException(nameof(script));

            _lastProcessedScript = script;
            return _replaceRegex.Replace(script, HandleMatchEvaluator);
        }

        protected abstract string HandleMatchEvaluator(Match match);
    }
}