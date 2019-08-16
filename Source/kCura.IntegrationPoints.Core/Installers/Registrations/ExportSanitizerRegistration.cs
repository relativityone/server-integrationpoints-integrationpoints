using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Services.Exporter.Sanitization;

namespace kCura.IntegrationPoints.Core.Installers.Registrations
{
	internal static class ExportSanitizerRegistration
	{
		public static IWindsorContainer AddExportSanitizer(this IWindsorContainer container)
		{
			return container.Register(
				Component
					.For<IChoiceTreeToStringConverter>()
					.ImplementedBy<ChoiceTreeToStringConverter>()
					.LifestyleTransient(),
				Component
					.For<IChoiceCache>()
					.ImplementedBy<ChoiceCache>()
					.LifestyleTransient(),
				Component
					.For<IExportFieldSanitizerProvider>()
					.ImplementedBy<ExportFieldSanitizerProvider>()
					.LifestyleTransient(),
				Component
					.For<IExportDataSanitizer>()
					.ImplementedBy<ExportDataSanitizer>()
					.LifestyleTransient());
		}
	}
}
