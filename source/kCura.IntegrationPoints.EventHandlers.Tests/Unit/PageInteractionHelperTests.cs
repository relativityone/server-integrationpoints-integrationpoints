using NUnit.Framework;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Unit
{
	public class PageInteractionHelperTests
	{
		[Test]
		public void ReturnCorrectApplicationPath()
		{
			// ARRANGE
			string relativityUrl = "http://localhost/Relativity/Case/Mask/EditField.aspx?AppID=1118254&ArtifactID=1051073&ArtifactTypeID=1000053";
			string expectedUrl = "http://localhost/Relativity/CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C";

			// ACT
			string applicationUrl = PageInteractionHelper.GetApplicationPath(relativityUrl);

			// ASSERT
			Assert.AreEqual(expectedUrl, applicationUrl);
		}
	}
}
