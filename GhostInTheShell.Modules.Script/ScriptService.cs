using Microsoft.Extensions.Logging;
using Prism.Events;
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
        bool IsRunning { get; }
        int ScriptCommandPumpInterval { get; }

        bool ChangeScriptCommandPumpInterval(int interval);
        void Execute(string script);
    }

    public sealed class ScriptService : IScriptService
    {
        readonly ILogger<ScriptService> _logger;

        readonly ClearWordsScriptCommandEvent _clearWordsScriptCommandEvent;
        readonly PrintWordScriptCommandEvent _printWordScriptCommandEvent;
        readonly TalkerChangeScriptCommandEvent _talkerChangeScriptCommandEvent;
        readonly ScriptExecuteCompletedEvent _scriptExecuteCompletedEvent;

        readonly PostPositionScriptReplacer _ppScriptReplacer = new PostPositionScriptReplacer();
        readonly Queue<ScriptCommandBase> _quParsedScriptCommand = new Queue<ScriptCommandBase>();

        public bool IsRunning { get; private set; }
        public int ScriptCommandPumpInterval { get; private set; } = 30;

        public ScriptService(ILogger<ScriptService> logger, IEventAggregator eventAggregator)
        {
            _logger = logger;

            _clearWordsScriptCommandEvent = eventAggregator.GetEvent<ClearWordsScriptCommandEvent>();
            _printWordScriptCommandEvent = eventAggregator.GetEvent<PrintWordScriptCommandEvent>();
            _talkerChangeScriptCommandEvent = eventAggregator.GetEvent<TalkerChangeScriptCommandEvent>();
            _scriptExecuteCompletedEvent = eventAggregator.GetEvent<ScriptExecuteCompletedEvent>();
        }

        public bool ChangeScriptCommandPumpInterval(int interval)
        {
            if (IsRunning)
                return false;

            if(interval <= 0) throw new ArgumentOutOfRangeException(nameof(interval));

            ScriptCommandPumpInterval = interval;
            return true;
        }

        public void Execute(string script)
        {
            string postPositionReplacedScript = _ppScriptReplacer.Replace(script);
            bool isScriptQueueReady = parseScript(postPositionReplacedScript);

            if(isScriptQueueReady)
                Task.Run(pumpScriptQueue);
        }

        private void pumpScriptQueue()
        {
            IsRunning = true;

            while (_quParsedScriptCommand.TryDequeue(out ScriptCommandBase? scriptCmd))
            {
                switch(scriptCmd)
                {
                    case ClearWordsScriptCommand:
                        {
                            _clearWordsScriptCommandEvent.Publish();
                            break;
                        }
                    case WaitScriptCommand waitScriptCmd:
                        {
                            Thread.Sleep(waitScriptCmd.Interval);
                            break;
                        }
                    case TalkerChangeScriptCommand talkerChangeScriptCmd:
                        {
                            _talkerChangeScriptCommandEvent.Publish(new TalkerChangeScriptCommandEventArgs(talkerChangeScriptCmd.TalkerIndex));
                            break;
                        }
                    case PrintWordScriptCommand printWordScriptCmd:
                        {
                            _printWordScriptCommandEvent.Publish(new PrintWordScriptCommandEventArgs(printWordScriptCmd.Word));                            
                            break;
                        }

                }

                Thread.Sleep(ScriptCommandPumpInterval);
            }

            IsRunning = false;

            _scriptExecuteCompletedEvent.Publish();
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
