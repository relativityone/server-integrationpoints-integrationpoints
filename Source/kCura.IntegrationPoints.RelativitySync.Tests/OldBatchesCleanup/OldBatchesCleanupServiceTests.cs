using System;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.RelativitySync.OldBatchesCleanup;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Sync.Storage;

namespace kCura.IntegrationPoints.RelativitySync.Tests.OldBatchesCleanup
{
	[TestFixture, Category("Unit")]
	public class OldBatchesCleanupServiceTests
	{
		private const int WORKSPACE_ID = 123987;
		private const int BATCH_EXPIRATION_IN_DAYS = 7;

		private Mock<IBatchRepository> _batchRepositoryMock;
		private Lazy<IErrorService> _errorServiceFactoryFake;
		private Mock<IErrorService> _errorServiceMock;
		private Mock<IAPILog> _apiLogFake;

		private OldBatchesCleanupService _sut;

		[SetUp]
		public void SetUp()
		{
			_batchRepositoryMock = new Mock<IBatchRepository>();
			_errorServiceMock = new Mock<IErrorService>();
			_errorServiceFactoryFake = new Lazy<IErrorService>(() => _errorServiceMock.Object);
			_apiLogFake = new Mock<IAPILog>();
			_sut = new OldBatchesCleanupService(_batchRepositoryMock.Object, _errorServiceFactoryFake, _apiLogFake.Object);
		}

		[Test]
		public async Task TryToDeleteOldBatchesInWorkspaceAsync_ShouldDeleteBatches_WhenOlderThanSevenDays()
		{
			// Act
			await _sut.TryToDeleteOldBatchesInWorkspaceAsync(WORKSPACE_ID).ConfigureAwait(false);

			// Assert
			_batchRepositoryMock.Verify(x => x.DeleteAllOlderThanAsync(WORKSPACE_ID, TimeSpan.FromDays(BATCH_EXPIRATION_IN_DAYS)));
		}

		[Test]
		public void TryToDeleteOldBatchesInWorkspaceAsync_ShouldNotThrow_WhenBatchRepositoryFails()
		{
			// Arrange
			_batchRepositoryMock.Setup(x => x.DeleteAllOlderThanAsync(It.IsAny<int>(), It.IsAny<TimeSpan>())).Throws<InvalidOperationException>();

			// Act
			Func<Task> action = () => _sut.TryToDeleteOldBatchesInWorkspaceAsync(WORKSPACE_ID);

			// Assert
			action.ShouldNotThrow();
		}

		[Test]
		public async Task TryToDeleteOldBatchesInWorkspaceAsync_ShouldLogToErrorTab_WhenExceptionWasThrown()
		{
			// Arrange
			_batchRepositoryMock.Setup(x => x.DeleteAllOlderThanAsync(It.IsAny<int>(), It.IsAny<TimeSpan>())).Throws<InvalidOperationException>();

			// Act
			await _sut.TryToDeleteOldBatchesInWorkspaceAsync(WORKSPACE_ID).ConfigureAwait(false);

			// Assert
			_errorServiceMock.Verify(x => x.Log(It.Is<ErrorModel>(model => model.AddToErrorTab)), Times.Once);
		}
	}
}