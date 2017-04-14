using System;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Authentication;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Helpers;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Services;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Abstract;
using kCura.Relativity.Client;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using kCura.WinEDDS.Exporters;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Helpers
{
	internal class ContainerInstaller
	{
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
			windsorContainer.Register(Component.For<IServiceManagerProvider>().ImplementedBy<ServiceManagerProvider>().LifestyleTransient());
			windsorContainer.Register(Component.For<IHelper>().Instance(Substitute.For<IHelper>()).LifestyleTransient());

			windsorContainer.Register(Component.For<IJobInfoFactory>().Instance(Substitute.For<IJobInfoFactory>()).LifestyleTransient());
			windsorContainer.Register(Component.For<IJobInfo>().Instance(Substitute.For<IJobInfo>()).LifestyleTransient());

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