using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.Integration.Helpers;

namespace Relativity.Sync.Tests.Integration
{
    internal abstract class FailingStepsBase<T> where T : IConfiguration
    {
        private List<Type> _executorTypes;
        private ContainerBuilder _containerBuilder;

        [SetUp]
        public void SetUp()
        {
            _executorTypes = new List<Type>();
            _containerBuilder = ContainerHelper.CreateInitializedContainerBuilder();
            IntegrationTestsContainerBuilder.RegisterStubsForPipelineBuilderTests(_containerBuilder, _executorTypes);
            IntegrationTestsContainerBuilder.MockReportingWithProgress(_containerBuilder);
        }

        [Test]
        public async Task ItShouldHandleExceptionAndStopExecutionAfterStepExecutionFails()
        {
            IExecutor<T> executor = new FailingExecutorStub<T>();

            _containerBuilder.RegisterInstance(executor).As<IExecutor<T>>();

            ISyncJob syncJob = _containerBuilder.Build().Resolve<ISyncJob>();

            // ACT
            Func<Task> action = () => syncJob.ExecuteAsync(CompositeCancellationToken.None);

            // ASSERT
            await action.Should().ThrowAsync<SyncException>().ConfigureAwait(false);

            PrintInfoAboutExecutedStepsIfNeeded();
            _executorTypes.Should().BeEquivalentTo(ExpectedExecutedSteps);
            
            // should contain steps run in parallel
            AssertExecutedSteps(_executorTypes);
        }

        protected abstract void AssertExecutedSteps(List<Type> executorTypes);

        protected abstract ICollection<Type> ExpectedExecutedSteps { get; }

        [Test]
        public async Task ItShouldHandleExceptionAndStopExecutionAfterStepConstrainsCheckFails()
        {
            IExecutionConstrains<T> executionConstrains = new FailingExecutionConstrainsStub<T>();

            _containerBuilder.RegisterInstance(executionConstrains).As<IExecutionConstrains<T>>();

            ISyncJob syncJob = _containerBuilder.Build().Resolve<ISyncJob>();

            // ACT
            Func<Task> action = () => syncJob.ExecuteAsync(CompositeCancellationToken.None);

            // ASSERT
            await action.Should().ThrowAsync<SyncException>().ConfigureAwait(false);

            PrintInfoAboutExecutedStepsIfNeeded();
            _executorTypes.Should().BeEquivalentTo(ExpectedExecutedSteps);

            _executorTypes.Should().NotContain(x => x == typeof(T));
        }

        private void PrintInfoAboutExecutedStepsIfNeeded()
        {
            List<(Type type, int count)> duplicatedSteps = _executorTypes
                .GroupBy(x => x)
                .Where(group => group.Count() > 1)
                .Select(group => (group.Key, group.Count()))
                .ToList();
            if (duplicatedSteps.Any())
            {
                IEnumerable<string> duplicatesWithCount = duplicatedSteps.Select(tc => $"{tc.type} ({tc.count})");
                Console.WriteLine($"The duplicated steps are: {string.Join(", ", duplicatesWithCount)}");
            }

            List<Type> missingSteps = ExpectedExecutedSteps.Except(_executorTypes).ToList();
            if (missingSteps.Any())
            {
                Console.WriteLine($"The missing steps are: {string.Join(", ", missingSteps)}");
            }

            List<Type> redundantSteps = _executorTypes.Except(ExpectedExecutedSteps).ToList();
            if (redundantSteps.Any())
            {
                Console.WriteLine($"The redundant steps are: {string.Join(", ", redundantSteps)}");
            }
        }
    }
}