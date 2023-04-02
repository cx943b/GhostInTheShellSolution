using GhostInTheShell.Modules.InfraStructure;
using GhostInTheShell.Modules.ShellInfra.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace GhostInTheShell.Modules.ShellInfra
{
    public abstract class CharacterServiceBase : XmlTableReaderBase, ICharacter, ICharacterService
    {
        readonly ILogger _logger;
        readonly IConfiguration _Config;
        readonly IShellModelFactory _modelFac;
        readonly IShellMaterialFactory _matFac;

        readonly MaterialCollectionChangedEvent _matCollChangedEvent;
        readonly ShellSizeChangedEvent _shellSizeChangedEvent;

        readonly Random _rand = new Random();
        

        readonly IDictionary<ShellPartType, ShellModelBase> _dicShellPartModel = new Dictionary<ShellPartType, ShellModelBase>();
        readonly IDictionary<ShellPartType, List<ShellModelBase>> _dicAccessoryModels = new Dictionary<ShellPartType, List<ShellModelBase>>();
        readonly IDictionary<ShellPartType, Hsl> _dicShellPartColor = new Dictionary<ShellPartType, Hsl>();


        public string? ShellName { get; private set; }
        public Size ShellSize { get; private set; }

        public CharacterServiceBase(ILogger logger, IEventAggregator eventAggregator, IConfiguration config, IShellModelFactory modelFac, IShellMaterialFactory matFac)
        {
            _logger = logger;
            _Config = config;

            _modelFac = modelFac;
            _matFac = matFac;

            _matCollChangedEvent = eventAggregator.GetEvent<MaterialCollectionChangedEvent>();
            _shellSizeChangedEvent = eventAggregator.GetEvent<ShellSizeChangedEvent>();
        }

        /// <summary>
        /// Call OnMaterialCollectionChanged at last
        /// </summary>
        /// <param name="dicParts"></param>
        /// <param name="dicAccessoryParts"></param>
        /// <returns></returns>
        public async Task ChangeShell(IDictionary<ShellPartType, string>? dicParts, IEnumerable<AccessoryAddPair>? dicAccessoryParts)
        {
            try
            {
                Task taskChangeParts = ChangeParts(dicParts);
                Task taskAddAccessories = AddAccessories(dicAccessoryParts);

                await Task.WhenAll(new Task[] { taskChangeParts, taskAddAccessories });

                OnMaterialCollectionChanged();
            }
            catch(AggregateException aggrEx)
            {
                string concatedMessage = aggrEx.InnerExceptions.Aggregate("", (msg, e) => e.Message + "\r\n\t");
                _logger.Log(LogLevel.Error, concatedMessage);
            }
            catch(Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.Message);
            }
        }
        /// <exception cref="ArgumentNullException"></exception>
        /// /// <exception cref="AggregateException"></exception>
        public async Task ChangeParts(IDictionary<ShellPartType, string>? dicParts)
        {
            if (dicParts is null)
                return;

            Task<(ShellPartType, ShellModelBase)?>[] tasks = dicParts.Select(kvp => changePart(kvp.Key, kvp.Value)).ToArray();
            (ShellPartType, ShellModelBase)?[]? changeResults = await Task.WhenAll(tasks);

            foreach ((ShellPartType partType, ShellModelBase shellModel) changeResult in changeResults.Where(r => r is not null).Cast<(ShellPartType, ShellModelBase)>())
            {
                if (_dicShellPartModel.ContainsKey(changeResult.partType))
                    _dicShellPartModel[changeResult.partType] = changeResult.shellModel;
                else
                    _dicShellPartModel.Add(changeResult.partType, changeResult.shellModel);
            }
        }

        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// /// /// <exception cref="AggregateException"></exception>
        public async Task AddAccessories(IEnumerable<AccessoryAddPair>? dicAccessoryParts)
        {
            if (dicAccessoryParts is null)
                return;
            if (dicAccessoryParts.Any(p => !ShellPartType.Accessory.HasFlag(p.PartType)))
                throw new ArgumentException($"NotAccessoryType is {dicAccessoryParts}");

            Task<(ShellPartType, ShellModelBase)?>[] tasks = dicAccessoryParts.Select(p => changePart(p.PartType, p.PartLabel)).ToArray();
            (ShellPartType, ShellModelBase)?[] changeResults = await Task.WhenAll(tasks);

            foreach ((ShellPartType partType, ShellModelBase shellModel) changeResult in changeResults.Where(r => r is not null).Cast<(ShellPartType, ShellModelBase)>())
            {
                
                if (!_dicAccessoryModels.ContainsKey(changeResult.partType))
                    _dicAccessoryModels.Add(changeResult.partType, new List<ShellModelBase>());

                _dicAccessoryModels[changeResult.partType].Add(changeResult.shellModel);
            }
        }

        public bool RemoveAccessory(ShellPartType accessoryType, string label)
        {
            if (_dicAccessoryModels.ContainsKey(accessoryType))
            {
                var oldModel = _dicAccessoryModels[accessoryType].FirstOrDefault(m => String.Compare(m.Label, label, true) == 0);
                if (oldModel == null)
                {
                    _logger.Log(LogLevel.Warning, $"RemoveAccessory-NotFound: {label}");
                    return false;
                }

                _matFac.UnloadMaterial(oldModel);
                return _dicAccessoryModels[accessoryType].Remove(oldModel);
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


        protected async Task<(IDictionary<ShellPartType, string>, IEnumerable<AccessoryAddPair>)> InitializeBaseAsync(string shellName)
        {
            if (string.IsNullOrEmpty(shellName))
                throw new ArgumentNullException(nameof(shellName));

            Stream? xmlStream = await ReadTableStreamAsync(shellName);
            if(xmlStream is null)
                throw new FileLoadException(nameof(shellName));                

            XmlReader xmlReader = base.CreateTableReader(xmlStream);
            XmlDocument xmlDoc = new XmlDocument();

            try
            {
                xmlDoc.Load(xmlReader);

                if (xmlDoc.DocumentElement is null)
                    throw new XmlException("NullRef: DocumentElement");

                bool isShellSizeReady = initializeShellSize(xmlDoc.DocumentElement);
                if (!isShellSizeReady)
                    throw new InvalidOperationException("NotReady: ShellSize");

                _logger.Log(LogLevel.Information, "ModelFactory Ready");
                var retParam = initializeCharacterParameters(xmlDoc.DocumentElement);
                
                ShellName = shellName;
                return retParam;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.Message);
                throw;
            }
            finally
            {
                xmlReader.Dispose();
                xmlStream.Dispose();
            }
        }

        protected virtual byte[]? GetOverlapedImage()
        {
            var partModelMaterials = _dicShellPartModel.Values.SelectMany(m => m.GetMaterials());
            var accessoryModelMaterials = _dicAccessoryModels.Values.SelectMany(m => m).SelectMany(m => m.GetMaterials());

            MemoryStream? materialStream = _matFac.Overlap(partModelMaterials.Concat(accessoryModelMaterials).OrderBy(m => m.MainIndex).ThenBy(m => m.SubIndex), ShellSize);
            return materialStream?.ToArray();
        }

        protected virtual void OnMaterialCollectionChanged()
        {
            var partModelMaterials = _dicShellPartModel.Values.SelectMany(m => m.GetMaterials());
            var accessoryModelMaterials = _dicAccessoryModels.Values.SelectMany(m => m).SelectMany(m => m.GetMaterials());

            MemoryStream? materialStream = _matFac.Overlap(partModelMaterials.Concat(accessoryModelMaterials).OrderBy(m => m.MainIndex).ThenBy(m => m.SubIndex), ShellSize);
            if(materialStream is not null)
                _matCollChangedEvent.Publish(materialStream);
        }

        protected abstract Task<Stream?> ReadTableStreamAsync(string shellName);


        protected string? GetConcatedHeadPartFileName()
        {
            var targetPartTypes = new ShellPartType[] { ShellPartType.Head, ShellPartType.Eye, ShellPartType.Face };
            bool isAllReady = !targetPartTypes.Any(pt => !_dicShellPartModel.ContainsKey(pt));

            if (!isAllReady)
            {
                _logger.Log(LogLevel.Warning, "NotCompletedHeadParts");
                return null;
            }

            return String.Join('-', targetPartTypes.Select(pt => _dicShellPartModel[pt].FileName.Replace(".png", "")));
        }

        


        private bool initializeShellSize(XmlElement rootEle)
        {
            XmlNode? sizeNode = rootEle.SelectSingleNode("/ShellInitializeInfo/ShellSize");
            if (sizeNode == null)
            {
                _logger.Log(LogLevel.Critical, "NotFound ShellInitializeInfo/ShellSize");
                return false;
            }

            IDictionary<string, string>? dicAttr = sizeNode.Attributes?.ToDictionary();
            if (dicAttr == null)
            {
                _logger.Log(LogLevel.Critical, "Empty attributes in ShellInitializeInfo/ShellSize");
                return false;
            }

            if (!dicAttr.ContainsKey("Width") || !dicAttr.ContainsKey("Height"))
            {
                _logger.Log(LogLevel.Critical, "NotFound Width and Height attributes in ShellInitializeInfo/ShellSize");
                return false;
            }

            if (Int32.TryParse(dicAttr["Width"], out int width) && Int32.TryParse(dicAttr["Height"], out int height))
            {
                ShellSize = new Size(width, height);
                return true;
            }
            else
            {
                _logger.Log(LogLevel.Critical, "InvalidValue Width and Height");
                return false;
            }
        }

        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="XmlException"></exception>
        private (IDictionary<ShellPartType, string>, IEnumerable<AccessoryAddPair>) initializeCharacterParameters(XmlElement rootEle)
        {
            XmlNode? charNode = rootEle.SelectSingleNode("/ShellInitializeInfo/ShellBasicStatus");
            if(charNode is null)
                throw new KeyNotFoundException("ShellBasicStatusNode");

            var dicPartColor = charNode
                    .SelectNodes("./PartColor")?
                    .Cast<XmlNode>()
                    .Select(createColorParameter)
                    .Where(kvp => kvp is not null);

            if (dicPartColor != null)
            {
                foreach (var partColor in dicPartColor)
                    _dicShellPartColor.Add(partColor!.Value.Key, partColor!.Value.Value);
            }

            var dicAccessoryColor = charNode
                .SelectNodes("./AccessoryColor")?
                .Cast<XmlNode>()
                .Select(createColorParameter)
                .Where(kvp => kvp is not null);

            if (dicAccessoryColor != null)
            {
                foreach (var partColor in dicAccessoryColor)
                    _dicShellPartColor.Add(partColor!.Value.Key, partColor!.Value.Value);
            }

            var dicPart = charNode
                .SelectNodes("./PartLabel")?
                .Cast<XmlNode>()
                .Select(createPartParameter)
                .Where(kvp => kvp is not null)
                .ToDictionary(kvp => kvp!.Value.Key, kvp => kvp!.Value.Value);

            var accessoryPairs = charNode
                .SelectNodes("./AccessoryLabel")?
                .Cast<XmlNode>()
                .Select(createPartParameter)
                .Where(kvp => kvp is not null)
                .Select(kvp => new AccessoryAddPair(kvp!.Value.Key, kvp.Value.Value))
                .ToArray();

            //await ChangeShell(dicPart, accessoryPairs);

            return (dicPart!, accessoryPairs!);

            KeyValuePair<ShellPartType, string>? createPartParameter(XmlNode partNode)
            {
                string? sPartType = partNode.Attributes?["PartType"]?.Value;
                if (String.IsNullOrEmpty(sPartType))
                {
                    _logger.Log(LogLevel.Warning, $"InvalidPartNodeAttributes");
                    return null;
                }

                if(!Enum.TryParse(sPartType, true, out ShellPartType partType))
                {
                    _logger.Log(LogLevel.Warning, $"InvalidValue: {sPartType}");
                    return null;
                }

                return new KeyValuePair<ShellPartType, string>(partType, partNode.InnerText);
            }
            KeyValuePair<ShellPartType, Hsl>? createColorParameter(XmlNode partColorNode)
            {
                IDictionary<string, string>? dicAtts = partColorNode.Attributes?.ToDictionary();
                if (dicAtts is null)
                {
                    _logger.Log(LogLevel.Warning, $"InvalidColorNodeAttributes");
                    return null;
                }
                    
                
                ShellPartType partType = ShellPartType.Cloth;
                double h = 0d, s = 0d, l = 0d;

                foreach(var kvp in dicAtts)
                {
                    switch(kvp.Key)
                    {
                        case "PartType":
                            {
                                if (!Enum.TryParse(kvp.Value, true, out partType))
                                {
                                    _logger.Log(LogLevel.Warning, $"InvalidValue: {kvp.Value}");
                                    return null;
                                }

                                break;
                            }
                        case "H":
                            {
                                if (!Double.TryParse(kvp.Value,  out h))
                                {
                                    _logger.Log(LogLevel.Warning, $"InvalidValue: {kvp.Value}");
                                    return null;
                                }   

                                break;
                            }
                        case "S":
                            {
                                if (!Double.TryParse(kvp.Value, out s))
                                {
                                    _logger.Log(LogLevel.Warning, $"InvalidValue: {kvp.Value}");
                                    return null;
                                }

                                break;
                            }
                        case "L":
                            {
                                if (!Double.TryParse(kvp.Value, out l))
                                {
                                    _logger.Log(LogLevel.Warning, $"InvalidValue: {kvp.Value}");
                                    return null;
                                }

                                break;
                            }
                    }
                }

                Hsl color = new Hsl(h, s, l);
                return new KeyValuePair<ShellPartType, Hsl>(partType, color);
            }
        }

        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        private async Task<(ShellPartType, ShellModelBase)?> changePart(ShellPartType partType, string newPartLabel)
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
            if (eyeModel != null) { eyeModel.IsEyeMakeup = true; }

            // ---



            bool isMaterialReady = await _matFac.LoadMaterial(ShellName, newModel);
            if (!isMaterialReady)
            {
                _logger.Log(LogLevel.Error, $"ErrorLoadMaterial: {newModel}");
                return null;
            }

            if (_dicShellPartColor.TryGetValue(partType, out Hsl? hslColor))
                newModel.ChangeColor(hslColor);

            //_dicShellPartModel.Add(partType, newModel);
            return (partType, newModel);
        }

        public abstract Task<bool> InitializeAsync(string shellName);
    }

    public class CharacterRemoteService : CharacterServiceBase
    {
        const string TableRootSectionName = "Shell:Remote:TableRoot";

        readonly HttpClient _client;
        readonly ILogger _logger;
        readonly string _tableRoot;

        public CharacterRemoteService(ILogger<CharacterRemoteService> logger, IEventAggregator eventAggregator, IConfiguration config, IShellModelFactory modelFac, IShellMaterialFactory matFac, HttpClient client)
            :base(logger, eventAggregator, config, modelFac, matFac)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _tableRoot = config?.GetSection(TableRootSectionName).Value ?? throw new KeyNotFoundException(TableRootSectionName);
        }

        public override async Task<bool> InitializeAsync(string shellName)
        {
            try
            {
                (IDictionary<ShellPartType, string> dicPart, IEnumerable<AccessoryAddPair> accPairs) = await base.InitializeBaseAsync(shellName);

#pragma warning disable CS4014 // 이 호출을 대기하지 않으므로 호출이 완료되기 전에 현재 메서드가 계속 실행됩니다.
                ChangeShell(dicPart, accPairs);
#pragma warning restore CS4014 // 이 호출을 대기하지 않으므로 호출이 완료되기 전에 현재 메서드가 계속 실행됩니다.

                return true;
            }
            catch(Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.Message);
                return false;
            }
            
        }

        protected override async Task<Stream?> ReadTableStreamAsync(string shellName)
        {
            string initTablePath = $"{_tableRoot}{shellName}/InitializeTable.xml";

            HttpResponseMessage? resMsg = null;
            Stream? xmlStream = null;
            
            try
            {
                resMsg = await _client.GetAsync(initTablePath);
                if (resMsg.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.Log(LogLevel.Error, $"NotFound: {initTablePath}");
                    return null;
                }

                xmlStream = await resMsg.Content.ReadAsStreamAsync();

                MemoryStream tableStream = new MemoryStream();
                await xmlStream.CopyToAsync(tableStream);

                tableStream.Position = 0;
                return tableStream;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Critical, ex.Message);
                return null;
            }
            finally
            {
                xmlStream?.Dispose();
                resMsg?.Dispose();
            }
        }
    }
}
