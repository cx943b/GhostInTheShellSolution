using GhostInTheShell.Servers.Shell;
using Google.Protobuf;
using Grpc.Core;

namespace GhostInTheShell.Servers.Shell.Services
{
    public class CharacterServerService : CharacterServer.CharacterServerBase
    {
        readonly ILogger _logger;
        readonly ICharacterLocalService _charLocalSvc;
       
        public CharacterServerService(ILogger<CharacterServerService> logger, ICharacterLocalService charLocalSvc)
        {
            _logger = logger;
            _charLocalSvc = charLocalSvc;
        }

        public override async Task<CharacterResponse> GetCharacterImage(CharacterRequest request, ServerCallContext context)
        {
            (byte[]? characterBytes, string resultMsg) = await _charLocalSvc.GetCharacterImage(request.HeadLabel, request.EyeLabel, request.FaceLabel);
            if(characterBytes is not null)
            {
                return new CharacterResponse { IsOk = true, Message = "", ImageBytes = ByteString.CopyFrom(characterBytes) };
            }
            else
            {
                _logger.Log(LogLevel.Warning, $"NullRef: {nameof(characterBytes)}");
                return new CharacterResponse() { Message = resultMsg };
            }
        }
    }
}