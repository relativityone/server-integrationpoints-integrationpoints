using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Helpers
{
	public interface IJobInfoFactory
	{
		IJobInfo Create(Job job);
	}
}
