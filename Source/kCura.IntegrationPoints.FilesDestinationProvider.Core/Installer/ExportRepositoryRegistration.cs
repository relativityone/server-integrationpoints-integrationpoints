using System;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Installers.Extensions;
using kCura.IntegrationPoints.Core.Toggles;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using kCura.WinEDDS.Service.Export;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Installer
{
	public static class ExportRepositoryRegistration
	{
		public static IWindsorContainer AddExportRepositories(this IWindsorContainer container)
		{
			container.Register(Component.For<Func<ISearchManager>>()
				.UsingFactoryMethod(k => (Func<ISearchManager>)(() => k.Resolve<IServiceManagerProvider>().Create<ISearchManager, SearchManagerFactory>()))
				.LifestyleTransient()
			);

			container
				.RegisterWithToggle<IFileRepository, EnableKeplerizedImportAPIToggle, FileRepository, WebAPIFileRepository>(c => c.LifestyleTransient());

			return container;
		}
	}
}
