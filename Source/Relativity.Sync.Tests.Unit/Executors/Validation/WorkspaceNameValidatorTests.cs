using Relativity.API;
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

		private Mock<IAPILog> _syncLog;

		private WorkspaceNameValidator _instance;

		[SetUp]
		public void SetUp()
		{
			_cancellationToken = CancellationToken.None;

			_syncLog = new Mock<IAPILog>();

			_instance = new WorkspaceNameValidator(_syncLog.Object);
		}

		[Test]
		public void ValidateGoldFlowTest()
		{
			// Arrange
			string testWorkspaceName = "GoodWorkspace Name";
			int testWorkspaceArtifactId = 1;

			// Act
			bool actualResult = _instance.Validate(testWorkspaceName, testWorkspaceArtifactId, _cancellationToken);

			// Assert
			Assert.IsTrue(actualResult);
			_syncLog.Verify(x => x.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
		}

		[Test]
		public void ValidateInvalidWorkspaceNameCharacterTest()
		{
			// Arrange
			string testWorkspaceName = "My Bad; Workspace";
			int testWorkspaceArtifactId = 1;

			// Act
			bool actualResult = _instance.Validate(testWorkspaceName, testWorkspaceArtifactId, _cancellationToken);

			// Assert
			Assert.IsFalse(actualResult);
			_syncLog.Verify(x => x.LogError(
				It.Is<string>(y => y.StartsWith("Invalid workspace name", StringComparison.InvariantCulture)),
				It.Is<object[]>(y => (int)y[0] == testWorkspaceArtifactId)), Times.Once);
		}
	}
}
