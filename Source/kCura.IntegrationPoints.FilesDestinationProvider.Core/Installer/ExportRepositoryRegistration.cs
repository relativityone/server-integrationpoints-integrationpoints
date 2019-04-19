using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Repositories;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Repositories.Implementations;
using Relativity.API;
using Relativity.Services.FileField;
using Relativity.Services.Interfaces.File;
using Relativity.Services.Interfaces.ViewField;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Installer
{
	public static class ExportRepositoryRegistration
	{
		public static IWindsorContainer AddExportRepositories(this IWindsorContainer container)
		{
			container.Register(
				Component.For<IViewFieldManager>().UsingFactoryMethod(f =>
					f.Resolve<IServicesMgr>().CreateProxy<IViewFieldManager>(ExecutionIdentity.CurrentUser)
				)
			);
			container.Register(
				Component.For<IViewFieldRepository>()
					.ImplementedBy<ViewFieldRepository>()
					.LifestyleTransient()
			);

			container.Register(
				Component.For<IFileManager>().UsingFactoryMethod(f =>
					f.Resolve<IServicesMgr>().CreateProxy<IFileManager>(ExecutionIdentity.CurrentUser)
				)
			);
			container.Register(
				Component.For<IFileRepository>()
					.ImplementedBy<FileRepository>()
					.LifestyleTransient()
			);

			container.Register(
				Component.For<IFileFieldManager>().UsingFactoryMethod(f => 
					f.Resolve<IServicesMgr>().CreateProxy<IFileFieldManager>(ExecutionIdentity.CurrentUser)
				)
			);
			container.Register(
				Component.For<IFileFieldRepository>()
					.ImplementedBy<FileFieldRepository>()
					.LifestyleTransient()
			);

			return container;
		}
	}
}
