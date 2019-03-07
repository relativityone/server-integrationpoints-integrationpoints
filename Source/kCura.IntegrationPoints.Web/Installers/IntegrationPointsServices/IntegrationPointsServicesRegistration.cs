using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
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
	public static class IntegrationPointsServicesRegistration
	{
		public static IWindsorContainer AddIntegrationPointsServices(this IWindsorContainer container)
		{
			return container
				.RegisterCoreIntegrationPointsServices()
				.RegisterHelpers()
				.AddLoggingContext();
		}

		private static IWindsorContainer RegisterCoreIntegrationPointsServices(this IWindsorContainer container)
		{
			return container.Register(
				Component
					.For<IServiceContextHelper>()
					.ImplementedBy<ServiceContextHelperForWeb>()
					.LifestyleTransient(),
				Component
					.For<IRsapiClientWithWorkspaceFactory>()
					.ImplementedBy<RsapiClientWithWorkspaceFactory>()
					.LifestyleTransient(),
				Component
					.For<WebClientFactory>()
					.UsingFactoryMethod(WebClientFactoryFactory)
					.LifestylePerWebRequest(),
				Component
					.For<IWorkspaceDBContext>()
					.ImplementedBy<Data.WorkspaceContext>()
					.UsingFactoryMethod(k => new Data.WorkspaceContext(k.Resolve<WebClientFactory>().CreateDbContext()))
					.LifestyleTransient(),
				Component
					.For<IAuthTokenGenerator>()
					.ImplementedBy<ClaimsTokenGenerator>()
					.LifestyleTransient()
			);
		}

		private static IWindsorContainer RegisterHelpers(this IWindsorContainer container)
		{
			return container.Register(
				Component
					.For<IFolderTreeBuilder>()
					.ImplementedBy<FolderTreeBuilder>()
					.LifestyleTransient()
			);
		}

		private static WebClientFactory WebClientFactoryFactory(IKernel kernel)
		{
			IHelper helper = kernel.Resolve<IHelper>();
			IRsapiClientWithWorkspaceFactory rsapiClientFactory = kernel.Resolve<IRsapiClientWithWorkspaceFactory>();
			IWorkspaceContext workspaceIdProvider = kernel.Resolve<IWorkspaceContext>();
			return new WebClientFactory(helper, rsapiClientFactory, workspaceIdProvider);
		}
	}
}