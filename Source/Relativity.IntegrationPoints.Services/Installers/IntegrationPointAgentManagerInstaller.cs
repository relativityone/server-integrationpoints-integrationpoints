using System.Collections.Generic;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace Relativity.IntegrationPoints.Services.Installers
{
	public class IntegrationPointAgentManagerInstaller : Installer
	{
		protected override IList<IWindsorInstaller> Dependencies { get; } = new List<IWindsorInstaller>();

		protected override void RegisterComponents(IWindsorContainer container, IConfigurationStore store, int workspaceID)
		{
			
		}
	}
}