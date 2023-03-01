using GhostInTheShell.Modules.ShellInfra;
using GhostInTheShell.Servers.Shell;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using System.Drawing;

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

        public override Task<CharacterSizeResponse> GetCharacterSize(Empty request, ServerCallContext context)
        {
            var size = _charLocalSvc.ShellSize;

            CharacterSizeResponse sizeRes = new CharacterSizeResponse { IsOk = true, Width = size.Width, Height = size.Height };
            return Task.FromResult(sizeRes);
        }

        public override async Task<CharacterImageResponse> GetCharacterImage(CharacterImageRequest request, ServerCallContext context)
        {
            (byte[]? characterBytes, string resultMsg) = await _charLocalSvc.GetCharacterImage(request.HeadLabel, request.EyeLabel, request.FaceLabel);
            if(characterBytes is not null)
            {
                return new CharacterImageResponse { IsOk = true, Message = "", ImageBytes = ByteString.CopyFrom(characterBytes) };
            }
            else
            {
                _logger.Log(LogLevel.Warning, $"NullRef: {nameof(characterBytes)}");
                return new CharacterImageResponse() { Message = resultMsg };
            }
        }
    }
}