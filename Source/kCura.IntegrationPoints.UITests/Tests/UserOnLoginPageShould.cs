using IntegrationPointsUITests.Pages;
using kCura.IntegrationPoint.Tests.Core;
using NUnit.Framework;

namespace IntegrationPointsUITests.Tests
{
    [TestFixture]
    public class UserOnLoginPageShould : BaseUiTest
    {
        [Test]
        public void LoginSuccessfullyWithValidCredentials()
        {
            // GIVEN
            var loginPage = new LoginPage(Driver);

            // WHEN / THEN
            loginPage.Login(SharedVariables.RelativityUserName, SharedVariables.RelativityPassword);
        }

    }
}
