﻿using kCura.IntegrationPoints.UITests.Pages;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.UITests.Common;
using NUnit.Framework;

namespace kCura.IntegrationPoints.UITests.Tests
{
	[TestFixture]
	[Category(TestCategory.MISCELLANEOUS)]
    public class UserOnLoginPageTest : UiTest
	{
		public UserOnLoginPageTest() : base(shouldLoginToRelativity: false)
		{
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
			var loginPage = new LoginPage(Driver);
			var generalPage = new GeneralPage(Driver);

			// Act
			loginPage.Login(SharedVariables.RelativityUserName, SharedVariables.RelativityPassword);

			// Act / Assert
			generalPage.ChooseWorkspace(Context.WorkspaceName);
		}
	}
}