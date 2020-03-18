using NUnit.Framework;
using Relativity.Sync.Tests.Performance.Helpers;
using System.IO;

namespace Relativity.Sync.Tests.Performance
{
	[TestFixture]
	public class DummyTests : PerformanceTestsBase
	{
		[Test]
		public void Test()
		{
			string filePath = StorageHelper.DownloadFile("1014823_New_Case_Template_20200316110000.zip", Path.GetTempPath());

			ARMHelper.RestoreWorkspace(filePath);
		}
	}
}
