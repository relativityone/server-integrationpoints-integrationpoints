using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Web.Context.WorkspaceIdProvider;

namespace kCura.IntegrationPoints.Web.Context
{
	public class ContextInstaller : IWindsorInstaller
	{
		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			container.AddWorkspaceIdProvider();
		}
	}
}