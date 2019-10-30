using System;
using kCura.IntegrationPoints.RelativitySync.OldBatchesCleanup;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Storage;

namespace kCura.IntegrationPoints.RelativitySync.Tests.OldBatchesCleanup
{
	[TestFixture]
	public class OldBatchesCleanupServiceTests
	{
		private const int WORKSPACE_ID = 123987;
		private const int OLD_BATCH_DAYS_AMOUNT = 7;

		private Mock<IBatchRepository> _batchRepositoryMock;

		private IOldBatchesCleanupService _sut;

		[SetUp]
		public void SetUp()
		{
			_batchRepositoryMock = new Mock<IBatchRepository>();
			_sut = new OldBatchesCleanupService(_batchRepositoryMock.Object);
		}

		[Test]
		public void ItShouldDeleteBatchesOlderThan7Days()
		{
			// Act
			_sut.DeleteOldBatchesInWorkspaceAsync(WORKSPACE_ID);

			// Assert
			_batchRepositoryMock.Verify(x => x.DeleteAllOlderThanAsync(WORKSPACE_ID, TimeSpan.FromDays(OLD_BATCH_DAYS_AMOUNT)));
		}
	}
}