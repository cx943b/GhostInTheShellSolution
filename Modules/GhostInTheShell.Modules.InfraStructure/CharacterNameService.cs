using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GhostInTheShell.Modules.InfraStructure
{
    public interface ICharacterNameService
    {
        IEnumerable<string> CharacterNames { get; }
    }

    public class CharacterNameService : ICharacterNameService
    {
        const string CharacterNamesSectionName = "CharacterNames";

        public IEnumerable<string> CharacterNames { get; init; } = Enumerable.Empty<string>();

        public CharacterNameService(ILogger<CharacterNameService> logger, IConfiguration config)
        {
            string[]? charNames = config.GetSection(CharacterNamesSectionName).Get<string[]>();
            if (charNames is null)
            {
                logger.Log(LogLevel.Error, $"EmptyArray: {nameof(charNames)}");
                return;
            }

            CharacterNames = charNames;
        }
    }
}
