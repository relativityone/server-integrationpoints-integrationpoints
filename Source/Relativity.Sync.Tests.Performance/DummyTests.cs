using NUnit.Framework;
using Relativity.Sync.Tests.Performance.Helpers;

namespace Relativity.Sync.Tests.Performance
{
	[TestFixture]
	public class DummyTests : PerformanceTestsBase
	{
		[Test]
		public void Test()
		{
			ARMHelper.RestoreWorkspace(@"C:\_Work\_Temp\ARM_Test_Data\1014823_New_Case_Template_20200316110000.zip");
		}
	}
}
