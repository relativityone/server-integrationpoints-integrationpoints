using System;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.RelativitySync.RdoCleanup;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.ArtifactGuid;
using Relativity.Services.Objects;
using Relativity.Services.Interfaces.ObjectType;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.RelativitySync.Tests.RdoCleanup
{
	[TestFixture]
	public class SyncRdoCleanupServiceTests
	{
		private readonly Guid _progressObjectType = new Guid("3D107450-DB18-4FE1-8219-73EE1F921ED9");
		private readonly Guid _batchObjectType = new Guid("18C766EB-EB71-49E4-983E-FFDE29B1A44E");
		private readonly Guid _syncConfigurationObjectType = new Guid("3BE3DE56-839F-4F0E-8446-E1691ED5FD57");

		private const int _WORKSPACE_ID = 11111;

		private Mock<IServicesMgr> _servicesMgrMock;
		private Mock<IArtifactGuidManager> _artifactGuidManager;
		private Mock<IObjectTypeManager> _objectTypeManager;
		private Mock<IObjectManager> _objectManager;
		private Mock<IAPILog> _loggerFake;
		private SyncRdoCleanupService _sut;

		[SetUp]
		public void Setup()
		{
			_objectTypeManager = new Mock<IObjectTypeManager>();
			_objectManager = new Mock<IObjectManager>();

			_artifactGuidManager = new Mock<IArtifactGuidManager>();
			_artifactGuidManager.Setup(x => x.GuidExistsAsync(_WORKSPACE_ID, _progressObjectType)).ReturnsAsync(true);
			_artifactGuidManager.Setup(x => x.GuidExistsAsync(_WORKSPACE_ID, _batchObjectType)).ReturnsAsync(true);
			_artifactGuidManager.Setup(x => x.GuidExistsAsync(_WORKSPACE_ID, _syncConfigurationObjectType)).ReturnsAsync(true);

			_servicesMgrMock = new Mock<IServicesMgr>();
			_servicesMgrMock.Setup(x => x.CreateProxy<IArtifactGuidManager>(ExecutionIdentity.System)).Returns(_artifactGuidManager.Object);
			_servicesMgrMock.Setup(x => x.CreateProxy<IObjectTypeManager>(ExecutionIdentity.System)).Returns(_objectTypeManager.Object);
			_servicesMgrMock.Setup(x => x.CreateProxy<IObjectManager>(ExecutionIdentity.System)).Returns(_objectManager.Object);

			_objectManager.Setup(x => x.DeleteAsync(It.IsAny<int>(), It.IsAny<MassDeleteByCriteriaRequest>())).ReturnsAsync(new MassDeleteResult());

			_loggerFake = new Mock<IAPILog>();
			_sut = new SyncRdoCleanupService(_servicesMgrMock.Object, _loggerFake.Object);
		}

		[Test]
		public async Task DeleteSyncRdosAsync_ShouldMassDeleteObjectsInstances()
		{
			// Arrange

			// Act
			await _sut.DeleteSyncRdosAsync(_WORKSPACE_ID).ConfigureAwait(false);

			// Assert
			_objectManager.Verify(x => x.DeleteAsync(_WORKSPACE_ID, It.Is<MassDeleteByCriteriaRequest>(request =>
				request.ObjectIdentificationCriteria.ObjectType.Guid == _progressObjectType)), Times.Once);
			_objectManager.Verify(x => x.DeleteAsync(_WORKSPACE_ID, It.Is<MassDeleteByCriteriaRequest>(request =>
				request.ObjectIdentificationCriteria.ObjectType.Guid == _batchObjectType)), Times.Once);
			_objectManager.Verify(x => x.DeleteAsync(_WORKSPACE_ID, It.Is<MassDeleteByCriteriaRequest>(request =>
				request.ObjectIdentificationCriteria.ObjectType.Guid == _syncConfigurationObjectType)), Times.Once);
		}

		[Test]
		public async Task DeleteSyncRdosAsync_ShouldDeleteObjectTypes()
		{
			// Arrange
			const int progressObjectTypeId = 222;
			const int batchObjectTypeId = 222;
			const int configurationObjectTypeId = 222;

			_artifactGuidManager.Setup(x => x.ReadSingleArtifactIdAsync(_WORKSPACE_ID, _progressObjectType)).ReturnsAsync(progressObjectTypeId);
			_artifactGuidManager.Setup(x => x.ReadSingleArtifactIdAsync(_WORKSPACE_ID, _progressObjectType)).ReturnsAsync(batchObjectTypeId);
			_artifactGuidManager.Setup(x => x.ReadSingleArtifactIdAsync(_WORKSPACE_ID, _progressObjectType)).ReturnsAsync(configurationObjectTypeId);

			// Act
			await _sut.DeleteSyncRdosAsync(_WORKSPACE_ID).ConfigureAwait(false);

			// Assert
			_objectTypeManager.Verify(x => x.DeleteAsync(_WORKSPACE_ID, progressObjectTypeId), Times.Once());
			_objectTypeManager.Verify(x => x.DeleteAsync(_WORKSPACE_ID, batchObjectTypeId), Times.Once());
			_objectTypeManager.Verify(x => x.DeleteAsync(_WORKSPACE_ID, configurationObjectTypeId), Times.Once());
		}

        [Test]
        public void DeleteSyncRdosAsync_ShouldNotThrowExceptionWhenObjectManagerThrowsException()
        {
			// Arrange
            _objectManager.Setup(x => x.DeleteAsync(It.IsAny<int>(), It.IsAny<MassDeleteByCriteriaRequest>()))
                .Throws(new Exception());

			// Act
            Func<Task> action = async () => await _sut.DeleteSyncRdosAsync(_WORKSPACE_ID).ConfigureAwait(false);

			// Assert
            action.ShouldNotThrow();
		}
	}
}