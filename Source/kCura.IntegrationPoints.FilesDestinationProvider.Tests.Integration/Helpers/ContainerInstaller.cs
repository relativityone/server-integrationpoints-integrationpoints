using Castle.Facilities.TypedFactory;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Abstract;
using kCura.Relativity.Client;
using Castle.MicroKernel.Registration;
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
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Installer;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Process.Internals;
using kCura.WinEDDS;
using kCura.WinEDDS.Exporters;
using NSubstitute;
using Relativity.API;
using Relativity.Services.Folder;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Helpers
{
	internal static class ContainerInstaller
	{
		private const int _EXPORT_BATCH_SIZE = 1000;
		private const int _EXPORT_THREAD_COUNT = 4;

		private const int _EXPORT_IO_ERROR_WAIT_TIME = 1;
		private const int _EXPORT_IO_ERROR_NUMBER_OF_RETRIES = 1;
		private const int _EXPORT_ERROR_NUMBER_OF_RETRIES = 2;
		private const int _EXPORT_ERROR_WAIT_TIME = 10;

		private const bool _FORCE_PARALLELISM_IN_NEW_EXPORT = true;
		private const int _MAX_NUMBER_OF_FILE_EXPORT_TASKS = 2;
		private const int _MAXIMUM_FILES_FOR_TAPI_BRIDGE = 1000;
		private const int _TAPI_BRIDGE_EXPORT_TRANSFER_WAITING_TIME_IN_SECONDS = 600;
		private const bool _TAPI_FORCE_HTTP_CLIENT = false;

		private const bool _USE_OLD_EXPORT = false;

		public static WindsorContainer CreateContainer(ExportTestConfiguration testConfiguration, ExportTestContext testContext)
		{
			var windsorContainer = new WindsorContainer();
			windsorContainer.Kernel.Resolver.AddSubResolver(new CollectionResolver(windsorContainer.Kernel));
			windsorContainer.Kernel.AddFacility<TypedFactoryFacility>();

			RegisterExportTestCases(windsorContainer);
			RegisterInvalidExportTestCases(windsorContainer);
			RegisterTestHelpers(windsorContainer);
			RegisterLoggingClasses(windsorContainer);
			RegisterMetrics(windsorContainer);
			RegisterJobManagers(windsorContainer);
			RegisterConfig(testConfiguration, testContext, windsorContainer);
			RegisterRSAPIClient(windsorContainer);

			windsorContainer.Register(Component.For<ICredentialProvider>().ImplementedBy<UserPasswordCredentialProvider>());
			windsorContainer.Register(Component.For<ISqlServiceFactory>().ImplementedBy<HelperConfigSqlServiceFactory>().LifestyleSingleton());
			windsorContainer.Register(Component.For<IServiceManagerProvider>().ImplementedBy<ServiceManagerProvider>().LifestyleTransient());
			windsorContainer.Register(Component.For<IHelper>().Instance(new TestHelper()).LifestyleTransient());

			windsorContainer.Register(Component.For<IServicesMgr>().UsingFactoryMethod(f => f.Resolve<IHelper>().GetServicesManager()));
			windsorContainer.Register(Component.For<IFolderManager>().UsingFactoryMethod(f =>
				f.Resolve<IServicesMgr>().CreateProxy<IFolderManager>(ExecutionIdentity.CurrentUser)));
			windsorContainer.Register(Component.For<FolderWithDocumentsIdRetriever>().ImplementedBy<FolderWithDocumentsIdRetriever>());

			IInstanceSettingRepository instanceSettings = Substitute.For<IInstanceSettingRepository>();
			instanceSettings.GetConfigurationValue(Domain.Constants.INTEGRATION_POINT_INSTANCE_SETTING_SECTION,
				Domain.Constants.REPLACE_WEB_API_WITH_EXPORT_CORE).Returns("False");

			windsorContainer.Register(Component.For<IInstanceSettingRepository>()
				.Instance(instanceSettings)
				.LifestyleTransient());

			windsorContainer.Register(Component.For<ISerializer>().Instance(Substitute.For<ISerializer>()).LifestyleTransient());
			windsorContainer.Register(Component.For<IFileNameProvidersDictionaryBuilder>().ImplementedBy<FileNameProvidersDictionaryBuilder>().LifestyleTransient());
			windsorContainer.Register(Component.For<IRepositoryFactory>().ImplementedBy<RepositoryFactory>());

			windsorContainer.Install(new ExportInstaller());

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
				return new RSAPIClient(SharedVariables.RsapiUri, new UsernamePasswordCredentials(SharedVariables.RelativityUserName, SharedVariables.RelativityPassword));
			}));
		}

		private static void RegisterConfig(
			ExportTestConfiguration testConfiguration,
			ExportTestContext configSettings,
			WindsorContainer windsorContainer)
		{
			windsorContainer.Register(Component.For<ExportTestConfiguration>().Instance(testConfiguration));
			windsorContainer.Register(Component.For<ExportTestContext>().Instance(configSettings));
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
			IExportConfig exportConfig = Substitute.For<IExportConfig>();

			exportConfig.ExportBatchSize.Returns(_EXPORT_BATCH_SIZE);
			exportConfig.ExportThreadCount.Returns(_EXPORT_THREAD_COUNT);

			exportConfig.ExportIOErrorWaitTime.Returns(_EXPORT_IO_ERROR_WAIT_TIME);
			exportConfig.ExportIOErrorNumberOfRetries.Returns(_EXPORT_IO_ERROR_NUMBER_OF_RETRIES);
			exportConfig.ExportErrorNumberOfRetries.Returns(_EXPORT_ERROR_NUMBER_OF_RETRIES);
			exportConfig.ExportErrorWaitTime.Returns(_EXPORT_ERROR_WAIT_TIME);

			exportConfig.ForceParallelismInNewExport.Returns(_FORCE_PARALLELISM_IN_NEW_EXPORT);
			exportConfig.MaxNumberOfFileExportTasks.Returns(_MAX_NUMBER_OF_FILE_EXPORT_TASKS);
			exportConfig.MaximumFilesForTapiBridge.Returns(_MAXIMUM_FILES_FOR_TAPI_BRIDGE);
			exportConfig.TapiBridgeExportTransferWaitingTimeInSeconds.Returns(_TAPI_BRIDGE_EXPORT_TRANSFER_WAITING_TIME_IN_SECONDS);
			exportConfig.TapiForceHttpClient.Returns(_TAPI_FORCE_HTTP_CLIENT);

			exportConfig.UseOldExport.Returns(_USE_OLD_EXPORT);

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

		private static void RegisterTestHelpers(WindsorContainer windsorContainer)
		{
			windsorContainer.Register(
				Component.For<ExportTestContextProvider>(),
				Component.For<ImportHelper>().LifestyleTransient(),
				Component.For<WorkspaceService>().LifestyleTransient()
				);
		}
	}
}