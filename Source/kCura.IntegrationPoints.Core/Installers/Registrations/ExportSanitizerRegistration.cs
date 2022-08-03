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
                    .For<ISanitizationDeserializer>()
                    .ImplementedBy<SanitizationDeserializer>()
                    .LifestyleTransient(),
                Component
                    .For<IExportFieldSanitizer>()
                    .ImplementedBy<SingleObjectFieldSanitizer>()
                    .LifestyleTransient(),
                Component
                    .For<IExportFieldSanitizer>()
                    .ImplementedBy<MultipleObjectFieldSanitizer>()
                    .LifestyleTransient(),
                Component
                    .For<IExportFieldSanitizer>()
                    .ImplementedBy<SingleChoiceFieldSanitizer>()
                    .LifestyleTransient(),
                Component
                    .For<IExportFieldSanitizer>()
                    .ImplementedBy<MultipleChoiceFieldSanitizer>()
                    .LifestyleTransient(),
                Component
                    .For<IExportFieldSanitizer>()
                    .ImplementedBy<UserFieldSanitizer>()
                    .LifestyleTransient(),
                Component
                    .For<IExportDataSanitizer>()
                    .ImplementedBy<ExportDataSanitizer>()
                    .LifestyleTransient());
        }
    }
}
