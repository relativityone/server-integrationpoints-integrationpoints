using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Domain.Managers;

namespace kCura.IntegrationPoints.Core
{
	public interface IServiceManagerProvider
	{
		TManager Create<TManager, TFactory>() where TFactory : IServiceManagerFactory<TManager>, new();

		TManager Create<TManager, TFactory>(int? federatedInstanceId, string federatedInstanceCredentials,
			IFederatedInstanceManager federatedInstanceManager)
			where TFactory : IServiceManagerFactory<TManager>, new();
	}
}