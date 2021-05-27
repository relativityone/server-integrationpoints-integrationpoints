using FluentAssertions;
using kCura.IntegrationPoints.Data;
using Moq;
using NUnit.Framework;
using Relativity;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.RelativitySync.Tests
{
	[TestFixture, Category("Unit")]
	public class SyncConfigurationServiceTests
	{
		private SyncConfigurationService _sut;

		private Mock<IObjectManager> _objectManagerFake;

		private const int _WORKSPACE_ID = 100;
		private const int _JOB_HISTORY_ID = 200;

		[SetUp]
		public void SetUp()
		{
			_objectManagerFake = new Mock<IObjectManager>();

			Mock<IServicesMgr> servicesMgr = new Mock<IServicesMgr>();
			servicesMgr.Setup(x => x.CreateProxy<IObjectManager>(It.IsAny<ExecutionIdentity>()))
				.Returns(_objectManagerFake.Object);

			Mock<IHelper> helper = new Mock<IHelper>();
			helper.Setup(x => x.GetServicesManager())
				.Returns(servicesMgr.Object);

			Mock<IAPILog> log = new Mock<IAPILog>();

			_sut = new SyncConfigurationService(helper.Object, log.Object);
		}

		[Test]
		public async Task TryGetResumedSyncConfigurationIdAsync_ShouldReturnConfigurationId_WhenSyncConfigurationRdoExist()
		{
			// Arrange
			const int expectedSyncConfigurationId = 10;

			SetupSyncConfiguration(true, expectedSyncConfigurationId);

			// Act
			var result = await _sut.TryGetResumedSyncConfigurationIdAsync(_WORKSPACE_ID, _JOB_HISTORY_ID).ConfigureAwait(false);

			// Assert
			result.Should().Be(expectedSyncConfigurationId);
		}

		[Test]
		public async Task TryGetResumedSyncConfigurationIdAsync_ShouldReturnNullConfigurationId_WhenSyncConfigurationRdoDoesNotExist()
		{
			// Arrange
			SetupSyncConfiguration(false);

			// Act
			var result = await _sut.TryGetResumedSyncConfigurationIdAsync(_WORKSPACE_ID, _JOB_HISTORY_ID).ConfigureAwait(false);

			// Assert
			result.Should().BeNull();
		}

		[Test]
		public async Task TryGetResumedSyncConfigurationIdAsync_ShouldReturnNullConfigurationId_WhenSyncConfigurationDoesNotExist()
		{
			// Arrange
			SetupSyncConfiguration(true);

			// Act
			var result = await _sut.TryGetResumedSyncConfigurationIdAsync(_WORKSPACE_ID, _JOB_HISTORY_ID).ConfigureAwait(false);

			// Assert
			result.Should().BeNull();
		}

		[Test]
		public async Task TryGetResumedSyncConfigurationIdAsync_ShouldReturnNullConfigurationId_WhenExistsMoreThanOneSyncConfigurationForJobHistoryId()
		{
			// Arrange
			int?[] multipleSyncConfigurations = new int?[] { 10, 20 };

			SetupSyncConfiguration(true, multipleSyncConfigurations);

			// Act
			var result = await _sut.TryGetResumedSyncConfigurationIdAsync(_WORKSPACE_ID, _JOB_HISTORY_ID).ConfigureAwait(false);

			// Assert
			result.Should().BeNull();
		}

		private void SetupSyncConfiguration(bool rdoExists, params int?[] syncConfigurationIds)
		{
			_objectManagerFake.Setup(x => x.QuerySlimAsync(_WORKSPACE_ID, It.Is<QueryRequest>(
					q => q.ObjectType.ArtifactTypeID == (int)ArtifactType.ObjectType), 0, 1))
				.ReturnsAsync(new QueryResultSlim
				{
					Objects = rdoExists
						? new List<RelativityObjectSlim>() { new RelativityObjectSlim() }
						: new List<RelativityObjectSlim>()
				});

			_objectManagerFake.Setup(x => x.QuerySlimAsync(_WORKSPACE_ID, It.Is<QueryRequest>(
					q => q.ObjectType.Guid == ObjectTypeGuids.SyncConfigurationGuid), 0, It.IsAny<int>()))
				.ReturnsAsync(new QueryResultSlim
				{
					Objects = syncConfigurationIds != null
						? syncConfigurationIds.Select(x => new RelativityObjectSlim { ArtifactID = x.Value}).ToList()
						: new List<RelativityObjectSlim>()
				});


		}
	}
}
