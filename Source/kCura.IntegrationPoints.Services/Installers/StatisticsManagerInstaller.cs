using System.Collections.Generic;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Installers;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Installers;
using kCura.IntegrationPoints.Domain.Authentication;
using Relativity.API;

namespace kCura.IntegrationPoints.Services.Installers
{
	public class StatisticsManagerInstaller : Installer
	{
		private readonly List<IWindsorInstaller> _dependencies;

		public StatisticsManagerInstaller()
		{
			_dependencies = new List<IWindsorInstaller>
			{
				new QueryInstallers(),
				new ServicesInstaller()
			};
		}

		protected override IList<IWindsorInstaller> Dependencies => _dependencies;

		protected override void RegisterComponents(IWindsorContainer container, IConfigurationStore store, int workspaceId)
		{
			container.Register(Component.For<IServiceHelper>().UsingFactoryMethod(k => global::Relativity.API.Services.Helper, true));
			container.Register(Component.For<IHelper>().UsingFactoryMethod(k => k.Resolve<IServiceHelper>(), true));
			container.Register(Component.For<IRSAPIService>().UsingFactoryMethod(k => new RSAPIService(k.Resolve<IHelper>(), workspaceId), true));
			container.Register(Component.For<IRsapiClientWithWorkspaceFactory>().ImplementedBy<RsapiClientWithWorkspaceFactory>().LifestyleTransient());
			container.Register(Component.For<IServiceContextHelper>()
				.UsingFactoryMethod(k =>
				{
					var helper = k.Resolve<IServiceHelper>();
					var rsapiClientFactory = k.Resolve<IRsapiClientWithWorkspaceFactory>();
					return new ServiceContextHelperForKeplerService(helper, workspaceId, rsapiClientFactory);
				}));
			container.Register(Component.For<IAuthTokenGenerator>().ImplementedBy<ClaimsTokenGenerator>().LifestyleTransient());
		}
	}
}