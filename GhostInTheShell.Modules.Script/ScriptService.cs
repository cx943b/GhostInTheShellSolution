using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace GhostInTheShell.Modules.Script
{
    public interface IScriptService
    {
        void Execute(string script, bool isAsync = true);
    }

    

    public sealed class ScriptService
    {
        readonly ILogger<ScriptService> _logger;

        readonly PostPositionScriptReplacer _ppScriptReplacer = new PostPositionScriptReplacer();
        readonly Queue<ScriptCommandBase> _quParsedScriptCommand = new Queue<ScriptCommandBase>();

        public int ScriptCommandProcessInterval { get; private set; } = 30;


        public event PrintWordsScriptCommandEventHandler? PrintWordsScriptCommandRequested;



        public ScriptService(ILogger<ScriptService> logger)
        {
            _logger = logger;
        }

        public void Execute(string script, bool isAsync = true)
        {
            string postPositionReplacedScript = _ppScriptReplacer.Replace(script);
            bool isScriptQueueReady = parseScript(postPositionReplacedScript);

            if(isScriptQueueReady)
            {
                if(isAsync)
                    Task.Run(isAsync ? beginPumpScriptQueue : pumpScriptQueue);
            }
        }

        private void beginPumpScriptQueue()
        {
            while(_quParsedScriptCommand.TryDequeue(out ScriptCommandBase? scriptCmd))
            {
                switch(scriptCmd)
                {
                    case WaitScriptCommand waitScriptCmd:
                        {
                            Thread.Sleep(waitScriptCmd.Interval);
                            break;
                        }
                    case TalkerChangeScriptCommand talkerChangeScriptCmd:
                        {
                            // Raise TalkerChangeEvent
                            break;
                        }
                    case PrintWordScriptCommand printWordScriptCmd:
                        {
                            Task.Run(() => PrintWordsScriptCommandRequested?.Invoke(this, new PrintWordsScriptCommandEventArgs(printWordScriptCmd.Word)));
                            
                            break;
                        }

                }

                Thread.Sleep(ScriptCommandProcessInterval);
            }
        }
        private void pumpScriptQueue()
        {
            while (_quParsedScriptCommand.TryDequeue(out ScriptCommandBase? scriptCmd))
            {
                switch (scriptCmd)
                {
                    case WaitScriptCommand waitScriptCmd:
                        {
                            Thread.Sleep(waitScriptCmd.Interval);
                            break;
                        }
                    case TalkerChangeScriptCommand talkerChangeScriptCmd:
                        {
                            // Raise TalkerChangeEvent
                            break;
                        }
                    case PrintWordScriptCommand printWordScriptCmd:
                        {
                            PrintWordsScriptCommandRequested?.Invoke(this, new PrintWordsScriptCommandEventArgs(printWordScriptCmd.Word));

                            break;
                        }

                }

                Thread.Sleep(ScriptCommandProcessInterval);
            }
        }

        private bool parseScript(string script)
        {
            try
            {
                string paramStr = "";
                int currentIndex = 0;

                for (int scriptIndex = 0; scriptIndex < script.Length; ++scriptIndex)
                {
                    if (script[scriptIndex] == '\\')
                    {
                        char chCmd = script[++scriptIndex];
                        bool isScriptCommandChar = Enum.TryParse(((int)chCmd).ToString(), out ScriptCommandChar cmdChar);

                        if (!isScriptCommandChar)
                            continue;

                        switch (cmdChar)
                        {
                            case ScriptCommandChar.ClearWords:
                                {
                                    _quParsedScriptCommand.Enqueue(new ClearWordsScriptCommand());
                                    break;
                                }
                            case ScriptCommandChar.ChangeTalker:
                                {
                                    (paramStr, currentIndex) = catchScriptCommandParameterString(script, scriptIndex);

                                    if(Int32.TryParse(paramStr, out int talkerIndex))
                                    {
                                        _quParsedScriptCommand.Enqueue(new TalkerChangeScriptCommand(talkerIndex));
                                        scriptIndex = currentIndex;
                                    }
                                    else
                                    {
                                        throw new ArgumentException("Could't Get TalkerIndex");
                                    }
                                    
                                    break;
                                }
                            case ScriptCommandChar.Wait:
                                {
                                    (paramStr, currentIndex) = catchScriptCommandParameterString(script, scriptIndex);

                                    if (Int32.TryParse(paramStr, out int waitInterval))
                                    {
                                        _quParsedScriptCommand.Enqueue(new WaitScriptCommand(waitInterval));
                                        scriptIndex = currentIndex;
                                    }
                                    else
                                    {
                                        throw new ArgumentException("Could't Get WaitInterval");
                                    }

                                    break;
                                }
                        }
                    }
                    else
                    {
                        _quParsedScriptCommand.Enqueue(new PrintWordScriptCommand(script[scriptIndex].ToString()));
                    }
                }
            }
            catch(Exception ex)
            {
                return false;
            }
            
            _logger.LogTrace($"QueuedCount: {_quParsedScriptCommand.Count}");

            return true;
        }

        private (string parameterStr, int currentIndex) catchScriptCommandParameterString(string script, int startIndex)
        {
            const char openChar = '[';
            const char closeChar = ']';

            int openIndex = script.IndexOf(openChar, startIndex);
            int closeIndex = script.IndexOf(closeChar, openIndex + 1);

            return (script.Substring(openIndex + 1, closeIndex - openIndex - 1), closeIndex);
        }
    }
}
