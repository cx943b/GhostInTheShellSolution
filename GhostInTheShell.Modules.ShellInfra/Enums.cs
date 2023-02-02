using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GhostInTheShell.Modules.ShellInfra
{
    //public enum ShellModelFactoryMode { Local, Remote }
    public enum ShellModelType { Accessory, Cloth, Eye, Face, Hair, Head, Underwear }

    [Flags]
    public enum ShellPartType : int
    {
        BackHair = 0b_00000000_00000001,
        FrontHair = 0b_00000000_00000010,

        Head = 0b_00000000_00000100,
        Face = 0b_00000000_00001000,
        Eye = 0b_00000000_00010000,

        Cloth = 0b_00000000_00100000,
        Underwear = 0b_00000000_01000000,

        AttachHair = 0b_00000001_00000000,
        AttachHead = 0b_00000010_00000000,
        Earing = 0b_00000100_00000000,
        Glasses = 0b_00001000_01000000,
        Shoes = 0b_00010000_00000000,
        ShoesEx = 0b_00100000_00000000,
        Socks = 0b_01000000_00000000,
        Stocking = 0b_10000000_00000000,
        Etc = 0b_1_00000000_00000000,


        Accessory = AttachHair | AttachHead | Earing | Glasses | Shoes | ShoesEx | Socks | Stocking | Etc,
        Hair = BackHair | FrontHair | AttachHair
    }
    public enum ColorPart { Hair, Eye, Cloth, }
    public enum UnderwearPosition { Upper, Lower }
    public enum HairPosition { Front, Back }
    public enum HeadType { None, Normal, Shyness0, Shyness1, Shyness2, Shyness3, Blue, Angry, ElfNormal, ElfShyness0, ElfShyness1, ElfShyness2, ElfBlue, ElfAngry }
    public enum MaterialID
    {
        // Back
        //Bottom,
        accessory_back,
        hair_back,
        hair_back_accessory,
        body_back,
        accessory_underwear,
        body_front,
        body_front_color,
        body_front_accessory,
        accessory_middle_back,
        head,
        accessory_middle_front,
        face_back,
        hair_front,             // 12
        hair_front_accessory,   // 13
        face_front,             // 14
        face_haircolor,         // 15
        eye,                    // 16
        eye_color,              // 17
        accessory_front
        //Top = accessory_front
        // Front
    }
}
