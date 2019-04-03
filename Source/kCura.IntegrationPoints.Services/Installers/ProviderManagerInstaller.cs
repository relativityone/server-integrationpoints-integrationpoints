using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Installers;
using kCura.IntegrationPoints.Core.Provider;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Installers;
using kCura.IntegrationPoints.Domain.Authentication;
using kCura.IntegrationPoints.Services.Repositories;
using kCura.IntegrationPoints.Services.Repositories.Implementations;
using Relativity.API;
using System.Collections.Generic;

namespace kCura.IntegrationPoints.Services.Installers
{
	public class ProviderManagerInstaller : Installer
	{
		private readonly List<IWindsorInstaller> _dependencies;

		public ProviderManagerInstaller()
		{
			_dependencies = new List<IWindsorInstaller>
			{
				new QueryInstallers(),
				new SharedAgentInstaller(),
				new ServicesInstaller()
			};
		}

		protected override IList<IWindsorInstaller> Dependencies => _dependencies;

		protected override void RegisterComponents(IWindsorContainer container, IConfigurationStore store, int workspaceId)
		{
			container.Register(Component.For<IRSAPIService>().UsingFactoryMethod(k => new RSAPIService(k.Resolve<IHelper>(), workspaceId), true));
			container.Register(Component.For<IProviderRepository>().ImplementedBy<ProviderRepository>().LifestyleTransient());
			container.Register(Component.For<IAuthTokenGenerator>().ImplementedBy<ClaimsTokenGenerator>().LifestyleTransient());

			container.Register(Component
				.For<IServiceContextHelper>()
				.UsingFactoryMethod(k =>
				{
					IServiceHelper helper = k.Resolve<IServiceHelper>();
					return new ServiceContextHelperForKeplerService(helper, workspaceId);
				})
			);
			container.Register(Component
				.For<IDBContext>()
				.UsingFactoryMethod(k =>
				{
					IServiceHelper helper = k.Resolve<IServiceHelper>();
					return helper.GetDBContext(workspaceId);
				})
			);

			container.Register(Component.For<DeleteIntegrationPoints>().LifestyleTransient());
			container.Register(Component.For<GetApplicationGuid>().ImplementedBy<GetApplicationGuid>().LifestyleTransient());

			container.Register(Component.For<IProviderInstaller>().ImplementedBy<ProviderInstaller>().LifestyleTransient());
			container.Register(Component.For<ProviderUninstaller>().LifestyleTransient());
		}
	}
}