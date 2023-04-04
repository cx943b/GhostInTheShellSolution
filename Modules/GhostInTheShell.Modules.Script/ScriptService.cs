using GhostInTheShell.Modules.InfraStructure;
using GhostInTheShell.Modules.ShellInfra;
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
        const char ParamStringOpenChar = '[';
        const char ParamStringCloseChar = ']';

        readonly ILogger _logger;
        readonly IShellService _charClientSvc;

        readonly ClearWordsScriptCommandEvent _clearWordsScriptCommandEvent;
        readonly PrintWordScriptCommandEvent _printWordScriptCommandEvent;
        readonly TalkerChangeScriptCommandEvent _talkerChangeScriptCommandEvent;
        readonly ScriptExecuteCompletedEvent _scriptExecuteCompletedEvent;
        readonly ShellChangeScriptCommandEvent _shellChangeScriptCommandEvent;
        readonly MaterialCollectionChangedEvent _materialCollectionChangedEvent;

        readonly IDictionary<int, MemoryStream> _dicImageStream = new Dictionary<int, MemoryStream>();

        readonly PostPositionScriptReplacer _ppScriptReplacer = new PostPositionScriptReplacer();
        readonly Queue<ScriptCommandBase> _quParsedScriptCommand = new Queue<ScriptCommandBase>();

        public bool IsRunning { get; private set; }
        public int ScriptCommandPumpInterval { get; private set; } = 30;

        public ScriptService(ILogger<ScriptService> logger, IEventAggregator eventAggregator, IShellService charClientSvc)
        {
            _logger = logger;
            _charClientSvc = charClientSvc ?? throw new NullReferenceException(nameof(charClientSvc));

            _clearWordsScriptCommandEvent = eventAggregator.GetEvent<ClearWordsScriptCommandEvent>();
            _printWordScriptCommandEvent = eventAggregator.GetEvent<PrintWordScriptCommandEvent>();
            _talkerChangeScriptCommandEvent = eventAggregator.GetEvent<TalkerChangeScriptCommandEvent>();
            _scriptExecuteCompletedEvent = eventAggregator.GetEvent<ScriptExecuteCompletedEvent>();
            _shellChangeScriptCommandEvent = eventAggregator.GetEvent<ShellChangeScriptCommandEvent>();
            _materialCollectionChangedEvent = eventAggregator.GetEvent<MaterialCollectionChangedEvent>();
        }

        public bool ChangeScriptCommandPumpInterval(int interval)
        {
            if (IsRunning)
            {
                _logger.Log(LogLevel.Warning, "Can't change ScriptInterval in PumpingQueue");
                return false;
            }

            if(interval <= 0)
            {
                _logger.Log(LogLevel.Warning, $"OutofRange: {nameof(interval)}");
                return false;
            }

            ScriptCommandPumpInterval = interval;
            return true;
        }

        public async void Execute(string script)
        {
            string postPositionReplacedScript = _ppScriptReplacer.Replace(script);
            bool isScriptQueueReady = await parseScript(postPositionReplacedScript);

            if(!isScriptQueueReady)
            {
                _logger.Log(LogLevel.Warning, $"InvalidScript: {script}");
                return;
            }

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
                    case ShellChangeScriptCommand shellChangeScriptCmd:
                        {
                            _materialCollectionChangedEvent.Publish(_dicImageStream[shellChangeScriptCmd.ImageIndex]);
                            //_shellChangeScriptCommandEvent.Publish(new ShellChangeScriptCommandEventArgs(shellChangeScriptCmd.HeadLabel, shellChangeScriptCmd.EyeLabel, shellChangeScriptCmd.FaceLabel));
                            break;
                        }
                }

                Thread.Sleep(ScriptCommandPumpInterval);
            }

            foreach (Stream stream in _dicImageStream.Values)
                stream.Dispose();

            _dicImageStream.Clear();

            _logger.Log(LogLevel.Trace, "AfterScriptPump: ClearedImageBuffer");

            IsRunning = false;

            _scriptExecuteCompletedEvent.Publish();
        }

        private async Task<bool> parseScript(string script)
        {
            foreach (Stream stream in _dicImageStream.Values)
                stream.Dispose();

            _dicImageStream.Clear();
            
            _logger.Log(LogLevel.Trace, "BeforeScriptParse: ClearedImageBuffer");

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
                            case ScriptCommandChar.ChangeShell:
                                {
                                    (paramStr, currentIndex) = catchScriptCommandParameterString(script, scriptIndex);
                                    if(currentIndex < 0)
                                    {
                                        throw new ArgumentException($"ScriptError-InvalidParams: {ScriptCommandChar.ChangeShell}");
                                    }

                                    string[] charParams = paramStr.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                                    if(charParams.Length == 3)
                                    {
                                        byte[]? imgBytes = await _charClientSvc.RequestShellImageAsync(ShellNames.Kaori, charParams[0], charParams[1], charParams[2]);
                                        if(imgBytes is null)
                                        {
                                            throw new ArgumentException($"ScriptError-InvalidResponses: {ScriptCommandChar.ChangeShell}");
                                        }

                                        int imgIndex = _dicImageStream.Count;

                                        MemoryStream ms = new MemoryStream(imgBytes);
                                        _dicImageStream.Add(imgIndex, ms);

                                        _quParsedScriptCommand.Enqueue(new ShellChangeScriptCommand(imgIndex));
                                        scriptIndex = currentIndex;
                                    }
                                    else
                                    {
                                        throw new ArgumentException($"ScriptError-InvalidParams: {ScriptCommandChar.ChangeShell}");
                                    }
                                    
                                    break;
                                }
                            case ScriptCommandChar.ChangeTalker:
                                {
                                    (paramStr, currentIndex) = catchScriptCommandParameterString(script, scriptIndex);
                                    if (currentIndex < 0)
                                    {
                                        throw new ArgumentException($"ScriptError-InvalidParams: {ScriptCommandChar.ChangeTalker}");
                                    }

                                    if (Int32.TryParse(paramStr, out int talkerIndex))
                                    {
                                        _quParsedScriptCommand.Enqueue(new TalkerChangeScriptCommand(talkerIndex));
                                        scriptIndex = currentIndex;
                                    }
                                    else
                                    {
                                        throw new ArgumentException($"ScriptError-InvalidParams: {ScriptCommandChar.ChangeTalker}");
                                    }
                                    
                                    break;
                                }
                            case ScriptCommandChar.Wait:
                                {
                                    (paramStr, currentIndex) = catchScriptCommandParameterString(script, scriptIndex);
                                    if (currentIndex < 0)
                                    {
                                        throw new ArgumentException($"ScriptError-InvalidParams: {ScriptCommandChar.Wait}");
                                    }

                                    if (Int32.TryParse(paramStr, out int waitInterval))
                                    {
                                        _quParsedScriptCommand.Enqueue(new WaitScriptCommand(waitInterval));
                                        scriptIndex = currentIndex;
                                    }
                                    else
                                    {
                                        throw new ArgumentException($"ScriptError-InvalidParams: {ScriptCommandChar.Wait}");
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

                //var taskResults = Task.WhenAll(lstTask.ToArray()).ConfigureAwait(false).GetAwaiter().GetResult();
                //if(taskResults.Any(r => r == null))
                //{
                //    _logger.Log(LogLevel.Warning, $"NullReferences in ShellResponse");
                //    return false;
                //}

                //foreach ((byte[] imgBytes, int index) in taskResults.Select((r, i) => (r!, i)))
                //{
                //    MemoryStream imgStream = new MemoryStream(imgBytes);
                //    _dicImageStream.Add(index, imgStream);
                //}

                _logger.Log(LogLevel.Trace, $"TotalCachedImageBytes: {_dicImageStream.Values.Sum(m => m.Length):n0} bytes");

                _logger.Log(LogLevel.Trace, $"QueuedCount: {_quParsedScriptCommand.Count}");

                return true;
            }
            catch(Exception ex)
            {
                _logger.Log(LogLevel.Warning, ex.Message);

                foreach (var imgStream in _dicImageStream.Values)
                    imgStream.Dispose();

                _dicImageStream.Clear();
                _logger.Log(LogLevel.Trace, "ErrorInScriptParse: ClearedImageBuffer");

                return false;
            }
        }

        private (string parameterStr, int currentIndex) catchScriptCommandParameterString(string script, int startIndex)
        {
            int openIndex = script.IndexOf(ParamStringOpenChar, startIndex);
            if(openIndex < 0)
            {
                _logger.Log(LogLevel.Warning, "NotFound: OpenBraketIndex");
                return (String.Empty, -1);
            }

            int closeIndex = script.IndexOf(ParamStringCloseChar, openIndex + 1);
            if (closeIndex < 0)
            {
                _logger.Log(LogLevel.Warning, "NotFound: CloseBraketIndex");
                return (String.Empty, -1);
            }

            return (script.Substring(openIndex + 1, closeIndex - openIndex - 1), closeIndex);
        }
    }
}
