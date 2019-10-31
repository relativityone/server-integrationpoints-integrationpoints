using kCura.IntegrationPoints.EventHandlers.Commands;
using kCura.IntegrationPoints.EventHandlers.Commands.Context;
using kCura.IntegrationPoints.RelativitySync.OldBatchesCleanup;
using Moq;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Commands
{
	public class RemoveBatchesFromOldJobsCommandTests
	{
		private RemoveBatchesFromOldJobsCommand _sut;

		private Mock<IEHContext> _contextStub;
		private Mock<IOldBatchesCleanupService> _oldBatchesCleanupServiceMock;

		private const int _WORKSPACE_ID = 456987;

		[SetUp]
		public void SetUp()
		{
			_contextStub = new Mock<IEHContext>();
			_oldBatchesCleanupServiceMock = new Mock<IOldBatchesCleanupService>();

			var helperStub = new Mock<IEHHelper>();
			helperStub
				.Setup(x => x.GetActiveCaseID())
				.Returns(_WORKSPACE_ID);

			_contextStub
				.SetupGet(x => x.Helper)
				.Returns(helperStub.Object);

			_sut = new RemoveBatchesFromOldJobsCommand(_contextStub.Object, _oldBatchesCleanupServiceMock.Object);   
		}

		[Test]
		public void Execute_ShouldCallOldBatchesCleanupService()
		{
			// Act
			_sut.Execute();

			// Assert
			_oldBatchesCleanupServiceMock
				.Verify(x => x.DeleteOldBatchesInWorkspaceAsync(_WORKSPACE_ID), Times.Once);
		}
	}
}