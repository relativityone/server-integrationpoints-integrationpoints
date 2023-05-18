using System;
using System.ComponentModel;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Relativity.Sync.Configuration;

using SyncFieldMapType = Relativity.Sync.Storage.FieldMapType;

namespace kCura.IntegrationPoints.RelativitySync.Utils
{
    public static class EnumConvertExtensions
    {
        public static ImportImageFileCopyMode ToSyncImageMode(this ImportNativeFileCopyModeEnum importNativeFileCopyMode)
        {
            if (importNativeFileCopyMode == ImportNativeFileCopyModeEnum.CopyFiles)
            {
                return ImportImageFileCopyMode.CopyFiles;
            }
            else
            {
                return ImportImageFileCopyMode.SetFileLinks;
            }
        }

        public static ImportNativeFileCopyMode ToSyncNativeMode(this ImportNativeFileCopyModeEnum importNativeFileCopyMode)
        {
            switch (importNativeFileCopyMode)
            {
                case ImportNativeFileCopyModeEnum.CopyFiles:
                    return ImportNativeFileCopyMode.CopyFiles;
                case ImportNativeFileCopyModeEnum.SetFileLinks:
                    return ImportNativeFileCopyMode.SetFileLinks;
                case ImportNativeFileCopyModeEnum.DoNotImportNativeFiles:
                    return ImportNativeFileCopyMode.DoNotImportNativeFiles;
                default:
                    throw new InvalidEnumArgumentException(nameof(importNativeFileCopyMode));
            }
        }

        public static SyncFieldMapType ToSyncFieldMapType(this FieldMapTypeEnum fieldMapType)
        {
            switch (fieldMapType)
            {
                case FieldMapTypeEnum.Identifier:
                    return SyncFieldMapType.Identifier;
                default:
                    return SyncFieldMapType.None;
            }
        }

        public static ImportOverwriteMode ToSyncImportOverwriteMode(this ImportOverwriteModeEnum importOverWriteMode)
        {
            switch (importOverWriteMode)
            {
                case ImportOverwriteModeEnum.AppendOnly:
                    return ImportOverwriteMode.AppendOnly;
                case ImportOverwriteModeEnum.AppendOverlay:
                    return ImportOverwriteMode.AppendOverlay;
                case ImportOverwriteModeEnum.OverlayOnly:
                    return ImportOverwriteMode.OverlayOnly;
                default:
                    throw new InvalidEnumArgumentException(nameof(importOverWriteMode));
            }
        }

        public static FieldOverlayBehavior ToSyncFieldOverlayBehavior(this string fieldOverlayBehavior)
        {
            switch (fieldOverlayBehavior)
            {
                case ImportSettings.FIELDOVERLAYBEHAVIOR_DEFAULT:
                    return FieldOverlayBehavior.UseFieldSettings;
                case ImportSettings.FIELDOVERLAYBEHAVIOR_MERGE:
                    return FieldOverlayBehavior.MergeValues;
                case ImportSettings.FIELDOVERLAYBEHAVIOR_REPLACE:
                    return FieldOverlayBehavior.ReplaceValues;
                default:
                    throw new ArgumentException(fieldOverlayBehavior);
            }
        }
    }
}
