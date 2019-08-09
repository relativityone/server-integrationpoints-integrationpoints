using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Unit.Executors
{
	[TestFixture]
	public class DocumentsTagRepositoryTests
	{
		private Mock<IDestinationWorkspaceTagRepository> _destinationWorkspace;
		private Mock<ISourceWorkspaceTagRepository> _sourceWorkspace;
		private Mock<ISyncLog> _logger;
		private Mock<IJobHistoryErrorRepository> _jobHistory;
		private DocumentsTagRepository _instance;

		[SetUp]
		public void SetUp()
		{
			_destinationWorkspace = new Mock<IDestinationWorkspaceTagRepository>();
			_sourceWorkspace = new Mock<ISourceWorkspaceTagRepository>();
			_logger = new Mock<ISyncLog>();
			_jobHistory = new Mock<IJobHistoryErrorRepository>();
			_instance = new DocumentsTagRepository(_destinationWorkspace.Object, _sourceWorkspace.Object,
				_logger.Object, _jobHistory.Object);
		}

		[Test]
		public async Task ItShouldReturnFailedIdentifiersTest()
		{
			//Arrange
			IEnumerable<string> listOfDocuments = new List<string>(new []{ "document1","document2" });
			IEnumerable<string> listOfDocuments2 = new List<string>(new[] { "document3" });
			IEnumerable<string> documentIdentifiers = new List<string>(new[] { "documentIdentifiers" });

			List<TagDocumentsResult<string>> documentsResult = new List<TagDocumentsResult<string>>()
			{
				new TagDocumentsResult<string>(listOfDocuments, "1", false, 1),
				new TagDocumentsResult<string>(listOfDocuments2, "2", true, 1)
			};

			_sourceWorkspace.Setup(x => x.TagDocumentsAsync(It.IsAny<ISynchronizationConfiguration>(),It.IsAny<IList<string>>(),It.IsAny<CancellationToken>())).ReturnsAsync(documentsResult);

			//Act
			IEnumerable<string> failedIdentifiers = await _instance.TagDocumentsInDestinationWorkspaceWithSourceInfoAsync(It.IsAny<ISynchronizationConfiguration>(),
				documentIdentifiers, CancellationToken.None).ConfigureAwait(false);

			//Assert
			const int expectedValue = 3;
			failedIdentifiers.First().Should().Be("document1");
			failedIdentifiers.Last().Should().Be("document3");
			failedIdentifiers.Count().Should().Be(expectedValue);
		}

		[Test]
		public async Task ItShouldReturnFailedArtifactIdsTest()
		{
			//Arrange
			const int count = 3;
			IEnumerable<int> listOfDocuments = Enumerable.Range(1, count);
			IEnumerable<int> documentIdentifiers = new List<int>(new[] {1});

			List<TagDocumentsResult<int>> documentsResult = new List<TagDocumentsResult<int>>()
			{
				new TagDocumentsResult<int>(listOfDocuments, "1", false, 1),
			};

			_destinationWorkspace.Setup(x => x.TagDocumentsAsync(It.IsAny<ISynchronizationConfiguration>(), It.IsAny<IList<int>>(), It.IsAny<CancellationToken>())).ReturnsAsync(documentsResult);

			//Act
			IEnumerable<int> failedArtifactIds = await _instance.TagDocumentsInSourceWorkspaceWithDestinationInfoAsync(It.IsAny<ISynchronizationConfiguration>(),
				documentIdentifiers, CancellationToken.None).ConfigureAwait(false);

			//Assert
			const int expectedValue = 3;
			failedArtifactIds.First().Should().Be(1);
			failedArtifactIds.Last().Should().Be(expectedValue);
			failedArtifactIds.Count().Should().Be(expectedValue);
		}

		[Test]
		public async Task ItShouldCancelTaggingResultTest()
		{
			CancellationTokenSource tokenSource = new CancellationTokenSource();
			tokenSource.Cancel();
			Task<IEnumerable<string>> t1 = DoFakeTagging(tokenSource.Token);
			IList<Task<IEnumerable<string>>> taggingTasks = new List<Task<IEnumerable<string>>>
			{
				t1
			};

			//Act
			ExecutionResult executionResult = await _instance.GetTaggingResultsAsync(taggingTasks, It.IsAny<int>()).ConfigureAwait(false);

			//Assert
			executionResult.Message.Should()
				.Be("Tagging synchronized documents in workspace was interrupted due to the job being canceled.");
			executionResult.Status.Should().Be(ExecutionStatus.Canceled);
		}

		[Test]
		public async Task ItShouldBeFailedStatusTest()
		{
			Task<IEnumerable<string>> t1 = DoFakeTaggingListWithDocument();
			IList<Task<IEnumerable<string>>> taggingTasks = new List<Task<IEnumerable<string>>>
			{
				t1
			};

			//Act
			ExecutionResult executionResult = await _instance.GetTaggingResultsAsync(taggingTasks, It.IsAny<int>()).ConfigureAwait(false);

			//Assert
			executionResult.Message.Should()
				.Be("Failed to tag synchronized documents in workspace. The first 2 out of 2 are: document1,document2.");
			executionResult.Status.Should().Be(ExecutionStatus.Failed);
		}
		[Test]
		public async Task ItShouldBeCompletedStatusTest()
		{
			Task<IEnumerable<string>> t1 = DoFakeTagging(CancellationToken.None);
			IList<Task<IEnumerable<string>>> taggingTasks = new List<Task<IEnumerable<string>>>
			{
				t1
			};

			//Act
			ExecutionResult executionResult = await _instance.GetTaggingResultsAsync(taggingTasks, It.IsAny<int>()).ConfigureAwait(false);

			//Assert
			executionResult.Exception.Should().Be(null);
			executionResult.Status.Should().Be(ExecutionStatus.Completed);
		}

		[Test]
		public async Task ItShouldCatchExceptionTest()
		{
			Task<IEnumerable<string>> t1 = DoFakeTaggingListWithNull();
			IList<Task<IEnumerable<string>>> taggingTasks = new List<Task<IEnumerable<string>>>
			{
				t1
			};

			//Act
			ExecutionResult executionResult = await _instance.GetTaggingResultsAsync(taggingTasks, It.IsAny<int>()).ConfigureAwait(false);

			//Assert
			executionResult.Message.Should()
				.Be("Unexpected exception occurred while tagging synchronized documents in workspace.");
			executionResult.Status.Should().Be(ExecutionStatus.Failed);
		}

		[Test]
		public async Task ItShouldCheckCreateAsyncTest()
		{
			const int jobHistoryArtifactId = 220034;
			const int sourceArtifactId = 20434;
			ExecutionResult executionResult = new ExecutionResult(ExecutionStatus.Failed,"Er",new Exception());
			Mock<ISynchronizationConfiguration> configuration = new Mock<ISynchronizationConfiguration>();
			configuration.Setup(x => x.JobHistoryArtifactId).Returns(jobHistoryArtifactId);
			configuration.Setup(x => x.SourceWorkspaceArtifactId).Returns(sourceArtifactId);

			//Act
			await _instance.GenerateDocumentTaggingJobHistoryErrorAsync(executionResult, configuration.Object).ConfigureAwait(false);

			//Assert
			_jobHistory.Verify(x =>
				x.CreateAsync(sourceArtifactId, jobHistoryArtifactId, It.Is<CreateJobHistoryErrorDto>(y => CheckCreateJobHistory(y))));
		}

		private bool CheckCreateJobHistory(CreateJobHistoryErrorDto createJobHistory)
		{
			createJobHistory.ErrorMessage.Should().Be("Er");
			createJobHistory.StackTrace.Should().Be(null);
			createJobHistory.ErrorType.Should().Be(ErrorType.Job);
			return true;
		}

		private async Task<IEnumerable<string>> DoFakeTagging(CancellationToken cancellationToken)
		{
			await Task.CompletedTask.ConfigureAwait(false);
			cancellationToken.ThrowIfCancellationRequested();
			return Enumerable.Empty<string>();
		}
		private async Task<IEnumerable<string>> DoFakeTaggingListWithDocument()
		{
			await Task.CompletedTask.ConfigureAwait(false);
			return new List<string>(new[] { "document1", "document2" });
		}

		private async Task<IEnumerable<string>> DoFakeTaggingListWithNull()
		{
			await Task.CompletedTask.ConfigureAwait(false);
			return new List<string>(null);
		}
	}
}