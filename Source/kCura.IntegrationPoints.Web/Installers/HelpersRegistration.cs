using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Factories.Implementations;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Core.Helpers.Implementations;
using kCura.IntegrationPoints.Data.Statistics;
using kCura.IntegrationPoints.Data.Statistics.Implementations;
using kCura.IntegrationPoints.DocumentTransferProvider;
using kCura.IntegrationPoints.Web.Helpers;
using kCura.IntegrationPoints.Web.Metrics;
using kCura.Relativity.ImportAPI;
using Relativity.IntegrationPoints.FieldsMapping;
using Relativity.IntegrationPoints.FieldsMapping.Metrics;

namespace kCura.IntegrationPoints.Web.Installers
{
	public static class HelpersRegistration
	{
		public static IWindsorContainer AddHelpers(this IWindsorContainer container)
		{
			return container.Register(
				Component
					.For<IRelativityUrlHelper>()
					.ImplementedBy<RelativityUrlHelper>()
					.LifestyleTransient(),
				Component
					.For<SummaryPageSelector>()
					.LifestyleSingleton(),
				Component
					.For<IDocumentAccumulatedStatistics>()
					.ImplementedBy<DocumentAccumulatedStatistics>()
					.LifestyleTransient(),
				Component
					.For<IFieldsRepository>()
					.ImplementedBy<FieldsRepository>()
					.LifestyleTransient(),
				Component
					.For<IImportAPI>()
					.UsingFactoryMethod(k => k.Resolve<IImportApiFactory>().Create())
					.LifestyleTransient(),
				Component
					.For<IFieldsClassifyRunnerFactory>()
					.ImplementedBy<FieldsClassifyRunnerFactory>()
					.LifestyleTransient(),
				Component
					.For<IAutomapRunner>()
					.ImplementedBy<AutomapRunner>()
					.LifestyleTransient(),
				Component
					.For<IFieldsMappingValidator>()
					.ImplementedBy<FieldsMappingValidator>()
					.LifestyleTransient(),

				Component
					.For<IMetricBucketNameGenerator>()
					.ImplementedBy<MetricBucketNameGenerator>()
					.LifestyleTransient(),
				Component
					.For<IMetricsSender>()
					.ImplementedBy<MetricsSender>()
					.LifestyleTransient(),
				Component
					.For<IMetricsSink>()
					.ImplementedBy<SplunkMetricsSink>()
					.LifestyleTransient(),

				Component
					.For<IControllerActionExecutionTimeMetrics>()
					.ImplementedBy<ControllerActionExecutionTimeMetrics>()
					.LifestyleTransient(),
				Component
					.For<IDateTimeHelper>()
					.ImplementedBy<DateTimeHelper>()
					.LifestyleTransient()
			);
		}
	}
}