using Castle.Windsor;
using kCura.IntegrationPoints.Contracts.Syncronizer;
using kCura.IntegrationPoints.Synchronizers.RDO;

namespace kCura.IntegrationPoints.Core.Services.Syncronizer
{
	public interface IDataSyncronizerFactory
	{
		IDataSyncronizer GetSyncronizer();
	}

	public class MockFactory : IDataSyncronizerFactory
	{
		private readonly IWindsorContainer _container;
		public MockFactory(IWindsorContainer container)
		{
			_container = container;
		}

		public IDataSyncronizer GetSyncronizer()
		{
			return _container.Resolve<RdoSynchronizer>();
		}
	}
}
