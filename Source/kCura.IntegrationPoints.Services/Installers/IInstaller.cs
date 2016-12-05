using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace kCura.IntegrationPoints.Services.Installers
{
	public interface IInstaller
	{
		void Install(IWindsorContainer container, IConfigurationStore store, int workspaceId);
	}
}