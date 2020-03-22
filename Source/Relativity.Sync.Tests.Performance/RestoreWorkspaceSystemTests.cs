using FluentAssertions;
using NUnit.Framework;
using Relativity.Sync.Tests.Performance.Helpers;
using System;
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
		public async Task Test()
		{
			// Arrange
			string filePath = await StorageHelper.DownloadFileAsync("TestWorkspace.zip", Path.GetTempPath()).ConfigureAwait(false);

			// Act
			int workspaceID = await ARMHelper.RestoreWorkspaceAsync(filePath).ConfigureAwait(false);

			// Assert
			workspaceID.Should().NotBe(0);
		}
	}
}
