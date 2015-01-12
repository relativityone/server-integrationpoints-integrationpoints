using Castle.Windsor;
using kCura.IntegrationPoints.Contracts.Syncronizer;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Component = Castle.MicroKernel.Registration.Component;

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
			_container.Register(Component.For<RelativityFieldQuery>().ImplementedBy<RelativityFieldQuery>().LifestyleTransient());
			//_container.Install(FromAssembly.InThisApplication());
		}

		public IDataSyncronizer GetSyncronizer()
		{
			return _container.Kernel.Resolve<kCura.IntegrationPoints.Synchronizers.RDO.RdoSynchronizer>();
		}
	}
}
