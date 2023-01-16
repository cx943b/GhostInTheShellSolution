using GhostInTheShell.Modules.InfraStructure;
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
        Task<bool> InitializeAsync(string shellName);
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
    }
}
