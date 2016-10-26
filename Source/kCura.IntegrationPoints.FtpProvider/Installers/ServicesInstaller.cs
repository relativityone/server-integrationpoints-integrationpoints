using System;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.FtpProvider.Connection;
using kCura.IntegrationPoints.FtpProvider.Connection.Interfaces;
using kCura.IntegrationPoints.FtpProvider.Helpers;
using kCura.IntegrationPoints.FtpProvider.Helpers.Interfaces;
using kCura.IntegrationPoints.FtpProvider.Parser;
using kCura.IntegrationPoints.FtpProvider.Parser.Interfaces;

namespace kCura.IntegrationPoints.FtpProvider.Installers
{
	//[Obsolete("This class is obsolete as it does not conform to our usage of the Composition Root.")]
    public class ServicesInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<IConnectorFactory>().ImplementedBy<ConnectorFactory>().LifestyleSingleton().OnlyNewServices());
            container.Register(Component.For<ISettingsManager>().ImplementedBy<SettingsManager>().LifestyleTransient().OnlyNewServices());
            container.Register(Component.For<IParserFactory>().ImplementedBy<ParserFactory>().LifestyleTransient());
            container.Register(Component.For<IDataReaderFactory>().ImplementedBy<DataReaderFactory>().LifestyleTransient());
        }
    }
}