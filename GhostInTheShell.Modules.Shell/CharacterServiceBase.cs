using GhostInTheShell.Modules.InfraStructure;
using GhostInTheShell.Modules.Shell.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.TextFormatting;
using System.Xml;

namespace GhostInTheShell.Modules.Shell
{
    public interface ICharacterService
    {
        Task<bool> AddAccessory(ShellPartType accessoryType, string newAccessoryLabel);
        Task<bool> ChangeCloth(string newClothLabel);
        Task<bool> ChangeUnderwear(string newUnderwearLabel);
        Task<bool> InitializeAsync(string shellName);
        bool RemoveAccessory(ShellPartType accessoryType, string oldAccessoryLabel);
    }

    public class CharacterService : ICharacter, ICharacterService
    {
        const string TableRootSectionName = "ShellData:Dav:TableRoot";

        readonly ILogger _logger;
        readonly IConfiguration _Config;
        readonly IShellModelFactory _modelFac;
        readonly IShellMaterialFactory _matFac;

        readonly HttpClient _Client;
        readonly string? _tableRoot;

        IDictionary<ShellPartType, ShellModelBase> _dicShellPartModel = new Dictionary<ShellPartType, ShellModelBase>();
        IDictionary<ShellPartType, List<ShellModelBase>> _dicAccessoryModels = new Dictionary<ShellPartType, List<ShellModelBase>>();

        IDictionary<ShellPartType, Hsl> _dicShellPartColor = new Dictionary<ShellPartType, Hsl>();


        public string? ShellName { get; private set; }
        public Size ShellSize { get; private set; }

        public CharacterService(ILogger logger, IConfiguration config, IShellModelFactory modelFac, IShellMaterialFactory matFac, HttpClient client)
        {
            _logger = logger;
            _Config = config;

            _modelFac = modelFac;
            _matFac = matFac;
            _Client = client;

            _tableRoot = _Config.GetSection(TableRootSectionName)?.Value ?? throw new KeyNotFoundException("TableRootSectionName");
        }

        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<bool> ChangeCloth(string newClothLabel)
        {
            if (String.IsNullOrEmpty(ShellName)) throw new InvalidOperationException("NotInitialize");
            if (String.IsNullOrEmpty(newClothLabel)) throw new ArgumentNullException(nameof(newClothLabel));

            if (_dicShellPartModel.ContainsKey(ShellPartType.Cloth))
            {
                var oldModel = _dicShellPartModel[ShellPartType.Cloth];
                _matFac.UnloadMaterial(oldModel);

                _dicShellPartModel.Remove(ShellPartType.Cloth);
            }

            IEnumerable<string> clothLabels = _modelFac.GetLabels(ShellPartType.Cloth);
            if(!clothLabels.Contains(newClothLabel))
            {
                _logger.Log(LogLevel.Warning, $"NotFound: {newClothLabel}");
                return false;
            }

            var newModel = _modelFac.GetModels(ShellPartType.Cloth, newClothLabel).FirstOrDefault();
            if(newModel is null)
            {
                _logger.Log(LogLevel.Warning, $"NotFound: {newModel}");
                return false;
            }

            bool isMaterialReady = await _matFac.LoadMaterial(ShellName, newModel);
            if(!isMaterialReady)
            {
                _logger.Log(LogLevel.Error, $"ErrorLoadMaterial: {newModel}");
                return false;
            }

            if(_dicShellPartColor.TryGetValue(ShellPartType.Cloth, out Hsl? hslColor))
                newModel.ChangeColor(hslColor);

            _dicShellPartModel.Add(ShellPartType.Cloth, newModel);
            return true;
        }
        public async Task<bool> ChangeUnderwear(string newUnderwearLabel)
        {
            if (String.IsNullOrEmpty(ShellName)) throw new InvalidOperationException("NotInitialize");
            if (String.IsNullOrEmpty(newUnderwearLabel)) throw new ArgumentNullException(nameof(newUnderwearLabel));

            if (_dicShellPartModel.ContainsKey(ShellPartType.Underwear))
            {
                var oldModel = _dicShellPartModel[ShellPartType.Underwear];
                _matFac.UnloadMaterial(oldModel);

                _dicShellPartModel.Remove(ShellPartType.Underwear);
            }

            IEnumerable<string> underwearLabels = _modelFac.GetLabels(ShellPartType.Underwear);
            if (!underwearLabels.Contains(newUnderwearLabel))
            {
                _logger.Log(LogLevel.Warning, $"NotFound: {newUnderwearLabel}");
                return false;
            }

            var newModel = _modelFac.GetModels(ShellPartType.Underwear, newUnderwearLabel).FirstOrDefault();
            if (newModel is null)
            {
                _logger.Log(LogLevel.Warning, $"NotFound: {newModel}");
                return false;
            }

            bool isMaterialReady = await _matFac.LoadMaterial(ShellName, newModel);
            if (!isMaterialReady)
            {
                _logger.Log(LogLevel.Error, $"ErrorLoadMaterial: {newModel}");
                return false;
            }

            //if (_dicShellPartColor.TryGetValue(ShellPartType.Underwear, out Hsl? hslColor))
            //    newModel.ChangeColor(hslColor);

            _dicShellPartModel.Add(ShellPartType.Underwear, newModel);
            return true;
        }

        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public async Task<bool> AddAccessory(ShellPartType accessoryType, string newAccessoryLabel)
        {
            if (String.IsNullOrEmpty(ShellName)) throw new InvalidOperationException("NotInitialize");
            if (String.IsNullOrEmpty(newAccessoryLabel)) throw new ArgumentNullException(nameof(newAccessoryLabel));
            if (!ShellPartType.Accessory.HasFlag(accessoryType)) throw new ArgumentException($"NotAccessoryType: {accessoryType}");

            if (_dicAccessoryModels.ContainsKey(accessoryType) && _dicAccessoryModels[accessoryType].Any(m => String.Compare(m.Label, newAccessoryLabel, true) == 0))
            {
                _logger.Log(LogLevel.Warning, $"AddAccessory-AlreadyExist: {newAccessoryLabel}");
                return false;
            }

            IEnumerable<string> accessoryLabels = _modelFac.GetLabels(accessoryType);
            if (!accessoryLabels.Contains(newAccessoryLabel))
            {
                _logger.Log(LogLevel.Warning, $"AddAccessory-NotFound: {newAccessoryLabel}");
                return false;
            }

            var newModel = _modelFac.GetModels(accessoryType, newAccessoryLabel).FirstOrDefault();
            if (newModel is null)
            {
                _logger.Log(LogLevel.Warning, $"AddAccessory-NotFound: {newModel}");
                return false;
            }

            bool isMaterialReady = await _matFac.LoadMaterial(ShellName, newModel);
            if (!isMaterialReady)
            {
                _logger.Log(LogLevel.Error, $"AddAccessory-ErrorLoadMaterial: {newModel}");
                return false;
            }

            if (_dicShellPartColor.TryGetValue(accessoryType, out Hsl? hslColor))
                newModel.ChangeColor(hslColor);

            if (!_dicAccessoryModels.ContainsKey(accessoryType))
                _dicAccessoryModels.Add(accessoryType, new List<ShellModelBase>());

            _dicAccessoryModels[accessoryType].Add(newModel);
            return true;
        }

