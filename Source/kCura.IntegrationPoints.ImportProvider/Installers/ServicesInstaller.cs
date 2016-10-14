using System;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

using kCura.IntegrationPoints.ImportProvider.Parser;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;

namespace kCura.IntegrationPoints.ImportProvider.Installers
{
	[Obsolete("This class is obsolete as it does not conform to our usage of the Composition Root.")]
    public class ServicesInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<IFieldParserFactory>().ImplementedBy<FieldParserFactory>().LifestyleSingleton().OnlyNewServices());
            container.Register(Component.For<IDataReaderFactory>().ImplementedBy<DataReaderFactory>().LifestyleSingleton().OnlyNewServices());
            container.Register(Component.For<IWinEddsLoadFileFactory>().ImplementedBy<WinEddsLoadFileFactory>().LifestyleSingleton().OnlyNewServices());
        }
    }
}
