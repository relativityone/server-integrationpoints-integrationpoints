using kCura.IntegrationPoints.Synchronizers.RDO;
using System.ComponentModel;
using static kCura.IntegrationPoints.Core.Constants;

namespace kCura.IntegrationPoints.Core.Utils
{
    public static class NameToEnumConvert
    {
        public static ImportOverwriteModeEnum GetEnumByModeName(string overwriteModeName)
        {
            switch (overwriteModeName)
            {
                case OverwriteModeNames.AppendOnlyModeName:
                    return ImportOverwriteModeEnum.AppendOnly;
                case OverwriteModeNames.OverlayOnlyModeName:
                    return ImportOverwriteModeEnum.OverlayOnly;
                case OverwriteModeNames.AppendOverlayModeName:
                    return ImportOverwriteModeEnum.AppendOverlay;
                default:
                    throw new InvalidEnumArgumentException(nameof(overwriteModeName));
            }
        }
    }
}
