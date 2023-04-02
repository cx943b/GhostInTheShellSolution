using GhostInTheShell.Modules.ShellInfra;
using Microsoft.AspNetCore.Mvc.Rendering;
using Prism.Events;
using System.Drawing;
using System.Drawing.Imaging;

namespace GhostInTheShell.Servers.Shell.Services
{
    public interface ICharacterLocalService : ICharacterService, ICharacter
    {
        Task<(byte[]?, string)> GetCharacterImage(string headLabel, string eyeLabel, string faceLabel);
    }
    public class CharacterLocalService : CharacterServiceBase, ICharacterLocalService
    {
        const string TableRootSectionName = "Shell:Local:TableRoot";
        //readonly string _saveFileRootPath = "D:\\Roots\\ServiceRoot\\gRPC\\SavedImages";

        readonly ILogger _logger;
        readonly IShellModelFactory _modelFac;
        readonly string _tableRoot;
        readonly IDictionary<ShellPartType, IEnumerable<string>>? _dicLabels = new Dictionary<ShellPartType, IEnumerable<string>>();

        //readonly IDictionary<string, IList<string>> _dicSavedFileNames = new Dictionary<string, IList<string>>();


        public CharacterLocalService(ILogger<CharacterLocalService> logger, IEventAggregator eventAggregator, IConfiguration config, IShellModelFactory modelFac, IShellMaterialFactory matFac)
            : base(logger, eventAggregator, config, modelFac, matFac)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _modelFac = modelFac;
            _tableRoot = config?.GetSection(TableRootSectionName).Value ?? throw new KeyNotFoundException(TableRootSectionName);

            _dicLabels = new ShellPartType[] { ShellPartType.Head, ShellPartType.Eye, ShellPartType.Face }
                    .Select(pt => new KeyValuePair<ShellPartType, IEnumerable<string>>(pt, _modelFac.GetLabels(pt)))
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            //Directory.CreateDirectory(_saveFileRootPath);
        }

