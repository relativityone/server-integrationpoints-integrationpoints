using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Installers;
using Relativity.API;
using Relativity.Telemetry.APM;

namespace kCura.IntegrationPoints.Management.Installers
{
	public class ContainerFactory : IContainerFactory
	{
		public IWindsorContainer Create(IAgentHelper helper)
		{
			var container = new WindsorContainer();
			container.Kernel.Resolver.AddSubResolver(new CollectionResolver(container.Kernel, true));
			container.Register(Component.For<IAPM>().UsingFactoryMethod(k => Client.APMClient, true).LifestyleTransient());

			container.Install(new SharedAgentInstaller());
			container.Install(new IntegrationPointsManagerInstaller(helper));

			return container;
		}
	}
}