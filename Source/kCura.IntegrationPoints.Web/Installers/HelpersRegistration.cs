﻿using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoints.Common.Metrics;
using kCura.IntegrationPoints.Common.Metrics.Sink;
using kCura.IntegrationPoints.DocumentTransferProvider;
using kCura.IntegrationPoints.Web.Attributes;
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