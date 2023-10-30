using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Common;
using kCura.IntegrationPoints.Common.Logger;
using kCura.IntegrationPoints.Common.Monitoring;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Installers
{
    public class InfrastructureInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<IInstanceSettingsBundle>()
                .UsingFactoryMethod(kernel => kernel.Resolve<IHelper>().GetInstanceSettingBundle())
                .LifestyleTransient());
            container.Register(Component.For<IRipAppVersionProvider>()
                .ImplementedBy<RipAppVersionProvider>()
                .IsFallback().LifestyleSingleton());

            container.Register(Component.For<IAPILog>()
                .UsingFactoryMethod(CreateLogger)
                .IsFallback().LifestyleSingleton());
            container.Register(Component.For<ISerilogLoggerInstrumentationService>()
                .ImplementedBy<SerilogLoggerInstrumentationService>()
                .IsFallback().LifestyleSingleton());
            container.Register(Component.For<Serilog.ILogger>()
                .UsingFactoryMethod(CreateSerilogLogger)
                .IsFallback().LifestyleSingleton());
            container.Register(Component.For(typeof(ILogger<>))
                .ImplementedBy(typeof(Logger<>))
                .LifestyleTransient());

            container.Register(Component.For<IExternalServiceInstrumentationProvider>()
                .ImplementedBy<ExternalServiceInstrumentationProviderWithoutJobContext>()
                .IsFallback().LifestyleSingleton());
        }

        private IAPILog CreateLogger(IKernel kernel)
        {
            return kernel.Resolve<IHelper>().GetLoggerFactory().GetLogger();
        }

        private Serilog.ILogger CreateSerilogLogger(IKernel kernel)
        {
            return kernel.Resolve<ISerilogLoggerInstrumentationService>().GetLogger();
        }
    }
}
