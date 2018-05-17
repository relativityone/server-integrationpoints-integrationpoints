using Castle.Windsor;
using Castle.MicroKernel.Registration;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Factories.Implementations
{
	public class DataTransferLocationServiceFactory : IDataTransferLocationServiceFactory
	{
		private IWindsorContainer _container;
		public DataTransferLocationServiceFactory(IWindsorContainer container)
		{
			_container = container;
		}

		public IDataTransferLocationService CreateService(int workspaceId)
		{
			IDataTransferLocationService dtService;

			//if the IServiceContextHelper has not been registered, create a mostly empty one that just has the workspaceID
			if (!_container.Kernel.HasComponent(typeof(IServiceContextHelper)))
			{
				_container.Register(Component.For<IServiceContextHelper>().UsingFactoryMethod(
				k =>
					new ServiceContextHelperForLoadFileReader(workspaceId)));
			}
			dtService = new DataTransferLocationService(
				_container.Resolve<IHelper>(),
				_container.Resolve<IIntegrationPointTypeService>(),
				_container.Resolve<SystemInterface.IO.IDirectory>());
			return dtService;
		}
	}
}
