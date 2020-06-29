﻿using System;
using Autofac;
using Relativity.Sync.Configuration;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.System.Core.Stubs;
using Relativity.Telemetry.APM;

namespace Relativity.Sync.Tests.System.Core.Helpers
{
	internal class ContainerHelper
	{
		public static IContainer Create(ConfigurationStub configuration, params Action<ContainerBuilder>[] mockActions)
		{
			ContainerBuilder containerBuilder = new ContainerBuilder();

			ContainerFactory factory = new ContainerFactory();
			SyncJobParameters syncParameters = new SyncJobParameters(configuration.SyncConfigurationArtifactId, configuration.SourceWorkspaceArtifactId, configuration.JobHistoryArtifactId);

			IAPM apm = new NullAPM();
			RelativityServices relativityServices = new RelativityServices(apm, new ServicesManagerStub(), AppSettings.RelativityUrl);

			factory.RegisterSyncDependencies(containerBuilder, syncParameters, relativityServices, new SyncJobExecutionConfiguration(), new ConsoleLogger());

			foreach (var mockStepsAction in mockActions)
			{
				mockStepsAction(containerBuilder);
			}

			containerBuilder.RegisterInstance(configuration).AsImplementedInterfaces();
			containerBuilder.RegisterType<PipelineSelectorConfigurationStub>().As<IPipelineSelectorConfiguration>();

			return containerBuilder.Build();
		}
	}
}