using System;
using System.Collections.Generic;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Installers;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Installers;
using kCura.Relativity.Client;
using Relativity.API;

namespace kCura.IntegrationPoints.Services
{
	public class ServiceInstaller
	{
		private readonly int _caseArtifactId;
		private readonly List<IWindsorInstaller> _dependencies;

		public ServiceInstaller(int caseArtifactId)
		{
			_caseArtifactId = caseArtifactId;

			_dependencies = new List<IWindsorInstaller>
			{
				new QueryInstallers(), new KeywordInstaller(), new ServicesInstaller()
			};
		}

		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			container.Register(Component.For<IServiceHelper>().UsingFactoryMethod(k => global::Relativity.API.Services.Helper, managedExternally: true));
			container.Register(Component.For<IHelper>().UsingFactoryMethod(k => k.Resolve<IServiceHelper>(), managedExternally: true));
			container.Register(Component.For<IServiceContextHelper>()
				.UsingFactoryMethod(k =>
				{
					IServiceHelper helper = k.Resolve<IServiceHelper>();
					return new ServiceContextHelperForKelperService(helper, _caseArtifactId);
				}));
			container.Register(
				Component.For<IWorkspaceDBContext>()
					.ImplementedBy<WorkspaceContext>()
					.UsingFactoryMethod(k => new WorkspaceContext(k.Resolve<IHelper>().GetDBContext(_caseArtifactId)))
					.LifeStyle.Transient);

			container.Register(
				Component.For<IRSAPIClient>()
				.UsingFactoryMethod(k =>
				{
					IRSAPIClient client = container.Resolve<IServiceHelper>().GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser);
					client.APIOptions.WorkspaceID = _caseArtifactId;
					return client;
				})
				.LifeStyle.Transient);

			container.Register(Component.For<IServicesMgr>().UsingFactoryMethod(k => global::Relativity.API.Services.Helper.GetServicesManager()));

			foreach (IWindsorInstaller dependency in _dependencies)
			{
				dependency.Install(container, store);
			}
		}
	}
}