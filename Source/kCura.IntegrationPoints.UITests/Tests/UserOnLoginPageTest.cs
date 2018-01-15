using kCura.IntegrationPoints.UITests.Pages;
using kCura.IntegrationPoint.Tests.Core;
using NUnit.Framework;

namespace kCura.IntegrationPoints.UITests.Tests
{
	[TestFixture]
	public class UserOnLoginPageTest : UiTest
	{
		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			var loginPage = new LoginPage(Driver);
			if (!loginPage.IsOnLoginPage())
			{
				new GeneralPage(Driver).LogOut();
			}
		}

		[Test, Order(10)]
		public void CanLoginSuccessfullyWithValidCredentials()
		{
			// Arrange
			var loginPage = new LoginPage(Driver);

			// Act / Assert
			loginPage.Login(SharedVariables.RelativityUserName, SharedVariables.RelativityPassword);
		}

		[Test, Order(20)]
		public void GoesToWorkspace()
		{
			// Arrange
			var generalPage = new GeneralPage(Driver);

			// Act / Assert
			generalPage.ChooseWorkspace("Smoke Workspace");
		}
	}
}