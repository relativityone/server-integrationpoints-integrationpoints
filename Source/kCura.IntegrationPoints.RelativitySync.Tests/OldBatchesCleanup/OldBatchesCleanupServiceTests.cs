using System;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.RelativitySync.OldBatchesCleanup;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Sync.Storage;

namespace kCura.IntegrationPoints.RelativitySync.Tests.OldBatchesCleanup
{
	[TestFixture]
	public class OldBatchesCleanupServiceTests
	{
		private const int WORKSPACE_ID = 123987;
		private const int BATCH_EXPIRATION_IN_DAYS = 7;

		private Mock<IBatchRepository> _batchRepositoryMock;
		private Mock<Lazy<IErrorService>> _errorServiceMock;
		private Mock<IAPILog> _apiLogMock;

		private IOldBatchesCleanupService _sut;

		[SetUp]
		public void SetUp()
		{
			_batchRepositoryMock = new Mock<IBatchRepository>();
			_errorServiceMock = new Mock<Lazy<IErrorService>>();
			_apiLogMock = new Mock<IAPILog>();
			_sut = new OldBatchesCleanupService(_batchRepositoryMock.Object, _errorServiceMock.Object, _apiLogMock.Object);
		}

		[Test]
		public void DeleteOldBatchesInWorkspaceAsync_ShouldDeleteBatches_WhenOlderThanSevenDays()
		{
			// Act
			_sut.DeleteOldBatchesInWorkspaceAsync(WORKSPACE_ID);

			// Assert
			_batchRepositoryMock.Verify(x => x.DeleteAllOlderThanAsync(WORKSPACE_ID, TimeSpan.FromDays(BATCH_EXPIRATION_IN_DAYS)));
		}
	}
}