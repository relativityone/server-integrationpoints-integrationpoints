using System;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core;
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

			container.Register(
				Component.For<IFileRepository>()
					.ImplementedBy<FileRepository>()
					.LifestyleTransient()
			);
			return container;
		}
	}
}
