using System;
using System.Linq;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Installer
{
	public class ValidationInstaller : IWindsorInstaller
	{
		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			container.Register(Component.For<IValidator>().ImplementedBy<FieldsMappingValidator>());
			container.Register(Component.For<IValidator>().ImplementedBy<DestinationProviderConfigurationValidator>());
			container.Register(Component.For<IValidator>().ImplementedBy<SourceProviderConfigurationValidator>());
		}
	}
}