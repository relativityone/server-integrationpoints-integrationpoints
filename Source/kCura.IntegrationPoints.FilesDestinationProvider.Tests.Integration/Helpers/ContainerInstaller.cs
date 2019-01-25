using System;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Helpers;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Services;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Abstract;
using kCura.Relativity.Client;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using kCura.Apps.Common.Data;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Authentication;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Helpers.FileNaming;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using kCura.WinEDDS;
using kCura.WinEDDS.Exporters;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Helpers
{
	internal class ContainerInstaller
	{
	    private static IInstanceSettingRepository _instanceSettings;

		private const int _DEF_EXPORT_BATCH_SIZE = 1000;
		private const int _DEF_EXPORT_THREAD_COUNT = 4;

		private const bool _DEF_USE_OLD_EXPORT = false;

		private const int _EXPORT_LOADFILE_IO_ERROR_WAIT_TIME = 1;
		private const int _EXPORT_LOADFILE_IO_ERROR_RETRIES_NUMBER = 1;

		public static WindsorContainer CreateContainer(ConfigSettings configSettings)
		{
			var windsorContainer = new WindsorContainer();
			windsorContainer.Kernel.Resolver.AddSubResolver(new CollectionResolver(windsorContainer.Kernel));

			RegisterExportTestCases(windsorContainer);
			RegisterInvalidExportTestCases(windsorContainer);
			RegisterLoggingClasses(windsorContainer);
			RegisterMetrics(windsorContainer);
			RegisterJobManagers(windsorContainer);
			RegisterConfig(configSettings, windsorContainer);
			RegisterRSAPIClient(windsorContainer);

			windsorContainer.Register(Component.For<ICredentialProvider>().ImplementedBy<UserPasswordCredentialProvider>());
			windsorContainer.Register(Component.For<IExportFieldsService>().ImplementedBy<ExportFieldsService>().LifestyleTransient());
            windsorContainer.Register(Component.For<ISqlServiceFactory>().ImplementedBy<HelperConfigSqlServiceFactory>().LifestyleSingleton());
            windsorContainer.Register(Component.For<IServiceManagerProvider>().ImplementedBy<ServiceManagerProvider>().LifestyleTransient());
			windsorContainer.Register(Component.For<IHelper>().Instance(new TestHelper()).LifestyleTransient());

            windsorContainer.Register(Component.For<IServicesMgr>().UsingFactoryMethod(f => f.Resolve<IHelper>().GetServicesManager()));

			windsorContainer.Register(Component.For<IFactoryConfigBuilder>().ImplementedBy<FactoryConfigBuilder>().LifestyleTransient());

			_instanceSettings = Substitute.For<IInstanceSettingRepository>();
		    _instanceSettings.GetConfigurationValue(Domain.Constants.INTEGRATION_POINT_INSTANCE_SETTING_SECTION,
		        Domain.Constants.REPLACE_WEB_API_WITH_EXPORT_CORE).Returns("False");

            windsorContainer.Register(Component.For<IInstanceSettingRepository>()
                .Instance(_instanceSettings)
                .LifestyleTransient());
		    windsorContainer.Register(Component.For<IExtendedExporterFactory>().ImplementedBy<ExtendedExporterFactory>().LifestyleTransient());
			windsorContainer.Register(Component.For<IJobInfoFactory>().Instance(Substitute.For<IJobInfoFactory>()).LifestyleTransient());
			windsorContainer.Register(Component.For<IJobInfo>().Instance(Substitute.For<IJobInfo>()).LifestyleTransient());
			windsorContainer.Register(Component.For<ISerializer>().Instance(Substitute.For<ISerializer>()).LifestyleTransient());
			windsorContainer.Register(Component.For<ITokenProvider>().Instance(Substitute.For<ITokenProvider>()).LifestyleTransient());
			windsorContainer.Register(Component.For<IFileNameProvidersDictionaryBuilder>().ImplementedBy<FileNameProvidersDictionaryBuilder>().LifestyleTransient());
			windsorContainer.Register(Component.For<IRepositoryFactory>().ImplementedBy<RepositoryFactory>());
			return windsorContainer;
		}

		private static void RegisterMetrics(WindsorContainer windsorContainer)
		{
			windsorContainer.Register(Component.For<IExternalServiceInstrumentationProvider>()
				.ImplementedBy<ExternalServiceInstrumentationProviderWithoutJobContext>()
				.LifestyleSingleton());
		}

		private static void RegisterRSAPIClient(WindsorContainer windsorContainer)
		{
			windsorContainer.Register(Component.For<IRSAPIClient>().UsingFactoryMethod(k =>
			{
				Uri relativityServicesUri = new Uri(SharedVariables.RsapiClientUri);
				return new RSAPIClient(relativityServicesUri, new UsernamePasswordCredentials(SharedVariables.RelativityUserName, SharedVariables.RelativityPassword));
			}));
		}

		private static void RegisterConfig(ConfigSettings configSettings, WindsorContainer windsorContainer)
		{
			windsorContainer.Register(Component.For<ConfigSettings>().Instance(configSettings).LifestyleTransient());
			IExportConfig exportConfig = GetMockExportLoadFileConfig();
			windsorContainer.Register(Component.For<IExportConfig>().Instance(exportConfig).LifestyleTransient());

			var configMock = Substitute.For<IConfig>();
			configMock.WebApiPath.Returns(SharedVariables.RelativityWebApiUrl);

			windsorContainer.Register(Component.For<IConfig>().Instance(configMock).LifestyleSingleton());

			var configFactoryMock = Substitute.For<IConfigFactory>();

			configFactoryMock.Create().Returns(configMock);
			windsorContainer.Register(Component.For<IConfigFactory>().Instance(configFactoryMock).LifestyleTransient());
		}

		private static IExportConfig GetMockExportLoadFileConfig()
		{
			var exportConfig = Substitute.For<IExportConfig>();

			exportConfig.ExportBatchSize.Returns(_DEF_EXPORT_BATCH_SIZE);
			exportConfig.ExportThreadCount.Returns(_DEF_EXPORT_THREAD_COUNT);

			exportConfig.ExportIOErrorWaitTime.Returns(_EXPORT_LOADFILE_IO_ERROR_WAIT_TIME);
			exportConfig.ExportIOErrorNumberOfRetries.Returns(_EXPORT_LOADFILE_IO_ERROR_RETRIES_NUMBER);
			exportConfig.ExportErrorNumberOfRetries.Returns(_DEF_EXPORT_BATCH_SIZE);
			exportConfig.ExportErrorWaitTime.Returns(_DEF_EXPORT_THREAD_COUNT);

			exportConfig.UseOldExport.Returns(_DEF_USE_OLD_EXPORT);

			return exportConfig;
		}

		private static void RegisterJobManagers(WindsorContainer windsorContainer)
		{
			var jobHistoryErrorService = Substitute.For<IJobHistoryErrorService>();
			windsorContainer.Register(Component.For<IJobHistoryErrorService>().Instance(jobHistoryErrorService).LifestyleSingleton());
			windsorContainer.Register(Component.For<JobHistoryErrorServiceProvider>().ImplementedBy<JobHistoryErrorServiceProvider>().LifestyleTransient());
		}

		private static void RegisterLoggingClasses(WindsorContainer windsorContainer)
		{
			var apiLog = Substitute.For<IAPILog>();
			windsorContainer.Register(Component.For<IAPILog>().Instance(apiLog).LifestyleSingleton());

			var exportUserNotification = Substitute.ForPartsOf<ExportUserNotification>();
			windsorContainer.Register(Component.For<IUserNotification, IUserMessageNotification>().Instance(exportUserNotification).LifestyleSingleton());

			windsorContainer.Register(Component.For<LoggingMediatorForTestsFactory>().ImplementedBy<LoggingMediatorForTestsFactory>().LifestyleSingleton());
			windsorContainer.Register(Component.For<ICompositeLoggingMediator>().UsingFactory((LoggingMediatorForTestsFactory f) => f.Create()).LifestyleSingleton());
		}

		private static void RegisterExportTestCases(WindsorContainer windsorContainer)
		{
			windsorContainer.Register(
				Classes.FromThisAssembly()
					.IncludeNonPublicTypes()
					.BasedOn<IExportTestCase>()
					.WithServiceAllInterfaces()
					.AllowMultipleMatches());
		}

		private static void RegisterInvalidExportTestCases(WindsorContainer windsorContainer)
		{
			windsorContainer.Register(
				Classes.FromThisAssembly()
					.IncludeNonPublicTypes()
					.BasedOn<IInvalidFileshareExportTestCase>()
					.WithServiceAllInterfaces()
					.AllowMultipleMatches());
		}
	}
}