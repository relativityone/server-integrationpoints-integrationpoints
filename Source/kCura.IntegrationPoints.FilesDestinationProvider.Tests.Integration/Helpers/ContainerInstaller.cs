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
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using kCura.Apps.Common.Data;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Authentication;
using kCura.IntegrationPoints.Core.Factories;
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

	    public static WindsorContainer CreateContainer(ConfigSettings configSettings)
		{
			var windsorContainer = new WindsorContainer();
			windsorContainer.Kernel.Resolver.AddSubResolver(new CollectionResolver(windsorContainer.Kernel));

			RegisterExportTestCases(windsorContainer);
			RegisterInvalidExportTestCases(windsorContainer);
			RegisterLoggingClasses(windsorContainer);
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

			return windsorContainer;
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

			var exportConfig = Substitute.For<IExportConfig>();
			exportConfig.ExportBatchSize.Returns(1000);
			exportConfig.ExportThreadCount.Returns(2);
			windsorContainer.Register(Component.For<IExportConfig>().Instance(exportConfig).LifestyleTransient());

			var configMock = Substitute.For<IConfig>();
			configMock.WebApiPath.Returns(SharedVariables.RelativityWebApiUrl);

			windsorContainer.Register(Component.For<IConfig>().Instance(configMock).LifestyleSingleton());

			var configFactoryMock = Substitute.For<IConfigFactory>();

			configFactoryMock.Create().Returns(configMock);
			windsorContainer.Register(Component.For<IConfigFactory>().Instance(configFactoryMock).LifestyleTransient());
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
					.Unless(type => Attribute.IsDefined(type, typeof(IgnoreAttribute)))
					.WithServiceAllInterfaces()
					.AllowMultipleMatches());
		}

		private static void RegisterInvalidExportTestCases(WindsorContainer windsorContainer)
		{
			windsorContainer.Register(
				Classes.FromThisAssembly()
					.IncludeNonPublicTypes()
					.BasedOn<IInvalidFileshareExportTestCase>()
					.Unless(type => Attribute.IsDefined(type, typeof(IgnoreAttribute)))
					.WithServiceAllInterfaces()
					.AllowMultipleMatches());
		}
	}
}