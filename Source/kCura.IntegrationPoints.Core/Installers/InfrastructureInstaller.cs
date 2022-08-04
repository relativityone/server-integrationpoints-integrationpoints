using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Installers
{
    public class InfrastructureInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<IAPILog>()
                .UsingFactoryMethod(CreateLogger)
                .IsFallback().LifestyleSingleton());
            container.Register(Component.For<IExternalServiceInstrumentationProvider>()
                .ImplementedBy<ExternalServiceInstrumentationProviderWithoutJobContext>()
                .IsFallback().LifestyleSingleton());
        }

        private IAPILog CreateLogger(IKernel kernel)
        {
            return kernel.Resolve<IHelper>().GetLoggerFactory().GetLogger();
        }
    }
}
