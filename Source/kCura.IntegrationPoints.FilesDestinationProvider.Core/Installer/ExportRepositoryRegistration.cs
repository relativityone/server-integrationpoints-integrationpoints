using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Repositories;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Repositories.Implementations;
using Relativity.API;
using Relativity.Services.Interfaces.ViewField;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Installer
{
	public static class ExportRepositoryRegistration
	{
		public static IWindsorContainer AddExportRepositories(this IWindsorContainer container)
		{
			container.Register(Component.For<IViewFieldManager>().UsingFactoryMethod(f =>
				f.Resolve<IServicesMgr>().CreateProxy<IViewFieldManager>(ExecutionIdentity.CurrentUser)));
			container.Register(Component.For<IViewFieldRepository>().ImplementedBy<ViewFieldRepository>().LifestyleTransient());
			return container;
		}
	}
}
