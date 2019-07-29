using System;
using System.Net;
using Autofac;
using kCura.WinEDDS.Service;
using kCura.WinEDDS.Service.Export;
using Moq;
using Relativity.Sync.Configuration;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.System.Stubs;
using Relativity.Telemetry.APM;

namespace Relativity.Sync.Tests.System.Helpers
{
	internal class ContainerHelper
	{
		public static IContainer Create(ConfigurationStub configuration, params Action<ContainerBuilder>[] mockActions)
		{
			ContainerBuilder containerBuilder = new ContainerBuilder();

			ContainerFactory factory = new ContainerFactory();
			SyncJobParameters syncParameters = new SyncJobParameters(configuration.SyncConfigurationArtifactId, configuration.SourceWorkspaceArtifactId, new ImportSettingsDto());

			IAPM apm = Mock.Of<IAPM>();
			Func<ISearchManager> searchManagerFactory = () => new SearchManager(new NetworkCredential(AppSettings.RelativityUserName, AppSettings.RelativityUserPassword),
				new CookieContainer(int.MaxValue));
			RelativityServices relativityServices = new RelativityServices(apm, new ServicesManagerStub(), searchManagerFactory, AppSettings.RelativityUrl);

			factory.RegisterSyncDependencies(containerBuilder, syncParameters, relativityServices, new SyncJobExecutionConfiguration(), new ConsoleLogger());

			foreach (var mockStepsAction in mockActions)
			{
				mockStepsAction(containerBuilder);
			}

			containerBuilder.RegisterInstance(configuration).AsImplementedInterfaces();

			return containerBuilder.Build();
		}
	}
}