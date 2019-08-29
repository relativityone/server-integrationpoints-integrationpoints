using kCura.IntegrationPoints.Core.Factories;

namespace kCura.IntegrationPoints.Core
{
	public interface IServiceManagerProvider
	{
		TManager Create<TManager, TFactory>() where TFactory : IServiceManagerFactory<TManager>, new();
	}
}