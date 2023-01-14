using GhostInTheShell.Modules.Shell.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
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
            IDictionary<ShellModelType, XmlReader>? dicTable = await ReadTableDataAsync(shellName);
            if (dicTable == null)
                return false;

            try
            {
                IEnumerable<HairModel> hairModels = InitializeHair(shellName);
                if (hairModels == null)
                    throw new XmlException("Hair info NotFound");

                _dicPartModels.Add(ShellPartType.BackHair, hairModels.Where(model => model.Position == HairPosition.Back).ToArray());
                _dicPartModels.Add(ShellPartType.FrontHair, hairModels.Where(model => model.Position == HairPosition.Front).ToArray());
                _dicPartModels.Add(ShellPartType.Head, InitializeHead(dicTable[ShellModelType.Head]));
                _dicPartModels.Add(ShellPartType.Face, InitializeFace(dicTable[ShellModelType.Face]));
                _dicPartModels.Add(ShellPartType.Eye, InitializeEye(dicTable[ShellModelType.Eye]));
                _dicPartModels.Add(ShellPartType.Accessory, InitializeAccessory(dicTable[ShellModelType.Accessory]));
                _dicPartModels.Add(ShellPartType.Cloth, InitializeCloth(dicTable[ShellModelType.Cloth]));
                _dicPartModels.Add(ShellPartType.Underwear, InitializeUnderwear(dicTable[ShellModelType.Underwear]));

                _dicPartLabels.Add(ShellPartType.AttachHair, _dicPartModels[ShellPartType.Accessory].Cast<AccessoryModel>().Where(model => String.Compare(model.TypeEn, "AttachHair", true) == 0).Select(model => model.Label).ToArray());
                _dicPartLabels.Add(ShellPartType.AttachHead, _dicPartModels[ShellPartType.Accessory].Cast<AccessoryModel>().Where(model => String.Compare(model.TypeEn, "AttachHead", true) == 0).Select(model => model.Label).ToArray());
                _dicPartLabels.Add(ShellPartType.Glasses, _dicPartModels[ShellPartType.Accessory].Cast<AccessoryModel>().Where(model => String.Compare(model.TypeEn, "Glasses", true) == 0).Select(model => model.Label).Prepend("None").ToArray());
                _dicPartLabels.Add(ShellPartType.Shoes, _dicPartModels[ShellPartType.Accessory].Cast<AccessoryModel>().Where(model => String.Compare(model.TypeEn, "Shoes", true) == 0).Select(model => model.Label).Prepend("None").ToArray());
                _dicPartLabels.Add(ShellPartType.ShoesEx, _dicPartModels[ShellPartType.Accessory].Cast<AccessoryModel>().Where(model => String.Compare(model.TypeEn, "ShoesEx", true) == 0).Select(model => model.Label).Prepend("None").ToArray());
                _dicPartLabels.Add(ShellPartType.Stocking, _dicPartModels[ShellPartType.Accessory].Cast<AccessoryModel>().Where(model => String.Compare(model.TypeEn, "Stocking", true) == 0).Select(model => model.Label).Prepend("None").ToArray());
                _dicPartLabels.Add(ShellPartType.Socks, _dicPartModels[ShellPartType.Accessory].Cast<AccessoryModel>().Where(model => String.Compare(model.TypeEn, "Socks", true) == 0).Select(model => model.Label).Prepend("None").ToArray());
                _dicPartLabels.Add(ShellPartType.Etc, _dicPartModels[ShellPartType.Accessory].Cast<AccessoryModel>().Where(model => String.Compare(model.TypeEn, "Etc", true) == 0).Select(model => model.Label).ToArray());

                _dicPartLabels.Add(ShellPartType.BackHair, _dicPartModels[ShellPartType.BackHair].Cast<HairModel>().Select(model => model.Label).ToArray());
                _dicPartLabels.Add(ShellPartType.FrontHair, _dicPartModels[ShellPartType.FrontHair].Cast<HairModel>().Select(model => model.Label).ToArray());
                _dicPartLabels.Add(ShellPartType.Cloth, _dicPartModels[ShellPartType.Cloth].Select(model => model.Label).ToArray());
                _dicPartLabels.Add(ShellPartType.Eye, _dicPartModels[ShellPartType.Eye].Select(model => model.Label).ToArray());
                _dicPartLabels.Add(ShellPartType.Face, _dicPartModels[ShellPartType.Face].Select(model => model.Label).Distinct().ToArray());
                _dicPartLabels.Add(ShellPartType.Head, _dicPartModels[ShellPartType.Head].Select(model => model.Label).ToArray());
                _dicPartLabels.Add(ShellPartType.Underwear, _dicPartModels[ShellPartType.Underwear].Select(model => model.Label).ToArray());

                ShellName = shellName;

                return true;
            }
            catch (XmlException xmlEx)
            {
                _logger.LogError(xmlEx.Message, "ShellModelFactory", "Initialize");
                return false;
            }
            catch (ArgumentException argEx)
            {
                _logger.LogError(argEx.Message, "ShellModelFactory", "Initialize");
                return false;
            }
            finally
            {
                if (dicTable != null)
                {
                    foreach (var xmlReader in dicTable.Values)
                        xmlReader.Dispose();
                }
            }
        }


        protected abstract Task<Stream?> ReadTableDataAsync(ShellModelType modelType, string shellName);


        #region Initialize
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="XmlException"/>
        public async Task<IEnumerable<HairModel>?> InitializeHair(string shellName)
        {
            Stream? tableStream = await ReadTableDataAsync(ShellModelType.Hair, shellName);
            if (tableStream is null)
                return null;

            XmlReader xmlReader = createTableReader(tableStream);

            try
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(xmlReader);

                return xDoc.DocumentElement!.ChildNodes
                    .Cast<XmlNode>()
                    .SelectMany(CreateHairModels)
                    .ToArray();
            }
            catch(Exception ex)
            {
                _logger.Log(LogLevel.Critical, ex, "InitializeHair-Fail");
                return null;
            }
            finally
            {
                xmlReader.Dispose();
                tableStream.Dispose();
            }
        }

        protected IEnumerable<HairModel> CreateHairModels(XmlNode hairsNode)
        {
            if (hairsNode is null)
                throw new ArgumentNullException(nameof(hairsNode));

            HairPosition hairPos = (HairPosition)Enum.Parse(typeof(HairPosition), hairsNode.Attributes?["Type"]?.Value ?? throw new XmlException("NotFound HairNode.Type"));
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

                string hairLabel = hairsNode.Attributes?["Label"]?.Value ?? throw new XmlException("NotFound HairNode.Label");
                string hairFileName = hairNode.Attributes?["FileName"]?.Value ?? throw new XmlException("NotFound HairNode.FileName");

                return new HairModel(hairLabel, hairFileName, normalIds, colorIds, hairPos);
            }
        }


        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="XmlException"/>
        protected virtual HeadModel[] InitializeHead(XmlReader xmlReader)
        {
            new ArgumentNullException("InitializeHead: headTable");

            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(xmlReader);

            XmlElement rootEle = xDoc.DocumentElement;
            return rootEle.ChildNodes
                .Cast<XmlNode>()
                .Select(headNode =>
                {
                    return new HeadModel(
                        headNode.Attributes["Label"].Value, headNode.Attributes["FileName"].Value,
                        new MaterialID[] { MaterialID.head }, null, (HeadType)Enum.Parse(typeof(HeadType), headNode.Attributes["Type"].Value));

                }).ToArray();
        }
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="XmlException"/>
        protected virtual FaceModel[] InitializeFace(XmlReader xmlReader)
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(xmlReader);

            XmlElement rootEle = xDoc.DocumentElement;
            MaterialID[] faceNormalID = new MaterialID[] { MaterialID.face_back, MaterialID.face_front };
            MaterialID[] faceColorID = new MaterialID[] { MaterialID.face_haircolor };

            return rootEle.ChildNodes
                .Cast<XmlNode>()
                .SelectMany(node =>
                {
                    string label = node.Attributes["Label"]?.Value;
                    IEnumerable<string> fileNames = node.Attributes["FileName"]?.Value.Split(',').Select(name => name.Trim());

                    return fileNames.Select(fileName => new FaceModel(label, fileName, faceNormalID, faceColorID));
                }).ToArray();
        }
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="XmlException"/>
        protected virtual EyeModel[] InitializeEye(XmlReader xmlReader)
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(xmlReader);

            XmlElement rootEle = xDoc.DocumentElement;
            MaterialID[] eyeNormalID = new MaterialID[] { MaterialID.eye };
            MaterialID[] eyeColorID = new MaterialID[] { MaterialID.eye_color };

            return rootEle.ChildNodes
                .Cast<XmlNode>()
                .SelectMany(node =>
                {
                    string label = node.Attributes["Label"]?.Value;
                    IEnumerable<string> fileNames = node.Attributes["FileName"]?.Value.Split(',').Select(name => name.Trim());

                    return fileNames.Select(fileName => new EyeModel(label, fileName, eyeNormalID, eyeColorID));
                }).ToArray();
        }
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="XmlException"/>
        protected virtual AccessoryModel[] InitializeAccessory(XmlReader xmlReader)
        {
            XmlReaderSettings xrSetting = new XmlReaderSettings();
            xrSetting.IgnoreComments = true;

            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(xmlReader);

            XmlElement rootEle = xDoc.DocumentElement;

            MaterialID[] iColorID = new MaterialID[]
                {
                    MaterialID.accessory_back,
                    MaterialID.accessory_front,
                    MaterialID.accessory_middle_back,
                    MaterialID.accessory_middle_front,
                    MaterialID.accessory_underwear
                };

            return rootEle.ChildNodes
                .Cast<XmlNode>()
                .SelectMany(accessoreisNode =>
                {
                    string typeEn = accessoreisNode.Attributes["TypeEn"].Value;
                    string typeKr = accessoreisNode.Attributes["TypeKr"].Value;
                    bool isUseColor = accessoreisNode.Attributes["IsUseColor"]?.Value.ToLower() == "true";

                    return accessoreisNode.ChildNodes
                        .Cast<XmlNode>()
                        .Select(accessoryNode => new AccessoryModel(accessoryNode.Attributes["Label"].Value, accessoryNode.Attributes["FileName"].Value, null, iColorID, typeKr, typeEn));
                }).ToArray();
        }
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="XmlException"/>
        protected virtual ClothModel[] InitializeCloth(XmlReader xmlReader)
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(xmlReader);

            XmlElement rootEle = xDoc.DocumentElement;

            MaterialID[] clothNoramlID = new MaterialID[] { MaterialID.body_back, MaterialID.body_front, MaterialID.body_front_accessory };
            MaterialID[] clothColorID = new MaterialID[] { MaterialID.body_front_color };

            return rootEle.ChildNodes
                .Cast<XmlNode>()
                .Select(clothNode =>
                {
                    bool useUnderwear = true;
                    bool usePad = false;

                    XmlAttribute underAtt = clothNode.Attributes.Cast<XmlAttribute>().FirstOrDefault(att => String.Compare(att.Name, "UseUnderwear", true) == 0);
                    if (underAtt != null)
                    {
                        useUnderwear = String.Compare(underAtt.Value, "true", true) == 0;
                    }
                    else
                    {
                        underAtt = clothNode.Attributes.Cast<XmlAttribute>().FirstOrDefault(att => String.Compare(att.Name, "UsePad", true) == 0);

                        if (underAtt != null)
                        {
                            useUnderwear = false;
                            usePad = String.Compare(underAtt.Value, "true", true) == 0;
                        }
                    }

                    return new ClothModel(clothNode.Attributes["Label"].Value, clothNode.Attributes["FileName"].Value, clothNoramlID, clothColorID, useUnderwear, usePad);
                }).ToArray();
        }
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="XmlException"/>
        protected virtual UnderwearModel[] InitializeUnderwear(XmlReader xmlReader)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlReader);

            XmlElement rootEle = xmlDoc.DocumentElement;
            MaterialID[] normalIDs = new MaterialID[] { MaterialID.accessory_underwear };

            return rootEle.ChildNodes
                .Cast<XmlNode>()
                .Select(node => new UnderwearModel(node.Attributes["Label"].Value, node.Attributes["FileName"].Value, normalIDs, null)).ToArray();
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