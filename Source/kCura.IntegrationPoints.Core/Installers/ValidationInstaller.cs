using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Core.Validation.Implementation;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Synchronizers.RDO;

namespace kCura.IntegrationPoints.Core.Installers
{
	public class ValidationInstaller : IWindsorInstaller
	{
		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			container.Kernel.Resolver.AddSubResolver(new CollectionResolver(container.Kernel));

			container.Register(Component.For<IValidator>().ImplementedBy<EmailValidator>());
			container.Register(Component.For<IValidator>().ImplementedBy<NameValidator>());
			container.Register(Component.For<IValidator>().ImplementedBy<FieldsMappingValidator>());	//TODO Inject RdoFieldSynchronizerBase
			container.Register(Component.For<IValidator>().ImplementedBy<SchedulerValidator>());
			container.Register(Component.For<IImportApiFactory>().ImplementedBy<ImportApiFactory>().LifestyleTransient());  //TODO Figure out if it was registered in ServicesInstaller I need to register it again and register parameters (IHelper , ISystemEventLoggingService)
			container.Register(Component.For<IValidator>().ImplementedBy<ProviderConfigurationValidator>());
			
			container.Register(Component.For<IIntegrationModelValidator>().ImplementedBy<IntegrationModelValidator>());
		}
	}
}