using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	public class SyncJobWithUnhandledExceptionsLoggingTests
	{
		private Mock<ISyncJob> _syncJob;
		private FakeAppDomain _appDomain;
		private Mock<ISyncLog> _syncLog;

		private SyncJobWithUnhandledExceptionLogging _sut;

		[SetUp]
		public void SetUp()
		{
			_syncJob = new Mock<ISyncJob>();
			_appDomain = new FakeAppDomain();
			_syncLog = new Mock<ISyncLog>();

			_sut = new SyncJobWithUnhandledExceptionLogging(_syncJob.Object, _appDomain, _syncLog.Object);
		}

		[Test]
		public async Task ItShouldCallInnerExecuteAsync()
		{
			// ACT
			await _sut.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			_syncJob.Verify(x => x.ExecuteAsync(CancellationToken.None));
		}

		[Test]
		public async Task ItShouldCallInnerExecuteWithProgressAsync()
		{
			IProgress<SyncJobState> progress = new Progress<SyncJobState>();

			// ACT
			await _sut.ExecuteAsync(progress, CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			_syncJob.Verify(x => x.ExecuteAsync(progress, CancellationToken.None));
		}

		[Test]
		public async Task ItShouldCallInnerRetryAsync()
		{
			// ACT
			await _sut.RetryAsync(CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			_syncJob.Verify(x => x.RetryAsync(CancellationToken.None));
		}

		[Test]
		public async Task ItShouldCallInnerRetryWithProgressAsync()
		{
			IProgress<SyncJobState> progress = new Progress<SyncJobState>();

			// ACT
			await _sut.RetryAsync(progress, CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			_syncJob.Verify(x => x.RetryAsync(progress, CancellationToken.None));
		}

		[Test]
		public async Task ItShouldLogUnhandledExceptionWhenExecuted()
		{
			_syncJob.Setup(x => x.ExecuteAsync(It.IsAny<IProgress<SyncJobState>>(), It.IsAny<CancellationToken>())).Returns(() =>
			{
				_appDomain.FireUnhandledException();
				return Task.CompletedTask;
			});
			IProgress<SyncJobState> progress = new Progress<SyncJobState>();

			// ACT
			await _sut.ExecuteAsync(progress, CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			_syncLog.Verify(x => x.LogFatal(It.IsAny<Exception>(), It.IsAny<string>()));
		}
		
		[Test]
		public async Task ItShouldLogUnhandledExceptionWhenRetried()
		{
			_syncJob.Setup(x => x.RetryAsync(It.IsAny<IProgress<SyncJobState>>(), It.IsAny<CancellationToken>())).Returns(() =>
			{
				_appDomain.FireUnhandledException();
				return Task.CompletedTask;
			});
			IProgress<SyncJobState> progress = new Progress<SyncJobState>();

			// ACT
			await _sut.RetryAsync(progress, CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			_syncLog.Verify(x => x.LogFatal(It.IsAny<Exception>(), It.IsAny<string>()));
		}
	}
}