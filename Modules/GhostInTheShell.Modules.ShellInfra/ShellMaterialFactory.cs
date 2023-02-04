using GhostInTheShell.Modules.ShellInfra.Models;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using GhostInTheShell.Modules.InfraStructure;

namespace GhostInTheShell.Modules.ShellInfra
{
    public abstract class ShellMaterialFactoryBase : IShellMaterialFactory
    {
        protected readonly ILogger _Logger;

        public ShellMaterialFactoryBase(ILogger logger)
        {
            _Logger = logger;
        }

        public async Task<bool> LoadMaterial(string shellName, ShellModelBase shellModel)
        {
            UnloadMaterial(shellModel);

            try
            {
                if (shellModel.ColorMaterials == null && shellModel.ColorIds != null)
                {
                    Task<ColorableMaterialModel?>[] matTasks = shellModel.ColorIds.Select(matId => createColorMaterialAsync(shellName, shellModel.FileName, matId)).ToArray();
                    var colorMaterialModels = await Task.WhenAll(matTasks);

                    shellModel.ColorMaterials = colorMaterialModels.Where(m => m != null).Cast<ColorableMaterialModel>().ToArray();
                }

                if (shellModel.Materials == null && shellModel.NormalIds != null)
                {
                    Task<MaterialModel?>[] matTasks = shellModel.NormalIds.Select(matId => createMaterialAsync(shellName, shellModel.FileName, matId)).ToArray();
                    var materialModels = await Task.WhenAll(matTasks);

                    shellModel.Materials = materialModels.Where(m => m != null).Cast<MaterialModel>().ToArray();
                }

                return true;
            }
            catch(Exception ex)
            {
                _Logger.Log(LogLevel.Error, ex, null);
                return false;
            }
        }

        public void UnloadMaterial(ShellModelBase shellModel)
        {
            if(shellModel.ColorMaterials != null)
            {
                foreach(var colorMaterial in shellModel.ColorMaterials)
                {
                    colorMaterial.Dispose();
                }

                shellModel.ColorMaterials = null;
            }

            if(shellModel.Materials != null)
            {
                foreach(var material in shellModel.Materials)
                {
                    material.Dispose();
                }

                shellModel.Materials = null;
            }
        }



        public MemoryStream? Overlap(IOrderedEnumerable<IMaterialModel> materialModels, Size shellSize)
        {
            if (materialModels == null)
                return null;
            //throw new ArgumentNullException("Overlap: materialModels");

            if (!materialModels.Any())
                return null;
            //throw new ArgumentException("Overlap: materialModels, Count: 0");

            Bitmap? bitmap = null;
            Graphics? g = null;

            try
            {
                MemoryStream ms = new MemoryStream();

                bitmap = new Bitmap(shellSize.Width, shellSize.Height, PixelFormat.Format32bppArgb);
                bitmap.SetResolution(96, 96);

                g = Graphics.FromImage(bitmap);
                Point pos = new Point(0, 0);

                foreach (IMaterialModel model in materialModels)
                {
                    if (model.ImageData == null)
                    {
                        _Logger.Log(LogLevel.Warning, $"{model.FileName}'s ImageData is Null");
                        continue;
                    }

                    g.DrawImage(model.ImageData, pos);
                }

                bitmap.Save(ms, ImageFormat.Png);
                _Logger.LogInformation("MaterialOverlaped");

                return ms;
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex.Message, "ShellMaterialFactory", "Overlap");
                return null;
            }
            finally
            {
                g?.Dispose();
                bitmap?.Dispose();
            }
        }

        protected abstract Task<byte[]?> RequestMaterial(string shellName, string materialPath);

        private async Task<ColorableMaterialModel?> createColorMaterialAsync(string shellName, string fileName, MaterialID materialId)
        {
            string materialFileName = $"{materialId}/{fileName}";
            byte[]? imgBytes = await RequestMaterial(shellName, materialFileName);
            if (imgBytes == null)
                return null;

            MemoryStream imgStream = new MemoryStream(imgBytes);
            Bitmap bit = (Bitmap)Bitmap.FromStream(imgStream, false, false);
            bit.SetResolution(96, 96);

            return new ColorableMaterialModel(materialId, materialFileName, bit);
        }
        private async Task<MaterialModel?> createMaterialAsync(string shellName, string fileName, MaterialID materialId)
        {
            string materialFileName = $"{materialId}/{fileName}";
            byte[]? imgBytes = await RequestMaterial(shellName, materialFileName);
            if (imgBytes == null)
                return null;

            MemoryStream imgStream = new MemoryStream(imgBytes);
            Bitmap bit = (Bitmap)Bitmap.FromStream(imgStream, false, false);
            bit.SetResolution(96, 96);

            return new MaterialModel(materialId, materialFileName, bit);
        }
    }

    public sealed class ShellMaterialLocalFactory : ShellMaterialFactoryBase
    {
        readonly string _materialRoot;
        readonly IConfiguration _config;

        public ShellMaterialLocalFactory(ILogger<ShellMaterialFactoryBase> logger, IConfiguration config) : base(logger)
        {
            _config = config;

            _materialRoot = _config.GetSection(WellknownConfigurationSecsions.LocalMaterialRoot)?.Value ?? throw new KeyNotFoundException($"{WellknownConfigurationSecsions.LocalMaterialRoot}");
        }

        protected override async Task<byte[]?> RequestMaterial(string shellName, string materialPath)
        {
            // Allow NotExistFilePath
            string reqPath = $"{_materialRoot}{shellName}/1250/{materialPath}";

            bool isExistPath = System.IO.File.Exists(reqPath);
            if(!isExistPath)
            {
                _Logger.Log(LogLevel.Information, $"NotFound: {reqPath}");
                return null;
            }

            try
            {
                return await System.IO.File.ReadAllBytesAsync(reqPath);
            }
            catch (Exception ex)
            {
                _Logger.Log(LogLevel.Critical, ex, null);
                return null;
            }
        }
    }

    public sealed class ShellMaterialRemoteFactory : ShellMaterialFactoryBase
    {
        readonly string _materialRoot;
        readonly IConfiguration _config;
        readonly HttpClient _client;

        public ShellMaterialRemoteFactory(ILogger<ShellMaterialRemoteFactory> logger, IConfiguration config, HttpClient client) : base(logger)
        {
            _config = config;
            _client = client;

            _materialRoot = _config.GetSection(WellknownConfigurationSecsions.RemoteMaterialRoot)?.Value ?? throw new KeyNotFoundException($"{nameof(WellknownConfigurationSecsions.RemoteMaterialRoot)}");
        }

        protected override async Task<byte[]?> RequestMaterial(string shellName, string materialPath)
        {
            // Allow NotExistFilePath
            string reqPath = $"{_materialRoot}{shellName}/1250/{materialPath}";

            HttpResponseMessage? resMsg = null;

            try
            {
                resMsg = await _client.GetAsync(reqPath);
                if(resMsg.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _Logger.Log(LogLevel.Information, $"NotFound: {reqPath}");
                    return null;
                }

                return await resMsg.Content.ReadAsByteArrayAsync();
            }
            catch(Exception ex)
            {
                _Logger.Log(LogLevel.Critical, ex, null);
                return null;
            }
            finally
            {
                resMsg?.Dispose();
            }
        }
    }
}
