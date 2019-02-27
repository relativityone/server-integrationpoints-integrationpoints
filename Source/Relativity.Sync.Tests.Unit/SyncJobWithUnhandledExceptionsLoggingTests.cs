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
			IProgress<SyncProgress> progress = new Progress<SyncProgress>();

			// ACT
			await _sut.ExecuteAsync(progress, CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			_syncJob.Verify(x => x.ExecuteAsync(progress, CancellationToken.None));
		}

		[Test]
		public async Task ItShouldCallInnerRetryAsync()
		{
			IProgress<SyncProgress> progress = new Progress<SyncProgress>();

			// ACT
			await _sut.RetryAsync(progress, CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			_syncJob.Verify(x => x.RetryAsync(progress, CancellationToken.None));
		}

		[Test]
		public async Task ItShouldLogUnhandledExceptionWhenExecuted()
		{
			_syncJob.Setup(x => x.ExecuteAsync(It.IsAny<IProgress<SyncProgress>>(), It.IsAny<CancellationToken>())).Returns(() =>
			{
				_appDomain.FireUnhandledException();
				return Task.CompletedTask;
			});
			IProgress<SyncProgress> progress = new Progress<SyncProgress>();

			// ACT
			await _sut.ExecuteAsync(progress, CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			_syncLog.Verify(x => x.LogFatal(It.IsAny<Exception>(), It.IsAny<string>()));
		}
		
		[Test]
		public async Task ItShouldLogUnhandledExceptionWhenRetried()
		{
			_syncJob.Setup(x => x.RetryAsync(It.IsAny<IProgress<SyncProgress>>(), It.IsAny<CancellationToken>())).Returns(() =>
			{
				_appDomain.FireUnhandledException();
				return Task.CompletedTask;
			});
			IProgress<SyncProgress> progress = new Progress<SyncProgress>();

			// ACT
			await _sut.RetryAsync(progress, CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			_syncLog.Verify(x => x.LogFatal(It.IsAny<Exception>(), It.IsAny<string>()));
		}
	}
}