﻿using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoints.DocumentTransferProvider;
using kCura.IntegrationPoints.Web.Controllers.API.FieldMappings;
using kCura.IntegrationPoints.Web.Helpers;
using kCura.Relativity.ImportAPI;
using Relativity.IntegrationPoints.FieldsMapping;

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
					.LifestyleSingleton(),
				Component
					.For<IFieldsMappingValidator>()
					.ImplementedBy<FieldsMappingValidator>()
					.LifestyleTransient()
			);
		}
	}
}