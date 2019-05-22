using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Executors;
using Relativity.Sync.Logging;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	internal class ImportJobTests : IDisposable
	{
		private Mock<ISyncImportBulkArtifactJob> _importBulkArtifactJobMock;
		private Mock<IJobHistoryErrorRepository> _jobHistoryErrorRepository;
		private Mock<ISemaphoreSlim> _semaphore;

		private ImportJob _importJob;

		private const string _IDENTIFIER_COLUMN = "Identifier";
		private const string _MESSAGE_COLUMN = "Message";
		private const int _SOURCE_WORKSPACE_ARTIFACT_ID = 1;

		[SetUp]
		public void SetUp()
		{
			const int jobHistoryArtifactId = 2;
			_importBulkArtifactJobMock = new Mock<ISyncImportBulkArtifactJob>();
			_jobHistoryErrorRepository = new Mock<IJobHistoryErrorRepository>();
			_semaphore = new Mock<ISemaphoreSlim>();

			_importJob = new ImportJob(_importBulkArtifactJobMock.Object, _semaphore.Object, _jobHistoryErrorRepository.Object,
				_SOURCE_WORKSPACE_ARTIFACT_ID, jobHistoryArtifactId, new EmptyLogger());
		}

		[Test]
		public async Task ItShouldHandleItemLevelError()
		{
			const string identifier = "id";
			const string message = "msg";

			_importBulkArtifactJobMock.Setup(x => x.Execute()).Callback(() =>
			{
				_importBulkArtifactJobMock.Raise(x => x.OnError += null, new Dictionary<string, string>()
				{
					{_IDENTIFIER_COLUMN, identifier},
					{_MESSAGE_COLUMN, message}
				});
				_importBulkArtifactJobMock.Raise(x => x.OnComplete += null, CreateJobReport());
			});

			// act
			await _importJob.RunAsync(CancellationToken.None).ConfigureAwait(false);

			// assert
			string expectedErrorMessage = $"IAPI {message}";
			_jobHistoryErrorRepository.Verify(x => x.CreateAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, It.Is<CreateJobHistoryErrorDto>(dto =>
				dto.SourceUniqueId == identifier && dto.ErrorMessage == expectedErrorMessage && dto.ErrorType == ErrorType.Item)));
		}

		[Test]
		public void ItShouldHandleJobLevelError()
		{
			_importBulkArtifactJobMock.Setup(x => x.Execute()).Callback(() =>
			{
				JobReport jobReport = CreateJobReport();
				PropertyInfo fatalException = jobReport.GetType().GetProperty(nameof(jobReport.FatalException));
				InvalidOperationException ex = new InvalidOperationException();
				fatalException?.SetValue(jobReport, ex, BindingFlags.NonPublic | BindingFlags.Instance, null, null, CultureInfo.InvariantCulture);

				_importBulkArtifactJobMock.Raise(x => x.OnFatalException += null, jobReport);
			});

			// act
			Func<Task> action = async () => await _importJob.RunAsync(CancellationToken.None).ConfigureAwait(false);

			// assert
			action.Should().Throw<SyncException>().Which.InnerException.Should().BeOfType<InvalidOperationException>();
			_jobHistoryErrorRepository.Verify(x => x.CreateAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, It.Is<CreateJobHistoryErrorDto>(dto =>
				dto.ErrorType == ErrorType.Job)));
		}

		[Test]
		public void ItShouldHandleOnComplete()
		{
			_importBulkArtifactJobMock.Setup(x => x.Execute()).Raises(x => x.OnComplete += null, CreateJobReport());

			// act
			Func<Task> action = async () => await _importJob.RunAsync(CancellationToken.None).ConfigureAwait(false);

			// assert
			action.Should().NotThrow();
		}

		[Test]
		public void ItShouldNotThrowExceptionWhenCanceled()
		{
			CancellationTokenSource token = new CancellationTokenSource();
			token.Cancel();

			// act
			Func<Task> action = async () => await _importJob.RunAsync(token.Token).ConfigureAwait(false);

			// assert
			action.Should().NotThrow<OperationCanceledException>();
		}

		[Test]
		public void ItShouldDisposeSemaphoreSlim()
		{
			// act
			_importJob.Dispose();

			// assert
			_semaphore.Verify(x => x.Dispose());
		}

		public void Dispose()
		{
			_importJob?.Dispose();
		}

		private static JobReport CreateJobReport()
		{
			JobReport jobReport = (JobReport)Activator.CreateInstance(typeof(JobReport), true);
			return jobReport;
		}

	}
}