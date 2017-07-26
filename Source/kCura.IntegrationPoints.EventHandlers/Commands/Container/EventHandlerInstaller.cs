using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.EventHandlers.Commands.Context;
using kCura.IntegrationPoints.EventHandlers.Commands.Helpers;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Factories;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Validators;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.Commands.Container
{
	internal class EventHandlerInstaller : IWindsorInstaller
	{
		private readonly IEHContext _context;

		public EventHandlerInstaller(IEHContext context)
		{
			_context = context;
		}

		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			container.Register(Component.For<IEHContext>().Instance(_context).LifestyleSingleton());
			container.Register(Component.For<IHelper>().Instance(_context.Helper).LifestyleSingleton());
			container.Register(Component.For<IServicesMgr>().Instance(_context.Helper.GetServicesManager()).LifestyleSingleton());

			container.Register(Component.For<DeleteIntegrationPointCommand>().ImplementedBy<DeleteIntegrationPointCommand>().LifestyleTransient());
			container.Register(Component.For<PreCascadeDeleteIntegrationPointCommand>().ImplementedBy<PreCascadeDeleteIntegrationPointCommand>().LifestyleTransient());
			container.Register(Component.For<IIntegrationPointSecretDelete>().UsingFactoryMethod(k => IntegrationPointSecretDeleteFactory.Create(k.Resolve<IEHContext>().Helper))
				.LifestyleTransient());
			container.Register(Component.For<ICorrespondingJobDelete>().ImplementedBy<CorrespondingJobDelete>().LifestyleTransient());
			container.Register(Component.For<IPreCascadeDeleteEventHandlerValidator>().ImplementedBy<PreCascadeDeleteEventHandlerValidator>().LifestyleTransient());
			container.Register(Component.For<IArtifactsToDelete>().ImplementedBy<ArtifactsToDelete>().LifestyleTransient());
		}
	}
}