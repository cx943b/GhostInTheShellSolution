using GhostInTheShell.Modules.InfraStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GhostInTheShell.Modules.Shell.Models
{
    [Serializable]
    public abstract class ShellModelBase : IColorable
    {
        readonly string _Label;
        protected readonly string _FileName;
        protected readonly MaterialID[] _NormalIDs;
        protected readonly MaterialID[] _ColorIDs;

        protected MaterialModel[]? _Materials;
        protected ColorableMaterialModel[]? _ColorMaterials;

        public bool IsColorable => _ColorMaterials != null;
        public string Label => _Label;
        public virtual string FileName => _FileName;
        public MaterialID[] NormalIds => _NormalIDs;
        public MaterialID[] ColorIds => _ColorIDs;

        public MaterialModel[]? Materials
        {
            get { return _Materials; }
            set { _Materials = value; }
        }
        public ColorableMaterialModel[]? ColorMaterials
        {
            get { return _ColorMaterials; }
            set { _ColorMaterials = value; }
        }

        public ShellModelBase(string label, string fileName, MaterialID[] normalIDs, MaterialID[] colorIDs)
        {
            if (normalIDs == null && colorIDs == null)
                throw new InvalidOperationException();

            _Label = label;
            _FileName = fileName;

            _NormalIDs = normalIDs;
            _ColorIDs = colorIDs;
        }

        /// <summary>
        /// DelayExecute
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetRelativePaths()
        {
            var normalPaths = (_NormalIDs == null ? Enumerable.Empty<string>() : _NormalIDs.Select(nID => $"\\{nID}\\{_FileName}" + (_FileName.EndsWith("png") ? "" : ".png")));
            var colorPaths = (_ColorIDs == null ? Enumerable.Empty<string>() : ColorIds.Select(cID => $"\\{cID}\\{_FileName}" + (_FileName.EndsWith("png") ? "" : ".png")));

            return normalPaths.Concat(colorPaths);
        }

        /// <summary>
        /// Concat materials
        /// </summary>
        /// <returns>Lazy load Enumerables</returns>
        public IEnumerable<IMaterialModel> GetMaterials()
        {
            var materials = _ColorMaterials?.Cast<IMaterialModel>() ?? Enumerable.Empty<IMaterialModel>();
            return materials.Concat(_Materials?.Cast<IMaterialModel>() ?? Enumerable.Empty<IMaterialModel>());

            //List<IMaterialModel> lst = new List<IMaterialModel>();
            //if (_ColorMaterials != null)
            //    lst.AddRange(_ColorMaterials);
            //if (_Materials != null)
            //    lst.AddRange(_Materials);

            //return lst.AsEnumerable();
        }

        public bool ChangeColor(Hsl hslColor)
        {
            if (_ColorMaterials is null)
                return false;

            foreach (ColorableMaterialModel model in _ColorMaterials)
                model.ChangeColor(hslColor);

            return true;
        }
        public void ChangeDefaultColor()
        {
            if (_ColorMaterials is null)
                return;

            foreach (ColorableMaterialModel model in _ColorMaterials)
                model.ChangeDefaultColor();
        }

        /*public virtual IMaterialModel[] CreateMaterials(ShellMaterialFactory factory)
        {
            var normalMaterials = (_NormalIDs == null ? Enumerable.Empty<IMaterialModel>() : _NormalIDs.Select(nID => factory.CreateMaterialModel(nID, FileName, false)));//.Where(materialModel => materialModel != null));
            var colorMaterials = (_ColorIDs == null ? Enumerable.Empty<IMaterialModel>() : _ColorIDs.Select(cID => factory.CreateMaterialModel(cID, FileName, false)));//.Where(materialModel => materialModel != null);

            return normalMaterials.Concat(colorMaterials).ToArray();
        }*/
    }

    [Serializable]
    public class HairModel : ShellModelBase
    {
        public HairPosition Position { get; private set; }

        public HairModel(string label, string fileName, MaterialID[] normalIDs, MaterialID[] colorIDs, HairPosition hairPos)
            : base(label, fileName, normalIDs, colorIDs)
        {
            Position = hairPos;
        }
    }
    [Serializable]
    public class HeadModel : ShellModelBase
    {
        public HeadType Type { get; private set; }

        public HeadModel(string label, string fileName, MaterialID[] normalIDs, MaterialID[] colorIDs, HeadType type)
            : base(label, fileName, normalIDs, colorIDs)
        {
            Type = type;
        }
    }
    [Serializable]
    public class FaceModel : ShellModelBase
    {
        bool _IsMouthMakeup;

        public bool IsMouthMakeup
        {
            get { return _IsMouthMakeup; }
            set
            {
                if (_IsMouthMakeup != value)
                    _IsMouthMakeup = value;
            }
        }

        public override string FileName => (_IsMouthMakeup ? "s" : "") + _FileName + (_FileName.EndsWith("png") ? "" : ".png");


        public FaceModel(string label, string fileName, MaterialID[] normalIDs, MaterialID[] colorIDs)
            : base(label, fileName, normalIDs, colorIDs)
        {
        }
        public FaceModel(string label, string fileName, MaterialID[] normalIDs, MaterialID[] colorIDs, bool isMouthMakeup)
            : base(label, fileName, normalIDs, colorIDs)
        {
            _IsMouthMakeup = isMouthMakeup;
        }
    }
    [Serializable]
    public class EyeModel : ShellModelBase
    {
        bool _IsEyeMakeup;

        public bool IsEyeMakeup
        {
            get { return _IsEyeMakeup; }
            set { _IsEyeMakeup = value; }
        }
        public override string FileName => (_FileName.IndexOf("共通") >= 0 ? _FileName : (_IsEyeMakeup ? _FileName.Replace("たれ", "吊り") : _FileName)) + (_FileName.EndsWith("png") ? "" : ".png");

        public EyeModel(string label, string fileName, MaterialID[] normalIDs, MaterialID[] colorIDs, bool isEyeModel = false)
            : base(label, fileName, normalIDs, colorIDs)
        {
            _IsEyeMakeup = isEyeModel;
        }
    }

    [Serializable]
    public class UnderwearModel : ShellModelBase
    {
        public UnderwearModel(string label, string fileName, MaterialID[] normalIDs, MaterialID[] colorIDs)
            : base(label, fileName, normalIDs, colorIDs)
        {
        }
    }
    [Serializable]
    public class AccessoryModel : ShellModelBase
    {
        int _Index;

        public string TypeEn { get; private set; }
        public string TypeKr { get; private set; }

        public int Index
        {
            get { return _Index; }
            set
            {
                if (_Index != value)
                {
                    _Index = value;

                    if (_ColorMaterials != null)
                    {
                        foreach (MaterialModel matModel in _ColorMaterials)
                            matModel.SubIndex = _Index;
                    }
                }
            }
        }

        public AccessoryModel(string label, string fileName, MaterialID[] normalIDs, MaterialID[] colorIDs, string typeKr, string typeEn)
            : base(label, fileName, normalIDs, colorIDs)
        {
            TypeKr = typeKr;
            TypeEn = typeEn;
        }

        public override string ToString()
        {
            return $"[{TypeKr}] {Label}";
        }
    }
    [Serializable]
    public class ClothModel : ShellModelBase
    {
        public ClothModel(string label, string fileName, MaterialID[] normalIDs, MaterialID[] colorIDs, bool useUnderwear, bool usePad) : base(label, fileName, normalIDs, colorIDs)
        {
            UsePad = usePad;
            UseUnderwear = useUnderwear;
        }

        public bool UsePad { get; private set; }
        public bool UseUnderwear { get; private set; } = true;
    }

    [Serializable]
    public class CuppableClothModel : ClothModel
    {
        readonly char _Cup;

        public char Cup => _Cup;

        public override string FileName => $"{_Cup}{base.FileName}";

        public CuppableClothModel(string label, string fileName, char cup, MaterialID[] normalIDs, MaterialID[] colorIDs, bool useUnderwear, bool usePad)
            : base(label, fileName, normalIDs, colorIDs, usePad, useUnderwear)
        {
            _Cup = cup;
        }
    }
}
