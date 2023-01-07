namespace GhostInTheShell.Modules.Script.Utils
{
    internal sealed class KorCharBinder
    {
        const string korInitConsonant = "ㄱㄲㄴㄷㄸㄹㅁㅂㅃㅅㅆㅇㅈㅉㅊㅋㅌㅍㅎ";
        const string korMedialConsonant = "ㅏㅐㅑㅒㅓㅔㅕㅖㅗㅘㅙㅚㅛㅜㅝㅞㅟㅠㅡㅢㅣ";
        const string korFinalConsonant = " ㄱㄲㄳㄴㄵㄶㄷㄹㄺㄻㄼㄽㄾㄿㅀㅁㅂㅄㅅㅆㅇㅈㅊㅋㅌㅍㅎ";

        const ushort uniKorStartIndex = 0xAC00;
        const ushort uniKorEndIndex = 0xD79F;

        static readonly KorCharBinder _Instance = new KorCharBinder();
        public static KorCharBinder Instance => _Instance;

        private KorCharBinder() { }

        public char BindChars(char chInit, char chMedial, char chFinal = ' ')
        {
            int initIndex = korInitConsonant.IndexOf(chInit);
            if (initIndex < 0)
                return ' ';

            int medialIndex = korMedialConsonant.IndexOf(chMedial);
            if (medialIndex < 0)
                return ' ';

            int finalIndex = korFinalConsonant.IndexOf(chFinal);
            if (finalIndex < 0)
                return ' ';

            int uniIndex = uniKorStartIndex + (initIndex * 21 + medialIndex) * 28 + finalIndex;
            return Convert.ToChar(uniIndex);
        }
        public char[]? UnbindChar(char chKor)
        {
            if (chKor < uniKorStartIndex || chKor > uniKorEndIndex)
                return null;

            int korIndex = chKor - uniKorStartIndex;
            int i = 21 * 28;

            int initIndex = korIndex / i;
            korIndex %= i;
            int medialIndex = korIndex / 28;
            korIndex %= 28;

            if (korIndex > 0)
                return new char[] { korInitConsonant[initIndex], korMedialConsonant[medialIndex], korFinalConsonant[korIndex] };
            else
                return new char[] { korInitConsonant[initIndex], korMedialConsonant[medialIndex] };
        }
    }
}