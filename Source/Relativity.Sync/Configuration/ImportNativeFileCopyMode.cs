using kCura.Relativity.DataReaderClient;

namespace Relativity.Sync.Configuration
{
	internal enum ImportNativeFileCopyMode
	{
		DoNotImportNativeFiles = NativeFileCopyModeEnum.DoNotImportNativeFiles,
		CopyFiles = NativeFileCopyModeEnum.CopyFiles,
		SetFileLinks = NativeFileCopyModeEnum.SetFileLinks
	}
}