using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.TextFormatting;

namespace GhostInTheShell.Modules.Shell
{
    public interface ICharacterService
    {
        Task<bool> InitializeAsync(string shellName);
    }
    public abstract class CharacterServiceBase : ICharacter, ICharacterService
    {
        readonly ILogger _logger;
        readonly IConfiguration _config;
        readonly IShellModelFactory _modelFac;
        readonly IShellMaterialFactory _matFac;

        public string ShellName { get; private set; }

        public Size ShellSize { get; private set; }

        public CharacterServiceBase(ILogger logger, IConfiguration config, IShellModelFactory modelFac, IShellMaterialFactory matFac)
        {
            _logger = logger;
            _config = config;

            _modelFac = modelFac;
            _matFac = matFac;
        }

        public async Task<bool> InitializeAsync(string shellName)
        {
            bool isModelFactoryReady = await _modelFac.InitializeAsync(shellName);
            return isModelFactoryReady;
        }
    }
}
