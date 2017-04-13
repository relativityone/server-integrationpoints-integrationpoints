using IntegrationPointsUITests.Config;
using IntegrationPointsUITests.Pages;
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
            loginPage.Login(TestConfig.Username, TestConfig.Password);
        }

    }
}
