using Castle.Windsor;
using Relativity.API;

namespace kCura.IntegrationPoints.Management.Installers
{
	public interface IContainerFactory
	{
		IWindsorContainer Create(IAgentHelper helper);
	}
}