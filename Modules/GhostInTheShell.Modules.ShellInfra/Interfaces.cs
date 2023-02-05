using GhostInTheShell.Modules.InfraStructure;
using GhostInTheShell.Modules.ShellInfra.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using System;
using System.Drawing;
using System.IO;
using System.Linq;

namespace GhostInTheShell.Modules.ShellInfra
{
    public interface IColorable
    {
        bool ChangeColor(Hsl hslColor);
        void ChangeDefaultColor();
    }

    public interface ICharacter
    {
        string? ShellName { get; }
        Size ShellSize { get; }
    }
    //public interface IShellRemoteService : IDisposable
    //{
    //    Task<XmlReader> RequestInitializeDataAsync(string shellName);
    //    Task<byte[]> RequestMaterialAsync(string shellName, string materialPath);
    //    Task<(int width, int height)> RequestMaterialSizeAsync(string shellName);
    //    Task<XmlReader> RequestTableAsync(string shellName, string tableName);
    //}

    public interface ICharacterClientService
    {
        Task<byte[]?> RequestCharacterImage(string headLabel, string eyeLabel, string faceLabel);
        Task<Size> RequestCharacterSize();
    }
    public interface ICharacterService
    {
        Task AddAccessories(IEnumerable<AccessoryAddPair> dicAccessoryParts);
        bool ChangeAccessoryColor(ShellPartType partType, Hsl hslColor);

        void ChangeDefaultAccessoryColor(ShellPartType partType);
        void ChangeDefaultPartColor(ShellPartType partType);

        Task ChangeParts(IDictionary<ShellPartType, string> dicParts);
        Task ChangeShell(IDictionary<ShellPartType, string> dicParts, IEnumerable<AccessoryAddPair> dicAccessoryParts);
        Task<bool> InitializeAsync(string shellName);
        bool RemoveAccessory(ShellPartType accessoryType, string oldAccessoryLabel);
    }

    public interface IShellModelFactory
    {
        IEnumerable<string> GetLabels(ShellPartType partType);
        IEnumerable<ShellModelBase> GetModels(ShellPartType partType, string label);
        Task<bool> InitializeAsync(string shellName);
    }

    public interface IShellMaterialFactory
    {
        Task<bool> LoadMaterial(string shellName, ShellModelBase shellModel);
        MemoryStream? Overlap(IOrderedEnumerable<IMaterialModel> materialModels, Size shellSize);
        void UnloadMaterial(ShellModelBase shellModel);
    }
}
