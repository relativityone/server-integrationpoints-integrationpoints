using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.UITests.Common;
using kCura.IntegrationPoints.UITests.NUnitExtensions;
using kCura.IntegrationPoints.UITests.Pages;
using NUnit.Framework;
using Relativity.Testing.Identification;

namespace kCura.IntegrationPoints.UITests.Tests
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints]
	[Category(TestCategory.WEB_IMPORT_EXPORT)]
	[Category(TestCategory.MISCELLANEOUS)]
	public class UserOnLoginPageTest : UiTest
	{
		public UserOnLoginPageTest() : base(shouldLoginToRelativity: false, shouldImportDocuments: false)
		{
		}

		[IdentifiedTest("754f302f-24c1-44a1-bc17-ef0cbda3a36c")]
		[RetryOnError]
		[Order(10)]
		public void CanLoginSuccessfullyWithValidCredentials()
		{
			// Arrange
			var loginPage = new LoginPage(Driver);

			// Act / Assert
			loginPage.Login(SharedVariables.RelativityUserName, SharedVariables.RelativityPassword);
		}

		[IdentifiedTest("0ac01b6c-6ed2-4acb-b6ff-5cce6b3a3379")]
		[RetryOnError]
		[Order(20)]
		public void GoesToWorkspace()
		{
			// Arrange
			var loginPage = new LoginPage(Driver);
			var generalPage = new GeneralPage(Driver);

			// Act
			loginPage.Login(SharedVariables.RelativityUserName, SharedVariables.RelativityPassword);

			// Act / Assert
			generalPage.ChooseWorkspace(SourceContext.WorkspaceName);
		}
	}
}