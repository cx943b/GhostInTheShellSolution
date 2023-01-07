using System.Text.RegularExpressions;

namespace GhostInTheShell.Modules.Script
{
    enum PostPositionLabel { FirstPost = 3, SecondPost = 2 }

    internal sealed class PostPositionProcessor : IScriptFilterProcessor
    {
        static Regex _chkRegex = new Regex(@"\\p\[(.+?)\]");
        const string R = $"\\p\\[\\s*(?<{nameof(PostPositionLabel.FirstPost)}>.w+?)\\s*;\\s*(?<{nameof(PostPositionLabel.SecondPost)}>.w+?)\\s*\\]";

        private PostPositionProcessor() { }

        

        public string Process(string script)
        {
            if(script == null)
                throw new ArgumentNullException(nameof(script));

            // \p[을;를] or \p[이;가] or \p[은;는] etc...
            MatchCollection matches = Regex.Matches(script, $"\\p\\[\\s*(?<{nameof(PostPositionLabel.FirstPost)}>.w+?)\\s*;\\s*(?<{nameof(PostPositionLabel.SecondPost)}>.w+?)\\s*\\]");
            char postChar;
            char[]? unbindedPostChars = null;
            bool isKor = false;

            foreach (Match match in matches.Reverse())
            {
                postChar = script[match.Index];
                isKor = CharacterLanguageAnalyzer.Instance.CheckAccord(postChar, CharactorLanguageLabel.Kor);

                if (!isKor)
                    continue;

                unbindedPostChars = KorCharParser.Instance.UnbindChar(postChar);

                // ToDo: Use MatchReplace?
                script = script
                    .Remove(match.Index, match.Length)
                    .Insert(match.Index, match.Groups[$"{(PostPositionLabel)unbindedPostChars.Length}"].Value);
            }

            return script;
        }

        //public static string StartProcess(string srcString)
        //{
        //    MatchCollection mc = _chkRegex.Matches(srcString);

        //    if (mc.Count > 0)
        //    {
        //        string[] postPositions = null;
        //        string buffStr = "", targetPostPosition = "";

        //        int lastCaret = 0, targetCaret = 0;
        //        uint chAddress = 0;

        //        bool isKorChecked = false, isEngChecked = false;
        //        StringBuilder sb = new StringBuilder();

        //        for (int matchIndex = 0; matchIndex < mc.Count; matchIndex++)
        //        {
        //            postPositions = mc[matchIndex].Groups[1].Value.Split(';');
        //            targetCaret = mc[matchIndex].Index + 1;
        //            targetPostPosition = postPositions[1];

        //            do
        //            {
        //                targetCaret -= 1;
        //                chAddress = (uint)char.ToLower(srcString[targetCaret]);

        //                isKorChecked = chAddress >= _startAddress && chAddress <= _endAddress;
        //                isEngChecked = chAddress > 96 && chAddress < 123;

        //            } while ((!isKorChecked && !isEngChecked) && targetCaret > lastCaret);

        //            if (isKorChecked)
        //            {
        //                uint baseAddress = chAddress - _startAddress;

        //                if (baseAddress % 28 != 0)
        //                    targetPostPosition = postPositions[0];
        //            }
        //            else
        //                targetPostPosition = string.Format("({0};{1})", postPositions[0], postPositions[1]);

        //            buffStr = srcString.Substring(lastCaret, mc[matchIndex].Index - lastCaret);
        //            buffStr += targetPostPosition;

        //            sb.Append(buffStr);

        //            lastCaret = mc[matchIndex].Index + mc[matchIndex].Value.Length;
        //        }

        //        buffStr = srcString.Substring(lastCaret, srcString.Length - lastCaret);
        //        sb.Append(buffStr);

        //        return sb.ToString();
        //    }
        //    else
        //        return srcString;
        //}
    }

    
}