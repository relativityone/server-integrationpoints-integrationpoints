using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Core.Helpers.Implementations;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Authentication;
using kCura.IntegrationPoints.Web.Context.WorkspaceIdProvider;
using kCura.IntegrationPoints.Web.IntegrationPointsServices.Logging;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.IntegrationPointsServices
{
	public class IntegrationPointsServicesInstaller : IWindsorInstaller
	{
		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			RegisterHelpers(container);
			RegisterCoreIntegrationPointsServices(container);

			container.AddLoggingContext();
		}

		private void RegisterHelpers(IWindsorContainer container)
		{
			container.Register(Component.For<IFolderTreeBuilder>().ImplementedBy<FolderTreeBuilder>().LifestyleTransient());
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
				.ImplementedBy<WorkspaceContext>()
				.UsingFactoryMethod(k => new WorkspaceContext(k.Resolve<WebClientFactory>().CreateDbContext()))
				.LifestyleTransient()
			);
			container.Register(Component
				.For<IAuthTokenGenerator>()
				.ImplementedBy<ClaimsTokenGenerator>()
				.LifestyleTransient()
			);
		}

		private WebClientFactory WebClientFactoryFactory(IKernel kernel)
		{
			IHelper helper = kernel.Resolve<IHelper>();
			IRsapiClientWithWorkspaceFactory rsapiClientFactory = kernel.Resolve<IRsapiClientWithWorkspaceFactory>();
			IWorkspaceIdProvider workspaceIdProvider = kernel.Resolve<IWorkspaceIdProvider>();
			return new WebClientFactory(helper, rsapiClientFactory, workspaceIdProvider);
		}
	}
}