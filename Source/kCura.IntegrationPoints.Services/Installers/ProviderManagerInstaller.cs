using System.Collections.Generic;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Installers;
using kCura.IntegrationPoints.Data.Installers;
using kCura.IntegrationPoints.Services.Repositories;

namespace kCura.IntegrationPoints.Services.Installers
{
	public class ProviderManagerInstaller : IInstaller
	{
		private readonly List<IWindsorInstaller> _dependencies;

		public ProviderManagerInstaller()
		{
			_dependencies = new List<IWindsorInstaller>
			{
				new QueryInstallers(),
				new ServicesInstaller()
			};
		}

		public void Install(IWindsorContainer container, IConfigurationStore store, int workspaceId)
		{
			container.Register(Component.For<IProviderRepository>().ImplementedBy<ProviderRepository>().LifestyleTransient());

			foreach (IWindsorInstaller dependency in _dependencies)
			{
				dependency.Install(container, store);
			}
		}
	}
}