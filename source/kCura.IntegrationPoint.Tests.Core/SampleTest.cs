using kCura.IntegrationPoint.Tests.Core.Templates;
using NUnit.Framework;

namespace kCura.IntegrationPoint.Tests.Core
{
	[TestFixture]
	public class SampleTest //: WorkspaceDependentTemplate 
	{
		public SampleTest()
			//: base("WorkspaceA", "WorkspaceB")
		{
		}

		[Test]
		[Explicit]
		public void TestUser()
		{
			bool createdUser = User.CreateUserRest("first", "last", "flast@kcura.com");
		}

		[Test]
		[Explicit]
		public void TestIntegrationPoint()
		{
			bool createdIntegrationPoint = IntegrationPoint.CreateIntegrationPoint("My little integration point", 1118254,
				"Use Field Settings", false, "AppendOverlay", false, 1039795);
		}
	}
}