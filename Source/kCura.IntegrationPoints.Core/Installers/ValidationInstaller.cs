﻿using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.RelativityProviderValidator;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Core.Validation.Implementation;
using kCura.IntegrationPoints.Domain;

namespace kCura.IntegrationPoints.Core.Installers
{
	public class ValidationInstaller : IWindsorInstaller
	{
		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			container.Kernel.Resolver.AddSubResolver(new CollectionResolver(container.Kernel));

			container.Register(Component.For<IValidator>().ImplementedBy<EmailValidator>());
			container.Register(Component.For<IValidator>().ImplementedBy<FieldsMappingValidator>());
			container.Register(Component.For<IValidator>().ImplementedBy<SchedulerValidator>());
			container.Register(Component.For<IValidator>().ImplementedBy<DestinationProviderConfigurationValidator>());
			container.Register(Component.For<IValidator>().ImplementedBy<SourceProviderConfigurationValidator>());
			
			container.Register(Component.For<IIntegrationModelValidator>().ImplementedBy<IntegrationModelValidator>());
		}
	}
}