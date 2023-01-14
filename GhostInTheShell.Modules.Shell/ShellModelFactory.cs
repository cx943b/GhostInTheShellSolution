using GhostInTheShell.Modules.InfraStructure;
using GhostInTheShell.Modules.Shell.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Printing.IndexedProperties;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Xml;
using System.Xml.Linq;

namespace GhostInTheShell.Modules.Shell
{
    public interface ICharacter
    {
        string ShellName { get; }
        Size ShellSize { get; }
    }
    public interface IShellRemoteService : IDisposable
    {
        Task<XmlReader> RequestInitializeDataAsync(string shellName);
        Task<byte[]> RequestMaterialAsync(string shellName, string materialPath);
        Task<(int width, int height)> RequestMaterialSizeAsync(string shellName);
        Task<XmlReader> RequestTableAsync(string shellName, string tableName);
    }
    public interface IShellModelFactory : ICharacter
    {
        IEnumerable<string> GetLabels(ShellPartType partType);
        IEnumerable<ShellModelBase> GetModels(ShellPartType partType, string label);
        Task<bool> InitializeAsync(string shellName);
    }

    public abstract class ShellModelFactoryBase : IShellModelFactory
    {
        object _initLock = new object();

        protected readonly ILogger _logger;
        protected readonly IConfiguration _Config;
        protected readonly HttpClient _Client;

        readonly IDictionary<ShellPartType, IEnumerable<ShellModelBase>> _dicPartModels = new Dictionary<ShellPartType, IEnumerable<ShellModelBase>>();
        readonly IDictionary<ShellPartType, IEnumerable<string>> _dicPartLabels = new Dictionary<ShellPartType, IEnumerable<string>>();

        public string ShellName { get; protected set; } = String.Empty;
        public Size ShellSize { get; protected set; }

        public ShellModelFactoryBase(ILogger logger, IConfiguration config, HttpClient client)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _Config = config ?? throw new ArgumentNullException(nameof(config));
            _Client = client;
        }

        public IEnumerable<string> GetLabels(ShellPartType partType)
        {
            if (!_dicPartLabels.ContainsKey(partType))
                throw new KeyNotFoundException($"{partType}");

            return _dicPartLabels[partType];
        }
        public IEnumerable<ShellModelBase> GetModels(ShellPartType partType, string label)
        {
            if (!_dicPartModels.ContainsKey(partType))
                throw new KeyNotFoundException($"{partType}");

            return _dicPartModels[partType].Where(model => String.Compare(model.Label, label, true) == 0).ToArray();
        }
        public async Task<bool> InitializeAsync(string shellName)
        {
            if (String.IsNullOrEmpty(shellName))
            {
                _logger.Log(LogLevel.Critical, new ArgumentNullException(nameof(shellName)), "Invalid ShellName");
                return false;
            }

            try
            {
                Task<bool>[] initTasks = new Task<bool>[]
                {
                    InitializeAccessoriesAsync(shellName),
                    InitializeClothsAsync(shellName),
                    InitializeEyesAsync(shellName),
                    InitializeFacesAsync(shellName),
                    InitializeHairsAsync(shellName),
                    InitializeHeadsAsync(shellName),
                    InitializeUnderwearsAsync(shellName)
                };
                //await InitializeAccessoriesAsync(shellName);
                //await InitializeClothsAsync(shellName);
                //await InitializeEyesAsync(shellName);
                //await InitializeFacesAsync(shellName);
                //await InitializeHairsAsync(shellName);
                //await InitializeHeadsAsync(shellName);
                //await InitializeUnderwearsAsync(shellName);

                bool[] initResults = await Task.WhenAll(initTasks);
                bool isModelsReady = initResults.All(r => true);
                if(isModelsReady)
                    ShellName = shellName;

                return isModelsReady;
            }
            catch(Exception ex)
            {
                _logger.Log( LogLevel.Critical, ex.Message, "ShellModelFactoryInitialize-Fail");
                return false;
            }
        }

