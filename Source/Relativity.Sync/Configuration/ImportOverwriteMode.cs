using kCura.Relativity.DataReaderClient;

namespace Relativity.Sync.Configuration
{
	internal enum ImportOverwriteMode
	{
		AppendOnly = OverwriteModeEnum.Append,
		OverlayOnly = OverwriteModeEnum.Overlay,
		AppendOverlay = OverwriteModeEnum.AppendOverlay,
	}
}