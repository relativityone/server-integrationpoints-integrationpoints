using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Unit.Storage
{
	[TestFixture]
	internal sealed class JobHistoryErrorRepositoryTests
	{
		private Mock<ISourceServiceFactoryForAdmin> _serviceFactory;
		private JobHistoryErrorRepository _jobHistoryErrorRepository;

		[SetUp]
		public void SetUp()
		{
			_serviceFactory = new Mock<ISourceServiceFactoryForAdmin>();
			_jobHistoryErrorRepository = new JobHistoryErrorRepository(_serviceFactory.Object);
		}

		[Test]
		public async Task ItShouldCreateJobHistoryError()
		{
			const int workspaceArtifactId = 1;
			const int jobHistoryArtifactId = 2;
			ErrorType errorType = ErrorType.Item;
			const string stackTrace = "Stack trace";
			const string sourceUniqueId = "src unique id";
			const string errorMessage = "Some message.";
			CreateJobHistoryErrorDto createJobHistoryErrorDto = new CreateJobHistoryErrorDto(jobHistoryArtifactId, errorType)
			{
				ErrorMessage = errorMessage,
				SourceUniqueId = sourceUniqueId,
				StackTrace = stackTrace
			};
			Mock<IObjectManager> objectManager = new Mock<IObjectManager>();
			CreateResult createResult = new CreateResult()
			{
				Object = new RelativityObject()
			};
			objectManager.Setup(x => x.CreateAsync(It.IsAny<int>(), It.IsAny<CreateRequest>())).ReturnsAsync(createResult);
			_serviceFactory.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(objectManager.Object);

			// act
			IJobHistoryError jobHistoryError = await _jobHistoryErrorRepository.CreateAsync(workspaceArtifactId, createJobHistoryErrorDto).ConfigureAwait(false);

			// assert
			jobHistoryError.ErrorMessage.Should().Be(errorMessage);
			jobHistoryError.SourceUniqueId.Should().Be(sourceUniqueId);
			jobHistoryError.StackTrace.Should().Be(stackTrace);
			jobHistoryError.ErrorType.Should().Be(errorType);
			jobHistoryError.ErrorStatus.Should().Be(ErrorStatus.New);
		}
	}
}