namespace GhostInTheShell.Modules.Script
{
    public enum CharactorLanguageLabel { Kor, Eng, Jpn, Kan, Etc }
    internal sealed class CharacterLanguageAnalyzer
    {
        int korFirst = 44032;
        int korLast = 55279;

        int jpnFirst = 12353;
        int jpnLast = 12538;

        int chnFirst = 13179;
        int chnLast = 40907;
        int chnExFirst = 63744;
        int chnExLast = 64109;

        int ltnFirst = 192;
        int ltnLast = 696;

        int engFirst = 65;
        int engLast = 122;

        static readonly CharacterLanguageAnalyzer _Instance = new CharacterLanguageAnalyzer();

        public static CharacterLanguageAnalyzer Instance => _Instance;

        private CharacterLanguageAnalyzer() { }


        public  bool CheckAccord(char ch, CharactorLanguageLabel langLabel) => AnalysisWord(ch) == langLabel;
        public  CharactorLanguageLabel AnalysisWord(char ch)
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

        private  bool isKor(char ch) => ch >= korFirst && ch <= korLast;
        private  bool isJpn(char ch) => ch >= jpnFirst && ch <= jpnLast;
        private  bool isEng(char ch) => ch >= engFirst && ch <= engLast;
        private  bool isHj(char ch) => ch >= chnFirst && ch <= chnLast || ch >= chnExFirst && ch <= chnExLast;
        private  bool isLtn(char ch) => ch >= ltnFirst && ch <= ltnLast;
    }
}