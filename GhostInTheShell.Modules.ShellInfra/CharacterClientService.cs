using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using GhostInTheShell.Servers.Shell;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;

namespace GhostInTheShell.Modules.ShellInfra
{
    public class CharacterClientService
    {
        readonly ILogger<CharacterClientService> _logger;
        readonly CharacterServer.CharacterServerClient _grpcClient;
        

        public CharacterClientService(ILogger<CharacterClientService> logger)
        {
            _logger = logger;

            GrpcChannel channel = GrpcChannel.ForAddress("https://www.SosoConsole.com:8051");
            _grpcClient = new CharacterServer.CharacterServerClient(channel);
        }

        public async Task<byte[]?> RequestCharacterImage(string headLabel, string eyeLabel, string faceLabel)
        {
            CharacterRequest charReq = new CharacterRequest();
            charReq.HeadLabel = headLabel;
            charReq.EyeLabel = eyeLabel;
            charReq.FaceLabel = faceLabel;

            try
            {
                CharacterResponse charRes = await _grpcClient.GetCharacterImageAsync(charReq);
                if (!charRes.IsOk)
                {
                    _logger.Log(LogLevel.Error, $"BadRes: HeadLabel-{headLabel}, EyeLabel-{eyeLabel}, FaceLabel-{faceLabel}\r\n\t{charRes.Message}");
                    return null;
                }

                return charRes.ImageBytes.ToArray();
            }
            catch(Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.Message);
                return null;
            }
        }
    }
}
