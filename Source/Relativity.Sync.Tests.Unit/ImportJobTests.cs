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
using IImportBulkArtifactJob = Relativity.Sync.Executors.IImportBulkArtifactJob;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	internal class ImportJobTests : IDisposable
	{
		private Mock<IImportBulkArtifactJob> _importBulkArtifactJobMock;
		private Mock<IJobHistoryErrorRepository> _jobHistoryErrorRepository;
		private Mock<ISemaphoreSlim> _semaphore;

		private ImportJob _importJob;

		private const string _IDENTIFIER_COLUMN = "Identifier";
		private const string _MESSAGE_COLUMN = "Message";
		private const int _SOURCE_WORKSPACE_ARTIFACT_ID = 1;
		private const int _JOB_HISTORY_ARTIFACT_ID = 2;

		[SetUp]
		public void SetUp()
		{
			_importBulkArtifactJobMock = new Mock<IImportBulkArtifactJob>();
			_jobHistoryErrorRepository = new Mock<IJobHistoryErrorRepository>();
			_semaphore = new Mock<ISemaphoreSlim>();

			_importJob = new ImportJob(_importBulkArtifactJobMock.Object, _semaphore.Object, _jobHistoryErrorRepository.Object,
				_SOURCE_WORKSPACE_ARTIFACT_ID, _JOB_HISTORY_ARTIFACT_ID, new EmptyLogger());
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
			_jobHistoryErrorRepository.Verify(x => x.CreateAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, It.Is<CreateJobHistoryErrorDto>(dto => 
				dto.SourceUniqueId == identifier && dto.ErrorMessage == message && dto.ErrorType == ErrorType.Item)));
		}

		[Test]
		public void ItShouldHandleJobLevelError()
		{
			const string errorMessage = "Fatal exception occurred in ImportAPI during import job";

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
				dto.ErrorMessage == errorMessage && dto.ErrorType == ErrorType.Job)));
		}

		private static JobReport CreateJobReport()
		{
			JobReport jobReport = (JobReport) Activator.CreateInstance(typeof(JobReport), true);
			return jobReport;
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
		public void ItShouldThrowExceptionWhenCanceled()
		{
			Mock<IImportBulkArtifactJob> importBulkArtifactJob = new Mock<IImportBulkArtifactJob>();
			const int executionDurationMillisecods = 10;
			importBulkArtifactJob.Setup(x => x.Execute()).Callback(() => Task.Delay(executionDurationMillisecods).ConfigureAwait(false).GetAwaiter().GetResult());
			CancellationTokenSource token = new CancellationTokenSource();
			const int cancellationDelay = 5;
			token.CancelAfter(cancellationDelay);

			ImportJob importJob = new ImportJob(importBulkArtifactJob.Object, _semaphore.Object, _jobHistoryErrorRepository.Object,
				_SOURCE_WORKSPACE_ARTIFACT_ID, _JOB_HISTORY_ARTIFACT_ID, new EmptyLogger());

			// act
			Func<Task> action = async () => await importJob.RunAsync(token.Token).ConfigureAwait(false);

			// assert
			action.Should().Throw<OperationCanceledException>();
		}

		public void Dispose()
		{
			_importJob?.Dispose();
		}
	}
}