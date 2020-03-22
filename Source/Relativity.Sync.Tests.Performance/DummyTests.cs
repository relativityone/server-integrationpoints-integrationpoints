using NUnit.Framework;
using Relativity.Sync.Tests.Performance.Helpers;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Relativity.Sync.Tests.Performance
{
	[TestFixture]
	public class DummyTests : PerformanceTestsBase
	{
		[SetUp]
		public void SetUp()
		{
			ARMHelper.EnableAgents();
		}

		[Test]
		public async Task Test()
		{
			string filePath = await StorageHelper.DownloadFileAsync("TestWorkspace.zip", Path.GetTempPath()).ConfigureAwait(false);

			await ARMHelper.RestoreWorkspaceAsync(filePath).ConfigureAwait(false);
		}
	}
}
