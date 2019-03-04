using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Core.Helpers.Implementations;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Authentication;
using kCura.IntegrationPoints.Web.Context.WorkspaceContext;
using kCura.IntegrationPoints.Web.IntegrationPointsServices;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.Installers.IntegrationPointsServices
{
	public class IntegrationPointsServicesInstaller : IWindsorInstaller
	{
		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			RegisterCoreIntegrationPointsServices(container);
			RegisterHelpers(container);

			container.AddLoggingContext();
		}

		private void RegisterCoreIntegrationPointsServices(IWindsorContainer container)
		{
			container.Register(Component
				.For<IServiceContextHelper>()
				.ImplementedBy<ServiceContextHelperForWeb>()
				.LifestyleTransient()
			);
			container.Register(Component
				.For<IRsapiClientWithWorkspaceFactory>()
				.ImplementedBy<RsapiClientWithWorkspaceFactory>()
				.LifestyleTransient()
			);
			container.Register(Component
				.For<WebClientFactory>()
				.UsingFactoryMethod(WebClientFactoryFactory)
				.LifestylePerWebRequest()
			);
			container.Register(Component
				.For<IWorkspaceDBContext>()
				.ImplementedBy<Data.WorkspaceContext>()
				.UsingFactoryMethod(k => new Data.WorkspaceContext(k.Resolve<WebClientFactory>().CreateDbContext()))
				.LifestyleTransient()
			);
			container.Register(Component
				.For<IAuthTokenGenerator>()
				.ImplementedBy<ClaimsTokenGenerator>()
				.LifestyleTransient()
			);
		}

		private void RegisterHelpers(IWindsorContainer container)
		{
			container.Register(Component.For<IFolderTreeBuilder>().ImplementedBy<FolderTreeBuilder>().LifestyleTransient());
		}

		private WebClientFactory WebClientFactoryFactory(IKernel kernel)
		{
			IHelper helper = kernel.Resolve<IHelper>();
			IRsapiClientWithWorkspaceFactory rsapiClientFactory = kernel.Resolve<IRsapiClientWithWorkspaceFactory>();
			IWorkspaceContext workspaceIdProvider = kernel.Resolve<IWorkspaceContext>();
			return new WebClientFactory(helper, rsapiClientFactory, workspaceIdProvider);
		}
	}
}