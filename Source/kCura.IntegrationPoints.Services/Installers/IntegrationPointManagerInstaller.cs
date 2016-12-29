using System.Collections.Generic;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Installers;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Installers;
using kCura.IntegrationPoints.Services.Helpers;
using kCura.IntegrationPoints.Services.Repositories;
using kCura.IntegrationPoints.Services.Repositories.Implementations;
using kCura.Relativity.Client;
using Relativity.API;

namespace kCura.IntegrationPoints.Services.Installers
{
	public class IntegrationPointManagerInstaller : Installer
	{
		private readonly List<IWindsorInstaller> _dependencies;

		public IntegrationPointManagerInstaller()
		{
			_dependencies = new List<IWindsorInstaller>
			{
				new QueryInstallers(),
				new KeywordInstaller(),
				new ServicesInstaller(),
				new ValidationInstaller()
			};
		}

		protected override IList<IWindsorInstaller> Dependencies => _dependencies;

		protected override void RegisterComponents(IWindsorContainer container, IConfigurationStore store, int workspaceId)
		{
			container.Register(Component.For<IServiceHelper>().UsingFactoryMethod(k => global::Relativity.API.Services.Helper, true));
			container.Register(Component.For<IHelper>().UsingFactoryMethod(k => k.Resolve<IServiceHelper>(), true));
			container.Register(Component.For<IRSAPIService>().UsingFactoryMethod(k => new RSAPIService(k.Resolve<IHelper>(), workspaceId), true));

			container.Register(Component.For<IUserInfo>().UsingFactoryMethod(k => k.Resolve<IServiceHelper>().GetAuthenticationManager().UserInfo, true));

			container.Register(Component.For<IServiceContextHelper>()
				.UsingFactoryMethod(k =>
				{
					IServiceHelper helper = k.Resolve<IServiceHelper>();
					return new ServiceContextHelperForKelperService(helper, workspaceId);
				}));
			container.Register(
				Component.For<IWorkspaceDBContext>()
					.ImplementedBy<WorkspaceContext>()
					.UsingFactoryMethod(k => new WorkspaceContext(k.Resolve<IHelper>().GetDBContext(workspaceId)))
					.LifeStyle.Transient);

			container.Register(
				Component.For<IRSAPIClient>()
					.UsingFactoryMethod(k =>
					{
						IRSAPIClient client = container.Resolve<IServiceHelper>().GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser);
						client.APIOptions.WorkspaceID = workspaceId;
						return client;
					})
					.LifeStyle.Transient);

			container.Register(Component.For<IServicesMgr>().UsingFactoryMethod(k => global::Relativity.API.Services.Helper.GetServicesManager()));
			container.Register(Component.For<IIntegrationPointRepository>().ImplementedBy<IntegrationPointRepository>().LifestyleTransient());
			container.Register(Component.For<IIntegrationPointProfileRepository>().ImplementedBy<IntegrationPointProfileRepository>().LifestyleTransient());
			container.Register(Component.For<IProviderRepository>().ImplementedBy<ProviderRepository>().LifestyleTransient());
			container.Register(Component.For<IBackwardCompatibility>().ImplementedBy<BackwardCompatibility>().LifestyleTransient());
		}
	}
}