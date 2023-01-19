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
        bool ChangeAccessoryColor(ShellPartType partType, Hsl hslColor);
        Task<bool> ChangeCloth(string newClothLabel);
        void ChangeDefaultAccessoryColor(ShellPartType partType);
        void ChangeDefaultPartColor(ShellPartType partType);
        Task<bool> ChangeEye(string newFaceLabel);
        Task<bool> ChangeFace(string newFaceLabel);
        Task<bool> ChangeHead(string newHeadLabel);
        bool ChangePartColor(ShellPartType partType, Hsl hslColor);
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

        readonly Random _rand = new Random();
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
        public async Task<bool> ChangeCloth(string label)
        {
            ShellPartType partType = ShellPartType.Cloth;
            ShellModelBase? newModel = await changePart(partType, label);

            if (newModel is not null)
            {
                if (_dicShellPartColor.TryGetValue(partType, out Hsl? hslColor))
                    newModel.ChangeColor(hslColor);

                return true;
            }

            return false;
        }
        public async Task<bool> ChangeUnderwear(string label)
        {
            ShellPartType partType = ShellPartType.Underwear;
            ShellModelBase? newModel = await changePart(partType, label);

            return newModel != null;
        }

        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public async Task<bool> AddAccessory(ShellPartType accessoryType, string label)
        {
            if (String.IsNullOrEmpty(ShellName)) throw new InvalidOperationException("NotInitialize");
            if (String.IsNullOrEmpty(label)) throw new ArgumentNullException(nameof(label));
            if (!ShellPartType.Accessory.HasFlag(accessoryType)) throw new ArgumentException($"NotAccessoryType: {accessoryType}");

            if (_dicAccessoryModels.ContainsKey(accessoryType) && _dicAccessoryModels[accessoryType].Any(m => String.Compare(m.Label, label, true) == 0))
            {
                _logger.Log(LogLevel.Warning, $"AddAccessory-AlreadyExist: {label}");
                return false;
            }

            IEnumerable<string> accessoryLabels = _modelFac.GetLabels(accessoryType);
            if (!accessoryLabels.Contains(label))
            {
                _logger.Log(LogLevel.Warning, $"AddAccessory-NotFound: {label}");
                return false;
            }

            var newModel = _modelFac.GetModels(accessoryType, label).FirstOrDefault();
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

        public bool RemoveAccessory(ShellPartType accessoryType, string label)
        {
            if(_dicAccessoryModels.ContainsKey(accessoryType))
            {
                var oldModel = _dicAccessoryModels[accessoryType].FirstOrDefault(m => String.Compare(m.Label, label, true) == 0);
                if(oldModel == null)
                {
                    _logger.Log(LogLevel.Warning, $"RemoveAccessory-NotFound: {label}");
                    return false;
                }
                return _dicAccessoryModels[accessoryType].Remove(oldModel);
            }

            return false;
        }

        public async Task<bool> ChangeHead(string label)
        {
            ShellModelBase? newModel = await changePart(ShellPartType.Head, label);
            return newModel != null;
        }
        public async Task<bool> ChangeFace(string label)
        {
            ShellPartType partType = ShellPartType.Face;
            ShellModelBase? newModel = await changePart(partType, label);

            if(newModel is not null)
            {
                if (_dicShellPartColor.TryGetValue(partType, out Hsl? hslColor))
                    newModel.ChangeColor(hslColor);

                ((FaceModel)newModel).IsMouthMakeup = true;
                return true;
            }

            return false;
        }
        public async Task<bool> ChangeEye(string label)
        {
            ShellPartType partType = ShellPartType.Eye;
            ShellModelBase? newModel = await changePart(partType, label);

            if (newModel is not null)
            {
                ((EyeModel)newModel).IsEyeMakeup = true;

                if (_dicShellPartColor.TryGetValue(partType, out Hsl? hslColor))
                    newModel.ChangeColor(hslColor);

                return true;
            }

            return false;
        }

        public async Task<bool> ChangeFrontHair(string label)
        {
            ShellPartType partType = ShellPartType.FrontHair;
            ShellModelBase? newModel = await changePart(partType, label);

            if (newModel is not null)
            {
                if (_dicShellPartColor.TryGetValue(partType, out Hsl? hslColor))
                    newModel.ChangeColor(hslColor);

                return true;
            }

            return false;
        }
        public async Task<bool> ChangeBackHair(string label)
        {
            ShellPartType partType = ShellPartType.BackHair;
            ShellModelBase? newModel = await changePart(partType, label);

            if (newModel is not null)
            {
                if (_dicShellPartColor.TryGetValue(partType, out Hsl? hslColor))
                    newModel.ChangeColor(hslColor);

                return true;
            }

            return false;
        }

        public bool ChangePartColor(ShellPartType partType, Hsl hslColor)
        {
            if (!ShellPartType.Accessory.HasFlag(partType))
            {
                if (_dicShellPartColor.ContainsKey(partType))
                    _dicShellPartColor[partType] = hslColor; 
                else
                    _dicShellPartColor.Add(partType, hslColor);

                if (_dicShellPartModel.TryGetValue(partType, out ShellModelBase? modelBase))
                    modelBase.ChangeColor(hslColor);

                return true;
            }
            else
            {
                _logger.Log(LogLevel.Warning, "ChangePartColor-InvalidPartType: Use 'ChangeAccessoryColor'");
                return false;
            }
        }
        public void ChangeDefaultPartColor(ShellPartType partType)
        {
            if (!ShellPartType.Accessory.HasFlag(partType))
            {
                if (_dicShellPartColor.ContainsKey(partType))
                {
                    _dicShellPartColor.Remove(partType);

                    if (_dicShellPartModel.TryGetValue(partType, out ShellModelBase? modelBase))
                        modelBase.ChangeDefaultColor();
                }
            }
            else
            {
                _logger.Log(LogLevel.Warning, "ChangePartColor-InvalidPartType: Use 'ChangeAccessoryColor'");
            }
        }

        public bool ChangeAccessoryColor(ShellPartType partType, Hsl hslColor)
        {
            if (ShellPartType.Accessory.HasFlag(partType))
            {

                if (_dicShellPartColor.ContainsKey(partType))
                    _dicShellPartColor[partType] = hslColor; 
                else
                    _dicShellPartColor.Add(partType, hslColor);

                if (_dicAccessoryModels.TryGetValue(partType, out List<ShellModelBase>? modelBases))
                    modelBases.ForEach(m => m.ChangeColor(hslColor));

                return true;
            }
            else
            {
                _logger.Log(LogLevel.Warning, "ChangeAccessoryColor-InvalidPartType: Use 'ChangePartColor'");
                return false;
            }
        }
        public void ChangeDefaultAccessoryColor(ShellPartType partType)
        {
            if (ShellPartType.Accessory.HasFlag(partType))
            {
                if (_dicShellPartColor.ContainsKey(partType))
                {
                    _dicShellPartColor.Remove(partType);

                    if (_dicAccessoryModels.TryGetValue(partType, out List<ShellModelBase>? modelBases))
                        modelBases.ForEach(m => m.ChangeDefaultColor());
                }
            }
            else
            {
                _logger.Log(LogLevel.Warning, "ChangeDefaultAccessoryColor-InvalidPartType: Use 'ChangeDefaultPartColor'");
            }
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
        }

        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        private async Task<ShellModelBase?> changePart(ShellPartType partType, string newPartLabel)
        {
            if (String.IsNullOrEmpty(ShellName)) throw new InvalidOperationException("NotInitialize");
            if (String.IsNullOrEmpty(newPartLabel)) throw new ArgumentNullException(nameof(newPartLabel));

            if (_dicShellPartModel.ContainsKey(partType))
            {
                var oldModel = _dicShellPartModel[partType];
                _matFac.UnloadMaterial(oldModel);

                _dicShellPartModel.Remove(partType);
            }

            IEnumerable<string> partLabels = _modelFac.GetLabels(partType);
            if (!partLabels.Contains(newPartLabel))
            {
                _logger.Log(LogLevel.Warning, $"NotFound: {newPartLabel}");
                return null;
            }

            var models = _modelFac.GetModels(partType, newPartLabel);
            if (models is null)
            {
                _logger.Log(LogLevel.Warning, $"NotFound: {newPartLabel}");
                return null;
            }

            var newModel = models.ElementAt(_rand.Next(models.Count()));

            // ---
            var eyeModel = newModel as EyeModel;
            if (eyeModel != null) { eyeModel.IsEyeMakeup= true; }

            // ---



            bool isMaterialReady = await _matFac.LoadMaterial(ShellName, newModel);
            if (!isMaterialReady)
            {
                _logger.Log(LogLevel.Error, $"ErrorLoadMaterial: {newModel}");
                return null;
            }

            //if (_dicShellPartColor.TryGetValue(ShellPartType.Underwear, out Hsl? hslColor))
            //    newModel.ChangeColor(hslColor);

            _dicShellPartModel.Add(partType, newModel);
            return newModel;
        }
    }
}
