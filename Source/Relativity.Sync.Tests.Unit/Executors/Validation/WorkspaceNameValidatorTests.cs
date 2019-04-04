using System;
using System.Threading;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Executors.Validation;

namespace Relativity.Sync.Tests.Unit.Executors.Validation
{
	[TestFixture]
	public sealed class WorkspaceNameValidatorTests
	{
		private CancellationToken _cancellationToken;

		private Mock<ISyncLog> _syncLog;

		private WorkspaceNameValidator _instance;

		[SetUp]
		public void SetUp()
		{
			_cancellationToken = CancellationToken.None;

			_syncLog = new Mock<ISyncLog>();

			_instance = new WorkspaceNameValidator(_syncLog.Object);
		}

		[Test]
		public void ValidateGoldFlowTest()
		{
			// Arrange
			string testWorkspaceName = "GoodWorkspace Name";

			// Act
			bool actualResult = _instance.Validate(testWorkspaceName, _cancellationToken);

			// Assert
			Assert.IsTrue(actualResult);
			_syncLog.Verify(x => x.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
		}

		[Test]
		public void ValidateInvalidWorkspaceNameCharacterTest()
		{
			// Arrange
			string testWorkspaceName = "My Bad; Workspace";

			// Act
			bool actualResult = _instance.Validate(testWorkspaceName, _cancellationToken);

			// Assert
			Assert.IsFalse(actualResult);
			_syncLog.Verify(x => x.LogError(
				It.Is<string>(y => y.StartsWith("Invalid workspace name:", StringComparison.InvariantCulture)),
				It.Is<object[]>(y => (string)y[0] == testWorkspaceName)), Times.Once);
		}
	}
}