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
    public class DocumentTagRepositoryTests
    {
        private Mock<IDestinationWorkspaceTagRepository> _destinationWorkspace;
        private Mock<ISourceWorkspaceTagRepository> _sourceWorkspace;
        private Mock<IJobHistoryErrorRepository> _jobHistory;
        private Mock<ISynchronizationConfiguration> _config;
        private DocumentTagRepository _instance;

        private const int _JOB_HISTORY_ARTIFACT_ID = 23042;
        private const int _SOURCE_ARTIFACT_ID = 23042;

        [SetUp]
        public void SetUp()
        {
            _config = new Mock<ISynchronizationConfiguration>();
            _config.SetupGet(x => x.JobHistoryArtifactId).Returns(_JOB_HISTORY_ARTIFACT_ID);
            _config.Setup(x => x.SourceWorkspaceArtifactId).Returns(_SOURCE_ARTIFACT_ID);
            _destinationWorkspace = new Mock<IDestinationWorkspaceTagRepository>();
            _sourceWorkspace = new Mock<ISourceWorkspaceTagRepository>();
            _jobHistory = new Mock<IJobHistoryErrorRepository>();
            _instance = new DocumentTagRepository(_destinationWorkspace.Object, _sourceWorkspace.Object, _jobHistory.Object);
        }

        [Test]
        public async Task ItShouldReturnFailedIdentifiersTest()
        {
            //Arrange
            const string message = "Failed to tag synchronized documents in workspace. The first 3 out of 3 are: document1,document2,document3.";

            IEnumerable<string> listOfDocuments = new List<string>(new[] { "document1", "document2" });
            IEnumerable<string> listOfDocuments2 = new List<string>(new[] { "document3" });
            IEnumerable<string> documentIdentifiers = new List<string>(new[] { "documentIdentifiers" });

            List<TagDocumentsResult<string>> documentsResult = new List<TagDocumentsResult<string>>()
            {
                new TagDocumentsResult<string>(listOfDocuments, "1", false, 1),
                new TagDocumentsResult<string>(listOfDocuments2, "2", true, 1)
            };

            _sourceWorkspace.Setup(x => x.TagDocumentsAsync(It.IsAny<ISynchronizationConfiguration>(), It.IsAny<IList<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(documentsResult);

            //Act
            ExecutionResult executionResult = await _instance.TagDocumentsInDestinationWorkspaceWithSourceInfoAsync(_config.Object,
                documentIdentifiers, CancellationToken.None).ConfigureAwait(false);

            //Assert
            executionResult.Message.Should().Be(message);
            executionResult.Status.Should().Be(ExecutionStatus.Failed);

            _jobHistory.Verify(x => x.CreateAsync(_SOURCE_ARTIFACT_ID, _JOB_HISTORY_ARTIFACT_ID, It.Is<CreateJobHistoryErrorDto>(y => CheckCreateJobHistory(y, message))));
        }

        [Test]
        public async Task ItShouldReturnFailedArtifactIdsTest()
        {
            //Arrange
            const int count = 3;
            const string message = "Failed to tag synchronized documents in workspace. The first 3 out of 3 are: 1,2,3.";
            IEnumerable<int> listOfDocuments = Enumerable.Range(1, count);
            IEnumerable<int> documentIdentifiers = new List<int>(new[] { 1 });

            List<TagDocumentsResult<int>> documentsResult = new List<TagDocumentsResult<int>>()
            {
                new TagDocumentsResult<int>(listOfDocuments, "1", false, 1),
            };

            _destinationWorkspace.Setup(x => x.TagDocumentsAsync(It.IsAny<ISynchronizationConfiguration>(), It.IsAny<IList<int>>(), It.IsAny<CancellationToken>())).ReturnsAsync(documentsResult);

            //Act
            ExecutionResult executionResult = await _instance.TagDocumentsInSourceWorkspaceWithDestinationInfoAsync(_config.Object,
                documentIdentifiers, CancellationToken.None).ConfigureAwait(false);

            //Assert
            executionResult.Message.Should().Be(message);
            executionResult.Status.Should().Be(ExecutionStatus.Failed);

            _jobHistory.Verify(x => x.CreateAsync(_SOURCE_ARTIFACT_ID, _JOB_HISTORY_ARTIFACT_ID, It.Is<CreateJobHistoryErrorDto>(y => CheckCreateJobHistory(y, message))));
        }

        [Test]
        public async Task ItShouldReturnSuccessForSourceWorkspaceTest()
        {
            //Arrange
            IEnumerable<int> documentIdentifiers = new List<int>(new[] { 1 });
            List<TagDocumentsResult<int>> documentsResult = new List<TagDocumentsResult<int>>();

            _destinationWorkspace.Setup(x => x.TagDocumentsAsync(It.IsAny<ISynchronizationConfiguration>(), It.IsAny<IList<int>>(), It.IsAny<CancellationToken>())).ReturnsAsync(documentsResult);

            //Act
            ExecutionResult executionResult = await _instance.TagDocumentsInSourceWorkspaceWithDestinationInfoAsync(_config.Object,
                documentIdentifiers, CancellationToken.None).ConfigureAwait(false);

            //Assert
            executionResult.Message.Should().Be("");
            executionResult.Status.Should().Be(ExecutionStatus.Completed);
        }

        [Test]
        public async Task ItShouldReturnSuccessForDestinationWorkspaceTest()
        {
            //Arrange
            IEnumerable<string> documentIdentifiers = new List<string>(new[] { "documentIdentifiers" });
            List<TagDocumentsResult<string>> documentsResult = new List<TagDocumentsResult<string>>();

            _sourceWorkspace.Setup(x => x.TagDocumentsAsync(It.IsAny<ISynchronizationConfiguration>(), It.IsAny<IList<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(documentsResult);

            //Act
            ExecutionResult executionResult = await _instance.TagDocumentsInDestinationWorkspaceWithSourceInfoAsync(_config.Object,
                documentIdentifiers, CancellationToken.None).ConfigureAwait(false);

            //Assert
            executionResult.Message.Should().Be("");
            executionResult.Status.Should().Be(ExecutionStatus.Completed);
        }

        private bool CheckCreateJobHistory(CreateJobHistoryErrorDto createJobHistory, string message)
        {
            createJobHistory.ErrorMessage.Should().Be(message);
            createJobHistory.StackTrace.Should().Be(null);
            createJobHistory.ErrorType.Should().Be(ErrorType.Job);
            return true;
        }
    }
}
