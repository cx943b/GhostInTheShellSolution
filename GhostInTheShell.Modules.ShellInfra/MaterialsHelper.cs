using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GhostInTheShell.Modules.ShellInfra
{
    internal class MaterialsHelper
    {
        public static int GetMaterialIndex(MaterialID id, bool isBrowToFront, bool isEyesToFront)
        {
            int retID = (int)id;

            if (isBrowToFront && !isEyesToFront)
            {
                if (id == MaterialID.face_haircolor || id == MaterialID.face_front)
                    retID += 2;
                else if (id == MaterialID.hair_front_accessory || id == MaterialID.hair_front)
                    retID += 2;
                else if (id == MaterialID.eye || id == MaterialID.eye_color)
                    retID -= 4;
            }
            else if (!isBrowToFront && isEyesToFront)
            {
                if (id == MaterialID.face_haircolor || id == MaterialID.face_front)
                    retID -= 2;
                else if (id == MaterialID.hair_front_accessory || id == MaterialID.hair_front)
                    retID += 2;
            }
            else if (!isBrowToFront && !isEyesToFront)
            {
                if (id == MaterialID.hair_front_accessory || id == MaterialID.hair_front)
                    retID += 4;
                else if (id == MaterialID.face_haircolor || id == MaterialID.face_front)
                    retID -= 2;
                else if (id == MaterialID.eye || id == MaterialID.eye_color)
                    retID -= 2;
            }

            return retID;
        }
    }
}
