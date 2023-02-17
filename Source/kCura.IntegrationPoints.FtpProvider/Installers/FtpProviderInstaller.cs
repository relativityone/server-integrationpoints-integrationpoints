using System;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Common.Agent;
using kCura.IntegrationPoints.FtpProvider.Connection;
using kCura.IntegrationPoints.FtpProvider.Connection.Interfaces;
using kCura.IntegrationPoints.FtpProvider.Helpers;
using kCura.IntegrationPoints.FtpProvider.Helpers.Interfaces;
using kCura.IntegrationPoints.FtpProvider.Parser;
using kCura.IntegrationPoints.FtpProvider.Parser.Interfaces;
using Relativity.IntegrationPoints.Contracts.Provider;

namespace kCura.IntegrationPoints.FtpProvider.Installers
{
    public class ServicesInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<IConnectorFactory>().ImplementedBy<ConnectorFactory>().LifestyleSingleton().OnlyNewServices());
            container.Register(Component.For<ISettingsManager>().ImplementedBy<SettingsManager>().LifestyleTransient().OnlyNewServices());
            container.Register(Component.For<IParserFactory>().ImplementedBy<ParserFactory>().LifestyleTransient());
            container.Register(Component.For<IFieldParserFactory>().ImplementedBy<TextFieldParserFactory>().LifestyleTransient());
            container.Register(Component.For<IDataReaderFactory>().ImplementedBy<DataReaderFactory>().LifestyleTransient());
            container.Register(Component.For<IDataSourceProvider>().ImplementedBy<FtpProvider>().LifestyleTransient().Named(new Guid(Constants.Guids.FtpProviderEventHandler).ToString()));
            container.Register(Component.For<IHostValidator>().ImplementedBy<HostValidator>().LifestyleTransient());
            container.Register(Component.For<IRemovableAgent>().ImplementedBy<FakeNonRemovableAgent>().LifestyleTransient());
        }
    }
}
