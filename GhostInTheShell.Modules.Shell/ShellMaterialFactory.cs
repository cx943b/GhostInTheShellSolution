using GhostInTheShell.Modules.Shell.Models;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace GhostInTheShell.Modules.Shell
{
    public abstract class ShellMaterialFactoryBase
    {
        readonly ILogger _logger;

        public ShellMaterialFactoryBase(ILogger<ShellMaterialFactoryBase> logger)
        {
            _logger = logger;
        }

        public async Task<bool> LoadMaterial(string shellName, ShellModelBase shellModel)
        {
            if (shellModel.ColorMaterials == null && shellModel.ColorIDs != null)
            {
                List<ColorableMaterialModel> lstModels = new List<ColorableMaterialModel>();

                foreach (MaterialID matID in shellModel.ColorIDs)
                {
                    byte[] imgBytes = await RequestMaterial(shellName, $"{matID}\\{shellModel.FileName}");
                    if (imgBytes == null)
                        continue;

                    MemoryStream imgStream = new MemoryStream(imgBytes);
                    Bitmap bit = (Bitmap)Bitmap.FromStream(imgStream, false, false);
                    bit.SetResolution(96, 96);

                    ColorableMaterialModel matModel = new ColorableMaterialModel(matID, shellModel.FileName, bit);
                    lstModels.Add(matModel);
                }

                shellModel.ColorMaterials = lstModels.ToArray();
            }

            if (shellModel.Materials == null && shellModel.NormalIDs != null)
            {
                List<MaterialModel> lstModels = new List<MaterialModel>();

                try
                {
                    foreach (MaterialID matID in shellModel.NormalIDs)
                    {
                        byte[] imgBytes = await RequestMaterial(shellName, $"{matID}\\{shellModel.FileName}");
                        if (imgBytes == null)
                            continue;

                        MemoryStream imgStream = new MemoryStream(imgBytes);
                        Bitmap bit = (Bitmap)Bitmap.FromStream(imgStream, false, false);
                        bit.SetResolution(96, 96);

                        MaterialModel matModel = new MaterialModel(matID, shellModel.FileName, bit);
                        lstModels.Add(matModel);
                    }

                    shellModel.Materials = lstModels.ToArray();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
            }

            return true;
        }
        public MemoryStream? Overlap(IOrderedEnumerable<IMaterialModel> materialModels, Size shellSize)
        {
            if (materialModels == null)
                return null;
            //throw new ArgumentNullException("Overlap: materialModels");

            if (!materialModels.Any())
                return null;
            //throw new ArgumentException("Overlap: materialModels, Count: 0");

            try
            {
                MemoryStream ms = new MemoryStream();

                using (Bitmap bitmap = new Bitmap(shellSize.Width, shellSize.Height, PixelFormat.Format32bppArgb))
                {
                    bitmap.SetResolution(96, 96);

                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        Point pos = new Point(0, 0);
                        foreach (IMaterialModel model in materialModels)
                        {
                            if (model.ImageData == null)
                            {
                                Console.WriteLine();
                            }

                            g.DrawImage(model.ImageData, pos);
                        }


                        bitmap.Save(ms, ImageFormat.Png);
                    }
                }

                _logger.LogInformation("MaterialOverlaped");

                return ms;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, "ShellMaterialFactory", "Overlap");
                return null;
            }
        }

        protected abstract Task<byte[]> RequestMaterial(string shellName, string materialPath);
    }

    public sealed class ShellMaterialFactory : ShellMaterialFactoryBase
    {
        const string MaterialRootSectionName = "ShellData:Dav:MaterialRoot";

        public ShellMaterialFactory(ILogger<ShellMaterialFactory> logger) : base(logger)
        {
        }

        protected override async Task<byte[]> RequestMaterial(string shellName, string materialPath)
        {
            return null;
        }
    }
}
