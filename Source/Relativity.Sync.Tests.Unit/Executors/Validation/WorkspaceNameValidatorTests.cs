using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Executors;
using Relativity.Sync.Executors.Validation;

namespace Relativity.Sync.Tests.Unit.Executors.Validation
{
	[TestFixture]
	public sealed class WorkspaceNameValidatorTests
	{
		private CancellationToken _cancellationToken;

		private Mock<ISyncLog> _syncLog;
		private Mock<IWorkspaceNameQuery> _workspaceNameQuery;

		private WorkspaceNameValidator _instance;

		private const int _TEST_WORKSPACE_ARTIFACT_ID = 101202;

		[SetUp]
		public void SetUp()
		{
			_cancellationToken = CancellationToken.None;

			_syncLog = new Mock<ISyncLog>();
			_workspaceNameQuery = new Mock<IWorkspaceNameQuery>();

			_instance = new WorkspaceNameValidator(_workspaceNameQuery.Object, _syncLog.Object);
		}

		[Test]
		public async Task ValidateWorkspaceNameAsyncGoldFlowTest()
		{
			// Arrange
			_workspaceNameQuery.Setup(x => x.GetWorkspaceNameAsync(_TEST_WORKSPACE_ARTIFACT_ID, _cancellationToken)).ReturnsAsync("My Good Workspace").Verifiable();

			// Act
			bool actualResult = await _instance.ValidateWorkspaceNameAsync(_TEST_WORKSPACE_ARTIFACT_ID, _cancellationToken).ConfigureAwait(false);

			// Assert
			Assert.IsTrue(actualResult);

			Mock.VerifyAll(_workspaceNameQuery);
			_syncLog.Verify(x => x.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
		}

		[Test]
		public async Task ValidateWorkspaceNameAsyncInvalidWorkspaceNameCharacterTest()
		{
			// Arrange
			string testWorkspaceName = "My Bad; Workspace";
			_workspaceNameQuery.Setup(x => x.GetWorkspaceNameAsync(_TEST_WORKSPACE_ARTIFACT_ID, _cancellationToken)).ReturnsAsync(testWorkspaceName).Verifiable();

			// Act
			bool actualResult = await _instance.ValidateWorkspaceNameAsync(_TEST_WORKSPACE_ARTIFACT_ID, _cancellationToken).ConfigureAwait(false);

			// Assert
			Assert.IsFalse(actualResult);

			Mock.VerifyAll(_workspaceNameQuery);
			_syncLog.Verify(x => x.LogError(
				It.Is<string>(y => y.StartsWith("Invalid workspace name:", StringComparison.InvariantCulture)),
				It.Is<object[]>(y => (string)y[0] == testWorkspaceName && (int)y[1] == _TEST_WORKSPACE_ARTIFACT_ID)), Times.Once);
		}

		[Test]
		public async Task ValidateWorkspaceNameAsyncGetWorkspaceNameAsyncThrowsExceptionTest()
		{
			// Arrange
			_workspaceNameQuery.Setup(x => x.GetWorkspaceNameAsync(_TEST_WORKSPACE_ARTIFACT_ID, _cancellationToken)).ThrowsAsync(new Exception()).Verifiable();

			// Act
			bool actualResult = await _instance.ValidateWorkspaceNameAsync(_TEST_WORKSPACE_ARTIFACT_ID, _cancellationToken).ConfigureAwait(false);

			// Assert
			Assert.IsFalse(actualResult);

			Mock.VerifyAll(_workspaceNameQuery);
			_syncLog.Verify(x => x.LogError(
				It.IsAny<Exception>(),
				It.Is<string>(y => y.StartsWith("Error occurred", StringComparison.InvariantCulture)),
				It.Is<object[]>(y => (int)y[0] == _TEST_WORKSPACE_ARTIFACT_ID)), Times.Once);
		}
	}
}