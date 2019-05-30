﻿using System;
using Autofac;
using Moq;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.System.Stubs;
using Relativity.Telemetry.APM;

namespace Relativity.Sync.Tests.System.Helpers
{
	internal static class SyncJobHelper
	{
		public static ISyncJob CreateWithMockedProgressAndContainerExceptProvidedType<TStepConfiguration>(ConfigurationStub configuration)
		{
			return Create(configuration, IntegrationTestsContainerBuilder.MockStepsExcept<TStepConfiguration>, true);
		}

		public static ISyncJob CreateWithMockedProgressAndAllSteps(ConfigurationStub configuration)
		{
			return Create(configuration, IntegrationTestsContainerBuilder.MockAllSteps, true);
		}

		/// <summary>
		///     Use only in case you need to verify progress reporting
		/// </summary>
		public static ISyncJob CreateWithMockedAllSteps(ConfigurationStub configuration)
		{
			return Create(configuration, IntegrationTestsContainerBuilder.MockAllSteps, false);
		}

		private static ISyncJob Create(ConfigurationStub configuration, Action<ContainerBuilder> mockSteps, bool mockProgress)
		{
			ContainerBuilder containerBuilder = new ContainerBuilder();

			ContainerFactory factory = new ContainerFactory();
			SyncJobParameters syncParameters = new SyncJobParameters(configuration.JobHistoryArtifactId, configuration.SourceWorkspaceArtifactId);

			IAPM apm = Mock.Of<IAPM>();
			RelativityServices relativityServices = new RelativityServices(apm, new ServicesManagerStub(), AppSettings.RelativityUrl);

			factory.RegisterSyncDependencies(containerBuilder, syncParameters, relativityServices, new SyncJobExecutionConfiguration(), new ConsoleLogger());

			mockSteps.Invoke(containerBuilder);

			containerBuilder.RegisterInstance(configuration).AsImplementedInterfaces();
			if (mockProgress)
			{
				containerBuilder.RegisterInstance(Mock.Of<IProgress<SyncJobState>>()).As<IProgress<SyncJobState>>();
			}

			return containerBuilder.Build().Resolve<ISyncJob>();
		}
	}
}