using System.Collections.Generic;
using Castle.MicroKernel;
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
					.For<ISanitizationHelper>()
					.ImplementedBy<SanitizationHelper>()
					.LifestyleTransient(),
				Component
					.For<IEnumerable<IExportFieldSanitizer>>()
					.UsingFactoryMethod(CreateExportFieldSanitizers)
					.LifestyleTransient(),
				Component
					.For<IExportDataSanitizer>()
					.ImplementedBy<ExportDataSanitizer>()
					.LifestyleTransient());
		}

		private static IEnumerable<IExportFieldSanitizer> CreateExportFieldSanitizers(IKernel kernel)
		{
			ISanitizationHelper sanitizationHelper = kernel.Resolve<ISanitizationHelper>();
			IChoiceCache choiceCache = kernel.Resolve<IChoiceCache>();
			IChoiceTreeToStringConverter choiceTreeConverter = kernel.Resolve<IChoiceTreeToStringConverter>();

			IList<IExportFieldSanitizer> sanitizers = new List<IExportFieldSanitizer>
			{
				new SingleObjectFieldSanitizer(sanitizationHelper),
				new MultipleObjectFieldSanitizer(sanitizationHelper),
				new SingleChoiceFieldSanitizer(sanitizationHelper),
				new MultipleChoiceFieldSanitizer(choiceCache, choiceTreeConverter, sanitizationHelper)
			};

			return sanitizers;
		}
	}
}
