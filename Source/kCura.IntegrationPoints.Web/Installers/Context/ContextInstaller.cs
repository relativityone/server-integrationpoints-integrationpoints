using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace kCura.IntegrationPoints.Web.Installers.Context
{
	public class ContextInstaller : IWindsorInstaller
	{
		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			container
				.AddWorkspaceContext()
				.AddUserContext();
		}
	}
}