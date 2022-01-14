using System;
using Autofac;
using Relativity.API;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.System.Core.Helpers.APIHelper;
using Relativity.Sync.Tests.System.Core.Stubs;
using Relativity.Telemetry.APM;

namespace Relativity.Sync.Tests.System.Core.Helpers
{
	internal static class ContainerHelper
	{
		public static IContainer Create(ConfigurationStub configuration, params Action<ContainerBuilder>[] mockActions)
		{
			ContainerBuilder containerBuilder = new ContainerBuilder();

			ContainerFactory factory = new ContainerFactory();

			SyncJobParameters syncParameters = new SyncJobParameters(configuration.SyncConfigurationArtifactId, configuration.SourceWorkspaceArtifactId, Guid.NewGuid());

			IAPM apm = new NullAPM();

			IHelper helper = new TestHelper();

			IRelativityServices relativityServices = new RelativityServices(apm, new ServicesManagerStub(), AppSettings.RelativityUrl, helper);

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