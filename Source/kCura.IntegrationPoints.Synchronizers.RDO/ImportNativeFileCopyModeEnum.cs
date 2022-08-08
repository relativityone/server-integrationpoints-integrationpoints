using kCura.Relativity.DataReaderClient;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
    public enum ImportNativeFileCopyModeEnum
    {
        DoNotImportNativeFiles = NativeFileCopyModeEnum.DoNotImportNativeFiles,
        SetFileLinks = NativeFileCopyModeEnum.SetFileLinks,
        CopyFiles = NativeFileCopyModeEnum.CopyFiles
    }
}