        public override async Task<bool> InitializeAsync(string shellName)
        {
            try
            {
                (IDictionary<ShellPartType, string> dicPart, IEnumerable<AccessoryAddPair> accPairs) = await base.InitializeBaseAsync(shellName);

                Task[] tasks = new Task[]
                {
                    ChangeParts(dicPart),
                    AddAccessories(accPairs)
                };

                await Task.WhenAll(tasks);
                return true;
            }
            catch(AggregateException aggrEx)
            {
                string logMsg = aggrEx.InnerExceptions.Aggregate("", (msg, ex) => msg += ex.Message + "\r\n");
                _logger.Log(LogLevel.Error, logMsg);
                return false;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.Message);
                return false;
            }

        }

        public async Task<(byte[]?, string)> GetCharacterImage(string headLabel, string eyeLabel, string faceLabel)
        {
            if(String.IsNullOrEmpty(headLabel))
            {
                string msg = $"NullArg: {nameof(headLabel)}";
                _logger.Log(LogLevel.Warning, msg);

                return (null, msg);
            }
            if (String.IsNullOrEmpty(eyeLabel))
            {
                string msg = $"NullArg: {nameof(eyeLabel)}";
                _logger.Log(LogLevel.Warning, msg);

                return (null, msg);
            }
            if (String.IsNullOrEmpty(faceLabel))
            {
                string msg = $"NullArg: {nameof(faceLabel)}";
                _logger.Log(LogLevel.Warning, msg);

                return (null, msg);
            }


            if (!_dicLabels[ShellPartType.Head].Contains(headLabel))
            {
                string msg = "NotExistLabel: Head";
                _logger.Log(LogLevel.Warning, msg);
                
                return (null, msg);
            }
            if(!_dicLabels[ShellPartType.Eye].Contains(eyeLabel))
            {
                string msg = "NotExistLabel: Eye";
                _logger.Log(LogLevel.Warning, msg);

                return (null, msg);
            }
            if (!_dicLabels[ShellPartType.Face].Contains(faceLabel))
            {
                string msg = "NotExistLabel: Face";
                _logger.Log(LogLevel.Warning, msg);

                return (null, msg);
            }

            string saveKey = String.Join('-', headLabel, eyeLabel, faceLabel);
            MemoryStream? overlapedStream = null;
            MemoryStream? retStream = null;

            try
            {
                byte[]? overlapedBytes = null;
                //string? saveFileName = base.GetConcatedHeadPartFileName();
                //if (String.IsNullOrEmpty(saveFileName))
                //{
                //    _logger.Log(LogLevel.Error, $"NullRef: {nameof(saveFileName)}");
                //    return null;
                //}


                // ToDo: Cache.
                //if (_dicSavedFileNames.ContainsKey(saveKey))
                //{

                //}

                var dicParts = new Dictionary<ShellPartType, string>()
                    {
                        {ShellPartType.Head, headLabel },
                        {ShellPartType.Eye, eyeLabel },
                        {ShellPartType.Face, faceLabel }
                    };

                await base.ChangeParts(dicParts);
                overlapedBytes = base.GetOverlapedImage();

                if (overlapedBytes == null)
                {
                    _logger.Log(LogLevel.Error, $"NullRef: {nameof(overlapedBytes)}");
                    return (null, "FailOverlap");
                }

                overlapedStream = new MemoryStream(overlapedBytes);

                retStream = new MemoryStream();// new FileStream(_saveFileRootPath + $"\\{saveFileName}.png", FileMode.Create, FileAccess.ReadWrite);

                var bitmap = (Bitmap)Bitmap.FromStream(overlapedStream);
                bitmap.Save(retStream, ImageFormat.Png);

                byte[] retBytes = new byte[retStream.Length];
                retStream.Position = 0;
                retStream.Read(retBytes);

                //if (!_dicSavedFileNames.ContainsKey(saveKey))
                //    _dicSavedFileNames.Add(saveKey, new List<string>());

                //_dicSavedFileNames[saveKey].Add(saveFileName);

                return (retBytes, "");
            }
            catch(Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.Message);
                return (null, "UnhandledError in server");

            }
            finally
            {
                retStream?.Dispose();
                overlapedStream?.Dispose();
            }
        }

        protected override async Task<Stream?> ReadTableStreamAsync(string shellName)
        {
            string initTablePath = $"{_tableRoot}{shellName}/InitializeTable.xml";

            try
            {
                if (!File.Exists(initTablePath))
                {
                    _logger.Log(LogLevel.Critical, $"NotFound: {initTablePath}");
                    return null;
                }

                Stream xmlStream = File.OpenRead(initTablePath);
                return await Task.FromResult(xmlStream);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Critical, ex.Message);
                return null;
            }
        }


        //protected override async Task<Stream?> ReadTableStreamAsync(string shellName)
        //{
        //    string initTablePath = $"{_tableRoot}{shellName}/InitializeTable.xml";

        //    HttpResponseMessage? resMsg = null;
        //    Stream? xmlStream = null;

        //    try
        //    {
        //        resMsg = await _client.GetAsync(initTablePath);
        //        if (resMsg.StatusCode == System.Net.HttpStatusCode.NotFound)
        //        {
        //            _logger.Log(LogLevel.Error, $"NotFound: {initTablePath}");
        //            return null;
        //        }

        //        xmlStream = await resMsg.Content.ReadAsStreamAsync();

        //        MemoryStream tableStream = new MemoryStream();
        //        await xmlStream.CopyToAsync(tableStream);

        //        tableStream.Position = 0;
        //        return tableStream;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Log(LogLevel.Critical, ex.Message);
        //        return null;
        //    }
        //    finally
        //    {
        //        xmlStream?.Dispose();
        //        resMsg?.Dispose();
        //    }
        //}
    }
}
