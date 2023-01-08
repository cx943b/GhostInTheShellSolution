namespace GhostInTheShell.Modules.Script.Utils
{
    public enum CharactorLanguageLabel { Kor, Eng, Jpn, Kan, Etc }
    internal sealed class CharacterLanguageAnalyzer
    {
        const int korFirst = 44032;
        const int korLast = 55279;

        const int jpnFirst = 12353;
        const int jpnLast = 12538;

        const int chnFirst = 13179;
        const int chnLast = 40907;
        const int chnExFirst = 63744;
        const int chnExLast = 64109;

        const int ltnFirst = 192;
        const int ltnLast = 696;

        const int engFirst = 65;
        const int engLast = 122;

        static readonly CharacterLanguageAnalyzer _Instance = new CharacterLanguageAnalyzer();

        public static CharacterLanguageAnalyzer Instance => _Instance;

        private CharacterLanguageAnalyzer() { }


        public bool CheckAccord(char ch, CharactorLanguageLabel langLabel) => AnalysisWord(ch) == langLabel;
        public CharactorLanguageLabel AnalysisWord(char ch)
        {
            CharactorLanguageLabel label = CharactorLanguageLabel.Etc;
            if (isKor(ch))
                label = CharactorLanguageLabel.Kor;
            else if (isEng(ch))
                label = CharactorLanguageLabel.Eng;
            else if (isHj(ch))
                label = CharactorLanguageLabel.Kan;
            else if (isJpn(ch))
                label = CharactorLanguageLabel.Jpn;

            return label;
        }

        private bool isKor(char ch) => ch >= korFirst && ch <= korLast;
        private bool isJpn(char ch) => ch >= jpnFirst && ch <= jpnLast;
        private bool isEng(char ch) => ch >= engFirst && ch <= engLast;
        private bool isHj(char ch) => ch >= chnFirst && ch <= chnLast || ch >= chnExFirst && ch <= chnExLast;
        private bool isLtn(char ch) => ch >= ltnFirst && ch <= ltnLast;
    }
}