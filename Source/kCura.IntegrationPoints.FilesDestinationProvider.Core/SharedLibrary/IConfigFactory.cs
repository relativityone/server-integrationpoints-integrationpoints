using kCura.IntegrationPoints.Config;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
	public interface IConfigFactory
	{
		IConfig Create();
	}
}