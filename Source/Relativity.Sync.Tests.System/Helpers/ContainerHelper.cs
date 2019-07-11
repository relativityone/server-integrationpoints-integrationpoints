using System;
using Autofac;
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
			RelativityServices relativityServices = new RelativityServices(apm, new ServicesManagerStub(), AppSettings.RelativityUrl);

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