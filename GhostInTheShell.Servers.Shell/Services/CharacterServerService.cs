using GhostInTheShell.Modules.InfraStructure;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using System.Drawing;

namespace GhostInTheShell.Servers.Shell.Services
{
    public class CharacterServerService : CharacterServer.CharacterServerBase
    {
        readonly ILogger _logger;
        readonly IDictionary<string, ICharacterLocalService> _dicCharLocalSvc = new Dictionary<string, ICharacterLocalService>();
       
        public CharacterServerService(ILogger<CharacterServerService> logger, IFuminoCharacterLocalService fuminoCharLocalSvc, IKaoriCharacterLocalService kaoriCharLocalSvc)
        {
            _logger = logger;

            if(fuminoCharLocalSvc is null)
                throw new ArgumentNullException(nameof(fuminoCharLocalSvc));
            if (kaoriCharLocalSvc is null)
                throw new ArgumentNullException(nameof(kaoriCharLocalSvc));

            _dicCharLocalSvc.Add(CharacterNames.Fumino, fuminoCharLocalSvc);
            _dicCharLocalSvc.Add(CharacterNames.Kaori, kaoriCharLocalSvc);
        }

        public override Task<CharacterSizeResponse> GetCharacterSize(CharacterSizeRequest request, ServerCallContext context)
        {
            string? charName = request.CharName;
            if (String.IsNullOrEmpty(charName))
            {
                _logger.Log(LogLevel.Warning, $"NullRef: {charName}");
                return Task.FromResult(new CharacterSizeResponse());
            }
                
            if(!_dicCharLocalSvc.ContainsKey(charName))
            {
                _logger.Log(LogLevel.Warning, $"KeyNotFound: {charName}");
                return Task.FromResult(new CharacterSizeResponse());
            }

            var size = _dicCharLocalSvc[charName].ShellSize;

            CharacterSizeResponse sizeRes = new CharacterSizeResponse { IsOk = true, Width = size.Width, Height = size.Height };
            return Task.FromResult(sizeRes);
        }

        public override async Task<CharacterImageResponse> GetCharacterImage(CharacterImageRequest request, ServerCallContext context)
        {
            string? charName = request.CharName;
            if (String.IsNullOrEmpty(charName))
            {
                _logger.Log(LogLevel.Warning, $"NullRef: {charName}");
                return new CharacterImageResponse();
            }

            if (!_dicCharLocalSvc.ContainsKey(charName))
            {
                _logger.Log(LogLevel.Warning, $"KeyNotFound: {charName}");
                return new CharacterImageResponse();
            }

            (byte[]? characterBytes, string resultMsg) = await _dicCharLocalSvc[charName].GetCharacterImage(request.HeadLabel, request.EyeLabel, request.FaceLabel);
            if(characterBytes is not null)
            {
                return new CharacterImageResponse { IsOk = true, Message = "", ImageBytes = ByteString.CopyFrom(characterBytes) };
            }
            else
            {
                _logger.Log(LogLevel.Warning, $"NullRef: {nameof(characterBytes)}");
                return new CharacterImageResponse() { Message = resultMsg };    // Use message for Debug
            }
        }
    }
}