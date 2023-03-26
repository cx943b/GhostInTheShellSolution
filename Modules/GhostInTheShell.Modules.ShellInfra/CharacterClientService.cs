using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using GhostInTheShell.Servers.Shell;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GhostInTheShell.Modules.ShellInfra
{
    public class ShellService : IShellService
    {
        const string GrpcClientRoot = "Shell:GrpcRequestRoot";

        readonly ILogger _logger;
        readonly CharacterServer.CharacterServerClient _grpcClient;
        

        public ShellService(ILogger<ShellService> logger, IConfiguration config)
        {
            _logger = logger;

            if(config is null)
                throw new NullReferenceException(nameof(config));            

            string? grpcReqAddress = config.GetSection(GrpcClientRoot).Value;
            if (String.IsNullOrEmpty(grpcReqAddress))
                throw new KeyNotFoundException(GrpcClientRoot);

            GrpcChannel channel = GrpcChannel.ForAddress(grpcReqAddress);
            _grpcClient = new CharacterServer.CharacterServerClient(channel);
        }

        public async Task<Size> RequestShellSizeAsync()
        {
            var emptyReq = new Google.Protobuf.WellKnownTypes.Empty();

            try
            {
                var sizeRes = await _grpcClient.GetCharacterSizeAsync(emptyReq);
                return new Size(sizeRes.Width, sizeRes.Height);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.Message);
                return Size.Empty;
            }
        }

        public async Task<byte[]?> RequestShellImageAsync(string headLabel, string eyeLabel, string faceLabel)
        { 
            CharacterImageRequest charReq = new CharacterImageRequest();
            charReq.HeadLabel = headLabel;
            charReq.EyeLabel = eyeLabel;
            charReq.FaceLabel = faceLabel;

            try
            {
                CharacterImageResponse charRes = await _grpcClient.GetCharacterImageAsync(charReq);
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
