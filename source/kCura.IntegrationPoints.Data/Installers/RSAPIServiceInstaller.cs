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
			container.Register(Component.For<IRSAPIService>().ImplementedBy<RSAPIService>().LifestyleTransient());
			container.Register(Component.For<IGenericLibrary<IntegrationPoint>>().ImplementedBy<RsapiClientLibrary<IntegrationPoint>>().LifestyleTransient());
			container.Register(Component.For<IGenericLibrary<SourceProvider>>().ImplementedBy<RsapiClientLibrary<SourceProvider>>().LifestyleTransient());
		}
	}
}
