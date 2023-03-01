using GhostInTheShell.Modules.Script;
using GhostInTheShell.Modules.Script.Utils;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GhostInTheShell.Tests
{
    [TestClass]
    public class ScriptTests
    {
        [TestMethod]
        public void Parse()
        {
            const string script = "\\t[0]이쁜 여자\\p[은;는] 흔하지만\\w[1000] \\t[1]이쁜 남자\\p[은;는] 귀하다구";

            EventAggregator eventAggregator = new EventAggregator();
            eventAggregator.GetEvent<PrintWordScriptCommandEvent>().Subscribe(e => Debug.Write(e.PrintWord));

            //ScriptService scriptSvc = new ScriptService(LoggerMockFactory.CreateLogger<ScriptService>(), eventAggregator);
            //scriptSvc.Execute(script);

            Thread.Sleep(2000);
        }

        [TestMethod]
        public void UnbindKorChar()
        {
            KorCharBinder kcBinder = KorCharBinder.Instance;

            // --
            char[]? unbindedKorChars = kcBinder.UnbindChar(' ');

            Assert.IsNull(unbindedKorChars);
            // --

            // --
            unbindedKorChars = kcBinder.UnbindChar('갮');

            Assert.IsNotNull(unbindedKorChars);
            Assert.IsTrue(unbindedKorChars.SequenceEqual(new char[] { 'ㄱ', 'ㅐ', 'ㅄ' }));
            // --

            // --
            unbindedKorChars = kcBinder.UnbindChar('냐');

            Assert.IsNotNull(unbindedKorChars);
            Assert.IsTrue(unbindedKorChars.SequenceEqual(new char[] { 'ㄴ', 'ㅑ' }));
            // --
        }

        [TestMethod]
        public void PostPositionReplace()
        {
            PostPositionScriptReplacer comp = new PostPositionScriptReplacer();

            // ---
            string script = "남궁루리\\p[은;는] 사실 나코루루\\p[이;가] 아닐까?";
            string processedScript = comp.Replace(script);
            
            Assert.IsNotNull(processedScript);
            Debug.WriteLine(processedScript);

            Assert.IsTrue(String.Compare(processedScript, "남궁루리는 사실 나코루루가 아닐까?") == 0);
            // ---

            // ---
            script = "밤의 공원\\p[은;는] 무서워, 특히 물\\p[이;가] 있는 곳\\p[은;는]";
            processedScript = comp.Replace(script);

            Assert.IsNotNull(processedScript);
            Debug.WriteLine(processedScript);

            Assert.IsTrue(String.Compare(processedScript, "밤의 공원은 무서워, 특히 물이 있는 곳은") == 0);
            // ---

            // ---
            script = "인형\\p[이;가] 이렇게 이쁜데 악마\\p[이;가] 깃들면 또 어때요? 그건 그거대\\p[으로;로] 별미죠";
            processedScript = comp.Replace(script);

            Assert.IsNotNull(processedScript);
            Debug.WriteLine(processedScript);

            Assert.IsTrue(String.Compare(processedScript, "인형이 이렇게 이쁜데 악마가 깃들면 또 어때요? 그건 그거대로 별미죠") == 0);
            // ---
        }
    }
}