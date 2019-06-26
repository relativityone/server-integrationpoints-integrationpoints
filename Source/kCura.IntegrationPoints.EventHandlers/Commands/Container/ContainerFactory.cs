﻿using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Installers;
using kCura.IntegrationPoints.Data.Installers;
using kCura.IntegrationPoints.EventHandlers.Commands.Context;

namespace kCura.IntegrationPoints.EventHandlers.Commands.Container
{
	public class ContainerFactory : IContainerFactory
	{
		public IWindsorContainer Create(IEHContext context)
		{
			var container = new WindsorContainer();

			ConfigureContainer(container);

			container.Install(new EventHandlerInstaller(context));
			container.Install(new QueryInstallers());
			container.Install(new SharedAgentInstaller());
			container.Install(new ServicesInstaller());

			return container;
		}

		private void ConfigureContainer(IWindsorContainer container)
		{
			container.Register(Component
				.For<ILazyComponentLoader>()
				.ImplementedBy<LazyOfTComponentLoader>()
			);
		}
	}
}