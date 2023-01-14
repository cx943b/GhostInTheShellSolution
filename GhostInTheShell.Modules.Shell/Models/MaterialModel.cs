using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GhostInTheShell.Modules.Shell.Models
{
    public interface IMaterialModel : IEquatable<IMaterialModel>
    {
        Bitmap ImageData { get; }
        MaterialID Id { get; }

        string FileName { get; }
        string RelativePath { get; }

        int MainIndex { get; set; }
        int SubIndex { get; set; }
    }

    [Serializable]
    public class MaterialModel : IMaterialModel
    {
        protected readonly Bitmap _bitmap;

        public virtual Bitmap ImageData => _bitmap;
        public MaterialID Id { get; init; }
        public string FileName { get; init; }
        public string RelativePath { get; init; }

        public int MainIndex { get; set; }
        public int SubIndex { get; set; }

        public MaterialModel(MaterialID id, string fileName, Bitmap? bitmap)
        {
            if(bitmap is null) throw new ArgumentNullException(nameof(bitmap));
            if(String.IsNullOrEmpty(fileName)) throw new ArgumentNullException(nameof(fileName));

            Id = id;
            MainIndex = (int)id;

            FileName = fileName;
            RelativePath = string.Format("\\{0}\\{1}", Id, FileName);

            _bitmap = bitmap;
        }

        public virtual void Dispose() => _bitmap.Dispose();

        public override string ToString() => $"Material: {RelativePath}";
        public override int GetHashCode() => RelativePath.GetHashCode();
        public override bool Equals(object? obj)
        {
            MaterialModel? model = obj as MaterialModel;
            if(model is not null)
                return Equals(model);

            return false;
        }

        public bool Equals(IMaterialModel? other)
        {
            if (other is null)
                return false;

            return this.Id == other.Id && String.Compare(this.FileName, other.FileName, true) == 0;
        }
    }
}
