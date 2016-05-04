using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace kCura.IntegrationPoints.Data.Installers
{
	public class RSAPIServiceInstaller : IWindsorInstaller
	{
		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			container.Register(Component.For<IGenericLibrary<IntegrationPoint>>().ImplementedBy<RsapiClientLibrary<IntegrationPoint>>().LifestyleTransient());
			container.Register(Component.For<IGenericLibrary<SourceProvider>>().ImplementedBy<RsapiClientLibrary<SourceProvider>>().LifestyleTransient());
			container.Register(Component.For<IGenericLibrary<DestinationProvider>>().ImplementedBy<RsapiClientLibrary<DestinationProvider>>().LifestyleTransient());
			container.Register(Component.For<IGenericLibrary<JobHistory>>().ImplementedBy<RsapiClientLibrary<JobHistory>>().LifestyleTransient());
			container.Register(Component.For<IGenericLibrary<JobHistoryError>>().ImplementedBy<RsapiClientLibrary<JobHistoryError>>().LifestyleTransient());
		}
	}
}
