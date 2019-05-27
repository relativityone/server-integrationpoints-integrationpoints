using kCura.Relativity.DataReaderClient;

namespace Relativity.Sync.Configuration
{
	/// <summary>
	/// Determines import native file copy mode.
	/// </summary>
	public enum ImportNativeFileCopyMode
	{
		/// <summary>
		/// Disable import of natives.
		/// </summary>
		DoNotImportNativeFiles = NativeFileCopyModeEnum.DoNotImportNativeFiles,

		/// <summary>
		/// Copy files.
		/// </summary>
		CopyFiles = NativeFileCopyModeEnum.CopyFiles,

		/// <summary>
		/// Links only.
		/// </summary>
		SetFileLinks = NativeFileCopyModeEnum.SetFileLinks
	}
}