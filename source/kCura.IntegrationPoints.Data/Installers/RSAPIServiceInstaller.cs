using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Castle.MicroKernel.Registration;
namespace kCura.IntegrationPoints.Data.Installers
{
	public class RSAPIServiceInstaller : IWindsorInstaller
	{
		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			container.Register(Component.For<IGenericLibrary<Document>>().ImplementedBy<RsapiClientLibrary<Document>>().LifestyleTransient());
			container.Register(Component.For<IGenericLibrary<IntegrationPoint>>().ImplementedBy<RsapiClientLibrary<IntegrationPoint>>().LifestyleTransient());
			container.Register(Component.For<IGenericLibrary<SourceProvider>>().ImplementedBy<RsapiClientLibrary<SourceProvider>>().LifestyleTransient());
			container.Register(Component.For<IGenericLibrary<DestinationProvider>>().ImplementedBy<RsapiClientLibrary<DestinationProvider>>().LifestyleTransient());
			container.Register(Component.For<IGenericLibrary<JobHistory>>().ImplementedBy<RsapiClientLibrary<JobHistory>>().LifestyleTransient());
			container.Register(Component.For<IGenericLibrary<JobHistoryError>>().ImplementedBy<RsapiClientLibrary<JobHistoryError>>().LifestyleTransient());
			container.Register(Component.For<IGenericLibrary<DestinationWorkspace>>().ImplementedBy<RsapiClientLibrary<DestinationWorkspace>>().LifestyleTransient());
		}
	}
}
