using System;
using Autofac;
using Moq;
using Relativity.Sync.Tests.Common;

namespace Relativity.Sync.Tests.System.Core.Helpers
{
    internal static class SyncJobHelper
    {
        public static ISyncJob CreateWithMockedProgressAndContainerExceptProvidedType<TStepConfiguration>(ConfigurationStub configuration)
        {
            return Create(configuration, null, IntegrationTestsContainerBuilder.MockStepsExcept<TStepConfiguration>, MockProgress);
        }

        public static ISyncJob CreateWithMockedProgressAndAllSteps(ConfigurationStub configuration)
        {
            return Create(configuration, null, IntegrationTestsContainerBuilder.MockAllSteps, MockProgress);
        }

        /// <summary>
        ///     Use only in case you need to verify progress reporting
        /// </summary>
        public static ISyncJob CreateWithMockedAllSteps(ConfigurationStub configuration)
        {
            return Create(configuration, null, IntegrationTestsContainerBuilder.MockAllSteps);
        }

        private static ISyncJob Create(ConfigurationStub configuration, TestSyncToggleProvider toggleProvider, params Action<ContainerBuilder>[] mockActions)
        {
            IContainer container = ContainerHelper.Create(configuration, toggleProvider, mockActions);

            return container.Resolve<ISyncJob>();
        }

        private static void MockProgress(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterInstance(Mock.Of<IProgress<SyncJobState>>()).As<IProgress<SyncJobState>>();
        }
    }
}