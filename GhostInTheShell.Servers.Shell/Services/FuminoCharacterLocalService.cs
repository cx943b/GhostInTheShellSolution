using GhostInTheShell.Modules.InfraStructure;
using GhostInTheShell.Modules.ShellInfra;
using Prism.Events;

namespace GhostInTheShell.Servers.Shell.Services
{
    public interface IFuminoCharacterLocalService : ICharacterLocalService
    {
        Task<bool> InitializeAsync();
    }
    public interface IKaoriCharacterLocalService : ICharacterLocalService
    {
        Task<bool> InitializeAsync();
    }



    public class FuminoCharacterLocalService : CharacterLocalService, IFuminoCharacterLocalService
    {
        public FuminoCharacterLocalService(ILogger<CharacterLocalService> logger, IConfiguration config, IShellModelFactory modelFac, IShellMaterialFactory matFac)
            : base(logger, config, modelFac, matFac)
        {
        }

        public Task<bool> InitializeAsync() => base.InitializeAsync(ShellNames.Fumino);
    }
    public class KaoriCharacterLocalService: CharacterLocalService, IKaoriCharacterLocalService
    {
        public KaoriCharacterLocalService(ILogger<CharacterLocalService> logger, IConfiguration config, IShellModelFactory modelFac, IShellMaterialFactory matFac)
            : base(logger, config, modelFac, matFac)
        {
        }

        public Task<bool> InitializeAsync() => base.InitializeAsync(ShellNames.Kaori);
    }


}
