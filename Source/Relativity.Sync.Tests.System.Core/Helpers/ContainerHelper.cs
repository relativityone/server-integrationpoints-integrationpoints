using System;
using Autofac;
using Relativity.API;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.System.Core.Helpers.APIHelper;
using Relativity.Telemetry.APM;
using Relativity.Toggles;

namespace Relativity.Sync.Tests.System.Core.Helpers
{
    internal static class ContainerHelper
    {
        public static IContainer Create(ConfigurationStub configuration, TestSyncToggleProvider toggleProvider = null, params Action<ContainerBuilder>[] mockActions)
        {
            ContainerBuilder containerBuilder = new ContainerBuilder();

            ContainerFactory factory = new ContainerFactory();

            SyncJobParameters syncParameters = new SyncJobParameters(configuration.SyncConfigurationArtifactId, configuration.SourceWorkspaceArtifactId, configuration.ExecutingUserId, Guid.NewGuid());

            IAPM apm = new NullAPM();

            IHelper helper = new TestHelper();

            IRelativityServices relativityServices = new RelativityServices(apm, AppSettings.RelativityUrl, helper);

            factory.RegisterSyncDependencies(containerBuilder, syncParameters, relativityServices, new SyncJobExecutionConfiguration(), new ConsoleLogger());

            foreach (var mockStepsAction in mockActions)
            {
                mockStepsAction(containerBuilder);
            }

            containerBuilder.RegisterInstance(configuration).AsImplementedInterfaces();
            containerBuilder.RegisterInstance(toggleProvider ?? new TestSyncToggleProvider()).As<IToggleProvider>();

            return containerBuilder.Build();
        }
    }
}