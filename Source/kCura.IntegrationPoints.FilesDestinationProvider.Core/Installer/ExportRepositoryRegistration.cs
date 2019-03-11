using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Repositories;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Repositories.Implementations;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Installer
{
	public static class ExportRepositoryRegistration
	{
		public static IWindsorContainer AddExportRepositories(this IWindsorContainer container)
		{
			return container.Register(
				Component
					.For<IViewFieldRepository>()
					.ImplementedBy<ViewFieldRepository>()
					.LifestyleTransient());
		}
	}
}
