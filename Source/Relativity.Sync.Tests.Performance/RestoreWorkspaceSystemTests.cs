using FluentAssertions;
using NUnit.Framework;
using System.IO;
using System.Threading.Tasks;

namespace Relativity.Sync.Tests.Performance
{
	[TestFixture]
	public class RestoreWorkspaceSystemTests : PerformanceTestsBase
	{
		[SetUp]
		public void SetUp()
		{
			ARMHelper.EnableAgents();
		}

		[Test]
		public async Task Restore_SmallSaltPepperWorkspace()
		{
			// Arrange
			string filePath = await StorageHelper.DownloadFileAsync("SmallSaltPepperWorkspace.zip", Path.GetTempPath()).ConfigureAwait(false);

			// Act
			int workspaceID = await ARMHelper.RestoreWorkspaceAsync(filePath).ConfigureAwait(false);

			// Assert
			workspaceID.Should().NotBe(0);
		}
	}
}
