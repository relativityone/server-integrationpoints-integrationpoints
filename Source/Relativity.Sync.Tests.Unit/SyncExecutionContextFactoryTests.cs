using System;
using System.Threading;
using Banzai;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Sync.Logging;

namespace Relativity.Sync.Tests.Unit
{
    [TestFixture]
    public sealed class SyncExecutionContextFactoryTests
    {
        private SyncExecutionContextFactory _instance;

        private SyncJobExecutionConfiguration _configuration;

        [SetUp]
        public void SetUp()
        {
            _configuration = new SyncJobExecutionConfiguration();

            _instance = new SyncExecutionContextFactory(_configuration);
        }

        [Test]
        public void ItShouldSetExecutionContextParameters()
        {
            const int stepsInParallel = 123;

            _configuration.NumberOfStepsRunInParallel = stepsInParallel;

            // ACT
            IExecutionContext<SyncExecutionContext> context = _instance.Create(new EmptyProgress<SyncJobState>(), CompositeCancellationToken.None);

            // ASSERT
            context.GlobalOptions.ThrowOnError.Should().BeFalse();
            context.GlobalOptions.ContinueOnFailure.Should().BeFalse();
            context.GlobalOptions.DegreeOfParallelism.Should().Be(stepsInParallel);
        }

        [Test]
        public void ItShouldCreateSyncExecutionContext()
        {
            IProgress<SyncJobState> progress = new EmptyProgress<SyncJobState>();
            CompositeCancellationToken token = new CompositeCancellationToken(CancellationToken.None, CancellationToken.None, new EmptyLogger());

            // ACT
            IExecutionContext<SyncExecutionContext> context = _instance.Create(progress, token);

            // ASSERT
            context.Subject.Progress.Should().Be(progress);
            context.Subject.CompositeCancellationToken.Should().Be(token);
        }

        [Test]
        public void ItShouldBindExecutionContextWithCancellationToken()
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CompositeCancellationToken token = new CompositeCancellationToken(tokenSource.Token, CancellationToken.None, new EmptyLogger());

            IExecutionContext<SyncExecutionContext> context = _instance.Create(new EmptyProgress<SyncJobState>(), token);
            context.CancelProcessing.Should().BeFalse();

            // ACT
            tokenSource.Cancel();

            // ASSERT
            context.CancelProcessing.Should().BeTrue();
        }
    }
}
