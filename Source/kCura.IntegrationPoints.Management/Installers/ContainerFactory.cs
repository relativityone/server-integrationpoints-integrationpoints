using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Installers;
using Relativity.API;

namespace kCura.IntegrationPoints.Management.Installers
{
	public class ContainerFactory : IContainerFactory
	{
		public IWindsorContainer Create(IAgentHelper helper)
		{
			var container = new WindsorContainer();
			container.Kernel.Resolver.AddSubResolver(new CollectionResolver(container.Kernel, true));

			container.Install(new ServicesInstaller());
			container.Install(new IntegrationPointsManagerInstaller(helper));

			return container;
		}
	}
}