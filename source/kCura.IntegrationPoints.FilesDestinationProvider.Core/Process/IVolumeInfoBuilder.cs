using kCura.WinEDDS;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Process
{
	public interface IVolumeInfoBuilder
	{
		void SetVolumeInfo(ExportSettings exportSettings, ExportFile exportFile);
	}
}