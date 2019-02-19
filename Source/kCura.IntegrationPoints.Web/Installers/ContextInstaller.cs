using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Web.Context.UserContext;
using kCura.IntegrationPoints.Web.Context.WorkspaceContext;

namespace kCura.IntegrationPoints.Web.Installers
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