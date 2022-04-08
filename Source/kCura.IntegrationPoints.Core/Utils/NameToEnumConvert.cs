using kCura.IntegrationPoints.Synchronizers.RDO;
using System.ComponentModel;

namespace kCura.IntegrationPoints.Core.Utils
{
    public static class NameToEnumConvert
    {
        public static ImportOverwriteModeEnum GetEnumByModeName(string overwriteModeName)
        {
            switch (overwriteModeName)
            {
                case @"Append Only":
                    return ImportOverwriteModeEnum.AppendOnly;
                case @"Overlay Only":
                    return ImportOverwriteModeEnum.OverlayOnly;
                case @"Append/Overlay":
                    return ImportOverwriteModeEnum.AppendOverlay;
                default:
                    throw new InvalidEnumArgumentException(nameof(overwriteModeName));
            }
        }
    }
}
