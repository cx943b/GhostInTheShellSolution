using System;

namespace GhostInTheShell.Modules.Shell
{
    public class AccessoryAddPair
    {
        public string PartLabel { get; init; }
        public ShellPartType PartType { get; init; }

        public AccessoryAddPair(ShellPartType partType, string partLabel)
        {
            if (String.IsNullOrEmpty(partLabel))
                throw new ArgumentNullException(nameof(partLabel));

            PartLabel = partLabel;
            PartType = partType;
        }
    }
}