        #region Initialize
        public async Task<bool> InitializeHairsAsync(string shellName)
        {
            Stream? tableStream = await ReadTableDataAsync(ShellModelType.Hair, shellName);
            if (tableStream is null)
                return false;

            XmlReader xmlReader = createTableReader(tableStream);

            try
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(xmlReader);

                IEnumerable<HairModel>? hairModels = xDoc.DocumentElement?.ChildNodes
                    .Cast<XmlNode>()
                    .SelectMany(CreateHairModels)
                    .ToArray();

                if (hairModels is null)
                    return false;

                var backHairs = hairModels.Where(model => model.Position == HairPosition.Back).ToArray();
                var frontHairs = hairModels.Where(model => model.Position == HairPosition.Front).ToArray();

                lock(_initLock)
                {
                    _dicPartModels.Add(ShellPartType.BackHair, backHairs);
                    _dicPartModels.Add(ShellPartType.FrontHair, frontHairs);

                    _dicPartLabels.Add(ShellPartType.BackHair, backHairs.Cast<HairModel>().Select(model => model.Label).ToArray());
                    _dicPartLabels.Add(ShellPartType.FrontHair, frontHairs.Cast<HairModel>().Select(model => model.Label).ToArray());
                }

                return true;
            }
            catch(Exception ex)
            {
                _logger.Log(LogLevel.Critical, ex, "InitializeHairs-Fail");
                return false;
            }
            finally
            {
                xmlReader.Dispose();
                tableStream.Dispose();
            }
        }
        public async Task<bool> InitializeHeadsAsync(string shellName)
        {
            Stream? tableStream = await ReadTableDataAsync(ShellModelType.Head, shellName);
            if (tableStream is null)
                return false;

            XmlReader xmlReader = createTableReader(tableStream);

            try
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(xmlReader);

                var headModels = xDoc.DocumentElement?.ChildNodes
                    .Cast<XmlNode>()
                    .Select(CreateHeadModels)
                    .ToArray();

                if (headModels is null)
                    return false;

                lock(_initLock)
                {
                    _dicPartModels.Add(ShellPartType.Head, headModels);
                    _dicPartLabels.Add(ShellPartType.Head, headModels.Select(model => model.Label).ToArray());
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Critical, ex, "InitializeHeads-Fail");
                return false;
            }
            finally
            {
                xmlReader.Dispose();
                tableStream.Dispose();
            }
        }
        public async Task<bool> InitializeFacesAsync(string shellName)
        {
            Stream? tableStream = await ReadTableDataAsync(ShellModelType.Face, shellName);
            if (tableStream is null)
                return false;

            XmlReader xmlReader = createTableReader(tableStream);

            try
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(xmlReader);

                var faceModels = xDoc.DocumentElement?.ChildNodes
                    .Cast<XmlNode>()
                    .SelectMany(CreateFaceModels)
                    .ToArray();

                if (faceModels is null)
                    return false;

                lock (_initLock)
                {
                    _dicPartModels.Add(ShellPartType.Face, faceModels);
                    _dicPartLabels.Add(ShellPartType.Face, faceModels.Select(model => model.Label).Distinct().ToArray());
                }   

                return true;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Critical, ex, "InitializeFaces-Fail");
                return false;
            }
            finally
            {
                xmlReader.Dispose();
                tableStream.Dispose();
            }
        }
        public async Task<bool> InitializeEyesAsync(string shellName)
        {
            Stream? tableStream = await ReadTableDataAsync(ShellModelType.Eye, shellName);
            if (tableStream is null)
                return false;

            XmlReader xmlReader = createTableReader(tableStream);

            try
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(xmlReader);

                var eyeModels = xDoc.DocumentElement?.ChildNodes
                    .Cast<XmlNode>()
                    .SelectMany(CreateEyeModels)
                    .ToArray();

                if (eyeModels is null)
                    return false;

                lock (_initLock)
                {
                    _dicPartModels.Add(ShellPartType.Eye, eyeModels);
                    _dicPartLabels.Add(ShellPartType.Eye, eyeModels.Select(model => model.Label).ToArray());
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Critical, ex, "InitializeEyes-Fail");
                return false;
            }
            finally
            {
                xmlReader.Dispose();
                tableStream.Dispose();
            }
        }
        public async Task<bool> InitializeAccessoriesAsync(string shellName)
        {
            Stream? tableStream = await ReadTableDataAsync(ShellModelType.Accessory, shellName);
            if (tableStream is null)
                return false;

            XmlReader xmlReader = createTableReader(tableStream);

            try
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(xmlReader);

                var accessoryModels = xDoc.DocumentElement?.ChildNodes
                    .Cast<XmlNode>()
                    .SelectMany(CreateAccessories)
                    .ToArray();

                if (accessoryModels is null)
                    return false;

                lock (_initLock)
                {
                    _dicPartModels.Add(ShellPartType.Accessory, accessoryModels);

                    _dicPartLabels.Add(ShellPartType.AttachHair, accessoryModels.Where(model => String.Compare(model.TypeEn, nameof(ShellPartType.AttachHair), true) == 0).Select(model => model.Label).ToArray());
                    _dicPartLabels.Add(ShellPartType.AttachHead, accessoryModels.Where(model => String.Compare(model.TypeEn, nameof(ShellPartType.AttachHead), true) == 0).Select(model => model.Label).ToArray());
                    _dicPartLabels.Add(ShellPartType.Glasses, accessoryModels.Where(model => String.Compare(model.TypeEn, nameof(ShellPartType.Glasses), true) == 0).Select(model => model.Label).Prepend("None").ToArray());
                    _dicPartLabels.Add(ShellPartType.Shoes, accessoryModels.Where(model => String.Compare(model.TypeEn, nameof(ShellPartType.Shoes), true) == 0).Select(model => model.Label).Prepend("None").ToArray());
                    _dicPartLabels.Add(ShellPartType.ShoesEx, accessoryModels.Where(model => String.Compare(model.TypeEn, nameof(ShellPartType.ShoesEx), true) == 0).Select(model => model.Label).Prepend("None").ToArray());
                    _dicPartLabels.Add(ShellPartType.Stocking, accessoryModels.Where(model => String.Compare(model.TypeEn, nameof(ShellPartType.Stocking), true) == 0).Select(model => model.Label).Prepend("None").ToArray());
                    _dicPartLabels.Add(ShellPartType.Socks, accessoryModels.Where(model => String.Compare(model.TypeEn, nameof(ShellPartType.Socks), true) == 0).Select(model => model.Label).Prepend("None").ToArray());
                    _dicPartLabels.Add(ShellPartType.Etc, accessoryModels.Where(model => String.Compare(model.TypeEn, nameof(ShellPartType.Etc), true) == 0).Select(model => model.Label).ToArray());
                }   

                return true;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Critical, ex, "InitializeAccessories-Fail");
                return false;
            }
            finally
            {
                xmlReader.Dispose();
                tableStream.Dispose();
            }
        }
        public async Task<bool> InitializeClothsAsync(string shellName)
        {
            Stream? tableStream = await ReadTableDataAsync(ShellModelType.Cloth, shellName);
            if (tableStream is null)
                return false;

            XmlReader xmlReader = createTableReader(tableStream);

            try
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(xmlReader);

                var clothModels = xDoc.DocumentElement?.ChildNodes
                    .Cast<XmlNode>()
                    .Select(CreateClothModel)
                    .ToArray();

                if (clothModels is null)
                    return false;

                lock (_initLock)
                {
                    _dicPartModels.Add(ShellPartType.Cloth, clothModels);
                    _dicPartLabels.Add(ShellPartType.Cloth, clothModels.Select(model => model.Label).ToArray());
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Critical, ex, "InitializeCloths-Fail");
                return false;
            }
            finally
            {
                xmlReader.Dispose();
                tableStream.Dispose();
            }
        }
        public async Task<bool> InitializeUnderwearsAsync(string shellName)
        {
            Stream? tableStream = await ReadTableDataAsync(ShellModelType.Underwear, shellName);
            if (tableStream is null)
                return false;

            XmlReader xmlReader = createTableReader(tableStream);

            try
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(xmlReader);

                var underwearModels = xDoc.DocumentElement?.ChildNodes
                    .Cast<XmlNode>()
                    .Select(CreateUnderwearModel)
                    .ToArray();

                if (underwearModels is null)
                    return false;

                lock (_initLock)
                {
                    _dicPartModels.Add(ShellPartType.Underwear, underwearModels);
                    _dicPartLabels.Add(ShellPartType.Underwear, underwearModels.Select(model => model.Label).ToArray());
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Critical, ex, "InitializeHairs-Fail");
                return false;
            }
            finally
            {
                xmlReader.Dispose();
                tableStream.Dispose();
            }
        }




        protected abstract Task<Stream?> ReadTableDataAsync(ShellModelType modelType, string shellName);

        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="XmlException"></exception>
        protected virtual IEnumerable<HairModel> CreateHairModels(XmlNode hairsNode)
        {
            if (hairsNode is null)
                throw new ArgumentNullException(nameof(hairsNode));
            if (hairsNode.Attributes is null)
                throw new NullReferenceException("HairsNode.Attributes");

            string sHairPosition = hairsNode.Attributes["Type"]?.Value ?? throw new NullReferenceException("NotFound HairsNode.Type");
            if (!Enum.TryParse(sHairPosition, out HairPosition hairPos))
                throw new ArgumentException("InvalidValue.HairPoition");

            MaterialID[] normalIds = null!;
            MaterialID[] colorIds = null!;

            if (hairPos == HairPosition.Back)
            {
                normalIds = new MaterialID[] { MaterialID.hair_back_accessory };
                colorIds = new MaterialID[] { MaterialID.hair_back };
            }
            else
            {
                normalIds = new MaterialID[] { MaterialID.hair_front_accessory };
                colorIds = new MaterialID[] { MaterialID.hair_front };
            }

            return hairsNode.ChildNodes.Cast<XmlNode>().Select(createHairModel).ToArray();

            HairModel createHairModel(XmlNode hairNode)
            {
                if (hairNode is null)
                    throw new ArgumentNullException(nameof(hairNode));
                if (hairNode.Attributes is null)
                    throw new NullReferenceException("HairNode.Attributes");

                string hairLabel = hairNode.Attributes["Label"]?.Value ?? throw new NullReferenceException("NotFound HairNode.Label");
                string hairFileName = hairNode.Attributes["FileName"]?.Value ?? throw new NullReferenceException("NotFound HairNode.FileName");

                return new HairModel(hairLabel, hairFileName, normalIds, colorIds, hairPos);
            }
        }

        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="NullReferenceException"></exception>
        /// <exception cref="ArgumentException"></exception>
        protected virtual HeadModel CreateHeadModels(XmlNode headNode)
        {
            if (headNode is null)
                throw new ArgumentNullException(nameof(headNode));
            if (headNode.Attributes is null)
                throw new NullReferenceException("HeadNode.Attributes");

            string headLabel = headNode.Attributes["Label"]?.Value ?? throw new NullReferenceException("NotFound HeadNode.Label");
            string headFileName = headNode.Attributes["FileName"]?.Value ?? throw new NullReferenceException("NotFound HeadNode.FileName");
            string sHeadType = headNode.Attributes["Type"]?.Value ?? throw new NullReferenceException("NotFound HeadNode.Type");

            if (!Enum.TryParse(sHeadType, out HeadType headType))
                throw new ArgumentException("Invalid HeadTypeString");

            MaterialID[] normalIds = new MaterialID[] { MaterialID.head };

            return new HeadModel(headLabel, headFileName, normalIds, null, headType);
        }
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="XmlException"></exception>
        protected virtual IEnumerable<FaceModel> CreateFaceModels(XmlNode faceNode)
        {
            if (faceNode is null)
                throw new ArgumentNullException(nameof(faceNode));
            if (faceNode.Attributes is null)
                throw new NullReferenceException("FaceNode.Attributes");

            MaterialID[] normalID = new MaterialID[] { MaterialID.face_back, MaterialID.face_front };
            MaterialID[] colorID = new MaterialID[] { MaterialID.face_haircolor };

            string faceLabel = faceNode.Attributes["Label"]?.Value ?? throw new NullReferenceException("NotFound FaceNode.Label");
            string sFaceFileNames = faceNode.Attributes["FileName"]?.Value ?? throw new NullReferenceException("NotFound FaceNode.FileNames");

            IEnumerable<string> faceFileNames = sFaceFileNames.Split(',').Select(name => name.Trim());
            return faceFileNames.Select(fileName => new FaceModel(faceLabel, fileName, normalID, colorID)).ToArray();
        }
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="XmlException"></exception>
        protected virtual IEnumerable<EyeModel> CreateEyeModels(XmlNode eyeNode)
        {
            if (eyeNode is null)
                throw new ArgumentNullException(nameof(eyeNode));
            if (eyeNode.Attributes is null)
                throw new NullReferenceException("EyeNode.Attributes");

            MaterialID[] normalID = new MaterialID[] { MaterialID.eye };
            MaterialID[] colorID = new MaterialID[] { MaterialID.eye_color };

            string eyeLabel = eyeNode.Attributes?["Label"]?.Value ?? throw new NullReferenceException("NotFound EyeNode.Label");
            string sEyeFileNames = eyeNode.Attributes?["FileName"]?.Value ?? throw new NullReferenceException("NotFound EyeNode.FileNames");

            IEnumerable<string> eyeFileNames = sEyeFileNames.Split(',').Select(name => name.Trim());
            return eyeFileNames.Select(fileName => new EyeModel(eyeLabel, fileName, normalID, colorID)).ToArray();
        }
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="XmlException"></exception>
        protected virtual IEnumerable<AccessoryModel> CreateAccessories(XmlNode accessoriesNode)
        {
            if (accessoriesNode is null)
                throw new ArgumentNullException(nameof(accessoriesNode));
            if (accessoriesNode.Attributes is null)
                throw new NullReferenceException("AccessoriesNode.Attributes");

            string typeEn = accessoriesNode.Attributes["TypeEn"]?.Value ?? throw new NullReferenceException("NotFound AccessoriesNode.TypeEn");
            string typeKr = accessoriesNode.Attributes["TypeKr"]?.Value ?? throw new NullReferenceException("NotFound AccessoriesNode.TypeKr");
            string? sIsUseColor = accessoriesNode.Attributes["IsUseColor"]?.Value;

            if (!Boolean.TryParse(sIsUseColor, out bool isUseColor))
                isUseColor = false;

            MaterialID[] colorIds = new MaterialID[]
                {
                    MaterialID.accessory_back,
                    MaterialID.accessory_front,
                    MaterialID.accessory_middle_back,
                    MaterialID.accessory_middle_front,
                    MaterialID.accessory_underwear
                };

            return accessoriesNode.ChildNodes
                        .Cast<XmlNode>()
                        .Select(createAccessoryModel)
                        .ToArray();

            AccessoryModel createAccessoryModel(XmlNode accessoryNode)
            {
                if (accessoryNode is null)
                    throw new ArgumentNullException(nameof(accessoryNode));
                if (accessoryNode.Attributes is null)
                    throw new NullReferenceException("AccessoryNode.Attributes");

                string accessoryLabel = accessoryNode.Attributes["Label"]?.Value ?? throw new NullReferenceException("NotFound AccessoryNode.Label");
                string accessoryFileName= accessoryNode.Attributes["FileName"]?.Value ?? throw new NullReferenceException("NotFound AccessoryNode.FileName");

                return new AccessoryModel(accessoryLabel, accessoryFileName, null, colorIds, typeKr, typeEn);
            }
        }
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="XmlException"></exception>
        protected virtual ClothModel CreateClothModel(XmlNode clothNode)
        {
            if (clothNode is null)
                throw new ArgumentNullException(nameof(clothNode));
            if(clothNode.Attributes is null)
                throw new NullReferenceException("ClothNode.Attributes");

            MaterialID[] normalIds = new MaterialID[] { MaterialID.body_back, MaterialID.body_front, MaterialID.body_front_accessory };
            MaterialID[] colorIds = new MaterialID[] { MaterialID.body_front_color };

            string clothLabel = clothNode.Attributes["Label"]?.Value ?? throw new NullReferenceException("NotFound AccessoryNode.Label");
            string clothFileName = clothNode.Attributes["FileName"]?.Value ?? throw new NullReferenceException("NotFound AccessoryNode.FileName");

            bool isUseUnderwear = true;
            string? bIsUseUnderwear = clothNode.Attributes["UseUnderwear"]?.Value;
            if (!String.IsNullOrEmpty(bIsUseUnderwear) && Boolean.TryParse(bIsUseUnderwear, out isUseUnderwear))
            {
            }

            bool isUsePad = false;
            string? bIsUsePad = clothNode.Attributes["UsePad"]?.Value;
            if (!String.IsNullOrEmpty(bIsUsePad) && Boolean.TryParse(bIsUsePad, out isUsePad))
            {
            }

            return new ClothModel(clothLabel, clothFileName, normalIds, colorIds, isUseUnderwear, isUsePad);
        }
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="XmlException"></exception>
        protected virtual UnderwearModel CreateUnderwearModel(XmlNode underwearNode)
        {
            if (underwearNode is null)
                throw new ArgumentNullException(nameof(underwearNode));
            if (underwearNode.Attributes is null)
                throw new NullReferenceException("UnderwearNode.Attributes");

            MaterialID[] normalIds = new MaterialID[] { MaterialID.accessory_underwear };
            string underwearLabel = underwearNode.Attributes?["Label"]?.Value ?? throw new NullReferenceException("NotFound UnderwearNode.Label");
            string underwearFileName = underwearNode.Attributes?["FileName"]?.Value ?? throw new NullReferenceException("NotFound UnderwearNode.FileName");

            return new UnderwearModel(underwearLabel, underwearFileName, normalIds, null);
        }
        #endregion Initialize


        private XmlReader createTableReader(Stream tableStream)
        {
            if (tableStream is null)
                throw new ArgumentNullException(nameof(tableStream));

            XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
            xmlReaderSettings.IgnoreComments = true;

            return XmlReader.Create(tableStream, xmlReaderSettings);
        }
    }



    public class ShellModelFactory : ShellModelFactoryBase
    {
        const string TableRootSectionName = "ShellData:Dav:TableRoot";
        readonly string? _tableRoot;


        public ShellModelFactory(ILogger<ShellModelFactory> logger, IConfiguration config, HttpClient client) : base(logger, config, client)
        {
            _tableRoot = _Config.GetSection(TableRootSectionName)?.Value ?? throw new KeyNotFoundException("TableRootSectionName");
        }

        protected override async Task<Stream?> ReadTableDataAsync(ShellModelType modelType, string shellName)
        {
            string tablePath = $"{_tableRoot}{shellName}/{modelType}Table.xml";

            try
            {
                return await _Client.GetStreamAsync(tablePath);
            }
            catch(Exception ex)
            {
                _logger.Log(LogLevel.Critical, ex, $"Couldn't read {tablePath}");
                return null;
            }
        }
    }
}