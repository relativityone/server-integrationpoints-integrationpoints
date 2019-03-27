using System;
using System.Threading;
using Banzai;
using FluentAssertions;
using NUnit.Framework;

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
			IExecutionContext<SyncExecutionContext> context = _instance.Create(new EmptyProgress<SyncProgress>(), CancellationToken.None);

			// ASSERT
			context.GlobalOptions.ThrowOnError.Should().BeFalse();
			context.GlobalOptions.ContinueOnFailure.Should().BeFalse();
			context.GlobalOptions.DegreeOfParallelism.Should().Be(stepsInParallel);
		}

		[Test]
		public void ItShouldCreateSyncExecutionContext()
		{
			IProgress<SyncProgress> progress = new EmptyProgress<SyncProgress>();
			CancellationToken token = new CancellationToken();

			// ACT
			IExecutionContext<SyncExecutionContext> context = _instance.Create(progress, token);

			// ASSERT
			context.Subject.Progress.Should().Be(progress);
			context.Subject.CancellationToken.Should().Be(token);
		}

		[Test]
		public void ItShouldBindExecutionContextWithCancellationToken()
		{
			CancellationTokenSource tokenSource = new CancellationTokenSource();

			IExecutionContext<SyncExecutionContext> context = _instance.Create(new EmptyProgress<SyncProgress>(), tokenSource.Token);
			context.CancelProcessing.Should().BeFalse();

			// ACT
			tokenSource.Cancel();

			// ASSERT
			context.CancelProcessing.Should().BeTrue();
		}
	}
}