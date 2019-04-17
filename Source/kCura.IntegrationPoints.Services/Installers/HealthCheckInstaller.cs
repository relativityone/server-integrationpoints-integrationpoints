using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Installers;
using kCura.IntegrationPoints.Data.Installers;
using kCura.IntegrationPoints.Services.Helpers;
using System.Collections.Generic;

namespace kCura.IntegrationPoints.Services.Installers
{
	public class HealthCheckInstaller : Installer
	{
		private readonly List<IWindsorInstaller> _dependencies;

		public HealthCheckInstaller()
		{
			_dependencies = new List<IWindsorInstaller>
			{
				new QueryInstallers(),
				new SharedAgentInstaller(),
				new ServicesInstaller()
			};
		}

		protected override IList<IWindsorInstaller> Dependencies => _dependencies;

		protected override void RegisterComponents(IWindsorContainer container, IConfigurationStore store, int workspaceID)
		{
			container.Register(Component.For<IRelativityManagerSoapFactory>().ImplementedBy<RelativityManagerSoapFactory>().LifestyleTransient());
		}
	}
}
