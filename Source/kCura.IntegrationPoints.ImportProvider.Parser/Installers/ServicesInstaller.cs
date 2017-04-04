using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;
using kCura.IntegrationPoints.ImportProvider.Parser.Services;
using kCura.IntegrationPoints.ImportProvider.Parser.Services.Interfaces;

namespace kCura.IntegrationPoints.ImportProvider.Parser
{
	public class ServicesInstaller : IWindsorInstaller
	{
		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			container.Register(Component.For<IFieldParserFactory>().ImplementedBy<FieldParserFactory>().LifestyleSingleton().OnlyNewServices());
			container.Register(Component.For<IDataReaderFactory>().ImplementedBy<DataReaderFactory>().LifestyleSingleton().OnlyNewServices());
			container.Register(Component.For<IWinEddsLoadFileFactory>().ImplementedBy<WinEddsLoadFileFactory>().LifestyleSingleton().OnlyNewServices());
			container.Register(Component.For<IImportPreviewService>().ImplementedBy<ImportPreviewService>().LifestyleSingleton());
			container.Register(Component.For<IPreviewJobFactory>().ImplementedBy<PreviewJobFactory>().LifestyleSingleton());
			container.Register(Component.For<IImportFileLocationService>().ImplementedBy<ImportFileLocationService>().LifestyleSingleton());
			container.Register(Component.For<IWebApiConfig>().ImplementedBy<WebApiConfig>().LifestyleSingleton());
		}
	}
}