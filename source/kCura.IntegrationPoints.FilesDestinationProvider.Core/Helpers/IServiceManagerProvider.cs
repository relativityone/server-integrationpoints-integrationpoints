using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Helpers
{
	public interface IServiceManagerProvider
	{
		TManager Create<TManager, TFactory>() where TFactory : IManagerFactory<TManager>, new();
	}
}