        public bool RemoveAccessory(ShellPartType accessoryType, string oldAccessoryLabel)
        {
            if(_dicAccessoryModels.ContainsKey(accessoryType))
            {
                var oldModel = _dicAccessoryModels[accessoryType].FirstOrDefault(m => String.Compare(m.Label, oldAccessoryLabel, true) == 0);
                if(oldModel == null)
                {
                    _logger.Log(LogLevel.Warning, $"RemoveAccessory-NotFound: {oldAccessoryLabel}");
                    return false;
                }
                return _dicAccessoryModels[accessoryType].Remove(oldModel);
            }

            return false;
        }


        public async Task<bool> InitializeAsync(string shellName)
        {
            if (string.IsNullOrEmpty(shellName))
            {
                _logger.Log(LogLevel.Critical, new ArgumentNullException(nameof(shellName)), "InvalidShellName");
                return false;
            }

            string initTablePath = $"{_tableRoot}{shellName}/InitializeTable.xml";

            HttpResponseMessage? resMsg = null;
            Stream? xmlStream = null;
            XmlReader? xmlReader = null;

            try
            {
                resMsg = await _Client.GetAsync(initTablePath);
                if(resMsg.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.Log(LogLevel.Critical, $"{initTablePath} FileNotFound");
                    return false;
                }

                xmlStream = await resMsg.Content.ReadAsStreamAsync();

                XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
                xmlReaderSettings.IgnoreComments = true;

                xmlReader = XmlReader.Create(xmlStream, xmlReaderSettings);
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlReader);

                XmlNode? sizeNode = xmlDoc.DocumentElement!.SelectSingleNode("/ShellInitializeInfo/ShellSize");
                if(sizeNode == null)
                {
                    _logger.Log(LogLevel.Critical, new XmlException("NotFound TargetNode"), "NotFound ShellInitializeInfo/ShellSize");
                    return false;
                }

                IDictionary<string, string>? dicAttr = sizeNode.Attributes?.ToDictionary();
                if(dicAttr == null)
                {
                    _logger.Log(LogLevel.Critical, new XmlException("Empty attributes"), "Empty attributes in ShellInitializeInfo/ShellSize");
                    return false;
                }

                if (!dicAttr.ContainsKey("Width") || !dicAttr.ContainsKey("Height"))
                {
                    _logger.Log(LogLevel.Critical, new XmlException("NotFound TargetAttribute"), "NotFound Width and Height attributes in ShellInitializeInfo/ShellSize");
                    return false;
                }
                
                if(Int32.TryParse(dicAttr["Width"], out int width) && Int32.TryParse(dicAttr["Height"], out int height))
                {
                    bool isModelFactoryReady = await _modelFac.InitializeAsync(shellName);
                    if (isModelFactoryReady)
                    {
                        ShellName = shellName;
                        ShellSize = new Size(width, height);

                        return true;
                    }
                }
                else
                {
                    _logger.Log(LogLevel.Critical, new XmlException("InvalidValue TargetAttribute"), "InvalidValue Width and Height");
                }

                return false;
            }
            catch(XmlException xmlEx)
            {
                _logger.Log(LogLevel.Critical, xmlEx, "Problem in XmlString");
                return false;
            }
            catch(Exception ex)
            {
                _logger.Log(LogLevel.Critical, ex, "NotHandled Error");
                return false;
            }
            finally
            {
                xmlReader?.Dispose();
                xmlStream?.Dispose();
                resMsg?.Dispose();
            }
        }


        public IEnumerable<IMaterialModel> GetMaterials()
        {
            var partModelMaterials = _dicShellPartModel.Values.SelectMany(m => m.GetMaterials());
            var accessoryModelMaterials = _dicAccessoryModels.Values.SelectMany(m => m).SelectMany(m => m.GetMaterials());

            return partModelMaterials.Concat(accessoryModelMaterials);
                        ;
        }
    }
}
