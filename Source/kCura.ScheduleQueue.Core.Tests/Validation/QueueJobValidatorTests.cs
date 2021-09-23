using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core.Validation;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;

namespace kCura.ScheduleQueue.Core.Tests.Validation
{
	[TestFixture, Category("Unit")]
	public class QueueJobValidatorTests
	{
		private Mock<IObjectManager> _objectManagerMock;

		private const int _TEST_WORKSPACE_ID = 100;
		private const int _TEST_INTEGRATION_POINT_ID = 200;

		[Test]
		public async Task ValidateAsync_ShouldValidateJobAsValid_WhenContextWorkspaceAndIntegrationPointsExist()
		{
			// Arrange
			Job job = new JobBuilder()
				.WithWorkspaceId(_TEST_WORKSPACE_ID)
				.WithRelatedObjectArtifactId(_TEST_INTEGRATION_POINT_ID)
				.Build();

			QueueJobValidator sut = GetSut();

			SetUpWorkspaceExists(true);
			SetUpIntegrationPointExists(true);

			// Act
			ValidationResult result = await sut.ValidateAsync(job).ConfigureAwait(false);

			// Assert
			result.IsValid.Should().BeTrue();
			result.Message.Should().BeEmpty();
		}

		[Test]
		public async Task ValidateAsync_ShouldValidateJobAsInvalid_WhenContextWorkspaceDoesNotExist()
		{
			// Arrange
			Job job = new JobBuilder()
				.WithWorkspaceId(_TEST_WORKSPACE_ID)
				.WithRelatedObjectArtifactId(_TEST_INTEGRATION_POINT_ID)
				.Build();

			QueueJobValidator sut = GetSut();

			SetUpWorkspaceExists(false);
			SetUpIntegrationPointExists(false);

			// Act
			ValidationResult result = await sut.ValidateAsync(job).ConfigureAwait(false);

			// Assert
			result.IsValid.Should().BeFalse();
			result.Message.Should().Contain(_TEST_WORKSPACE_ID.ToString());
		}

		[Test]
		public async Task ValidateAsync_ShouldValidateJobAsInvalid_WhenContextIntegrationPointDoesNotExist()
		{
			// Arrange
			Job job = new JobBuilder()
				.WithWorkspaceId(_TEST_WORKSPACE_ID)
				.WithRelatedObjectArtifactId(_TEST_INTEGRATION_POINT_ID)
				.Build();

			QueueJobValidator sut = GetSut();

			SetUpWorkspaceExists(true);
			SetUpIntegrationPointExists(false);

			// Act
			ValidationResult result = await sut.ValidateAsync(job).ConfigureAwait(false);

			// Assert
			result.IsValid.Should().BeFalse();
			result.Message.Should().Contain(_TEST_INTEGRATION_POINT_ID.ToString());
		}

		[Test]
		public async Task ValidateAsync_ShouldNotValidateIntegrationPoint_WhenContextWorkspaceDoesNotExist()
		{
			// Arrange
			Job job = new JobBuilder()
				.WithWorkspaceId(_TEST_WORKSPACE_ID)
				.WithRelatedObjectArtifactId(_TEST_INTEGRATION_POINT_ID)
				.Build();

			QueueJobValidator sut = GetSut();

			SetUpWorkspaceExists(false);
			SetUpIntegrationPointExists(false);

			// Act
			await sut.ValidateAsync(job).ConfigureAwait(false);

			// Assert
			_objectManagerMock.Verify(x => x.ReadAsync(It.IsAny<int>(), It.IsAny<ReadRequest>()), 
				Times.Never());
		}

		private QueueJobValidator GetSut()
		{
			Mock<IHelper> helper = new Mock<IHelper>();
			Mock<IServicesMgr> servicesMgr = new Mock<IServicesMgr>();
			_objectManagerMock = new Mock<IObjectManager>();

			helper.Setup(x => x.GetServicesManager()).Returns(servicesMgr.Object);
			servicesMgr.Setup(x => x.CreateProxy<IObjectManager>(It.IsAny<ExecutionIdentity>()))
				.Returns(_objectManagerMock.Object);

			return new QueueJobValidator(helper.Object);
		}

		private void SetUpWorkspaceExists(bool exists)
		{
			int expectedTotalCount = exists ? 1 : 0;

			_objectManagerMock.Setup(x => x.QuerySlimAsync(-1, 
					It.Is<QueryRequest>(r => r.Condition.Contains(_TEST_WORKSPACE_ID.ToString())), 0, 1))
				.ReturnsAsync(new QueryResultSlim
				{
					TotalCount = expectedTotalCount
				})
				.Verifiable();
		}

		private void SetUpIntegrationPointExists(bool exists)
		{
			RelativityObject expectedObject = exists ? new RelativityObject() : null;

			_objectManagerMock.Setup(x => x.ReadAsync(_TEST_WORKSPACE_ID, 
					It.Is<ReadRequest>(r => r.Object.ArtifactID == _TEST_INTEGRATION_POINT_ID)))
				.ReturnsAsync(new ReadResult()
				{
					Object = expectedObject
				})
				.Verifiable();
		}
	}
}
