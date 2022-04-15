using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Relativity.API;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	public class SyncJobWithUnhandledExceptionsLoggingTests
	{
		private Mock<ISyncJob> _syncJob;
		private FakeAppDomain _appDomain;
		private Mock<IAPILog> _syncLog;

		private SyncJobWithUnhandledExceptionLogging _sut;

		[SetUp]
		public void SetUp()
		{
			_syncJob = new Mock<ISyncJob>();
			_appDomain = new FakeAppDomain();
			_syncLog = new Mock<IAPILog>();

			_sut = new SyncJobWithUnhandledExceptionLogging(_syncJob.Object, _appDomain, _syncLog.Object);
		}

		[Test]
		public async Task ItShouldCallInnerExecuteAsync()
		{
			// ACT
			await _sut.ExecuteAsync(CompositeCancellationToken.None).ConfigureAwait(false);

			// ASSERT
			_syncJob.Verify(x => x.ExecuteAsync(CompositeCancellationToken.None));
		}

		[Test]
		public async Task ItShouldCallInnerExecuteWithProgressAsync()
		{
			IProgress<SyncJobState> progress = new Progress<SyncJobState>();

			// ACT
			await _sut.ExecuteAsync(progress, CompositeCancellationToken.None).ConfigureAwait(false);

			// ASSERT
			_syncJob.Verify(x => x.ExecuteAsync(progress, CompositeCancellationToken.None));
		}
		
		[Test]
		public async Task ItShouldLogUnhandledExceptionWhenExecuted()
		{
			_syncJob.Setup(x => x.ExecuteAsync(It.IsAny<IProgress<SyncJobState>>(), It.IsAny<CompositeCancellationToken>())).Returns(() =>
			{
				_appDomain.FireUnhandledException();
				return Task.CompletedTask;
			});
			IProgress<SyncJobState> progress = new Progress<SyncJobState>();

			// ACT
			await _sut.ExecuteAsync(progress, CompositeCancellationToken.None).ConfigureAwait(false);

			// ASSERT
			_syncLog.Verify(x => x.LogFatal(It.IsAny<Exception>(), It.IsAny<string>()));
		}
	}
}
