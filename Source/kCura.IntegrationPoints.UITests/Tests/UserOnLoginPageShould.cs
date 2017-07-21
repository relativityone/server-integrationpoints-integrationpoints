﻿using System;
using IntegrationPointsUITests.Pages;
using kCura.IntegrationPoint.Tests.Core;
using NUnit.Framework;

namespace IntegrationPointsUITests.Tests
{
    [TestFixture]
    public class UserOnLoginPageShould : UiTest
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            Console.WriteLine("login Setup");
            var loginPage = new LoginPage(Driver);
            if (!loginPage.IsOnLoginPage())
            {
                new GeneralPage(Driver).LogOut();
            }
        }

        [Test, Order(10)]
        public void LoginSuccessfullyWithValidCredentials()
        {
            // GIVEN
            var loginPage = new LoginPage(Driver);

            // WHEN / THEN
            loginPage.Login(SharedVariables.RelativityUserName, SharedVariables.RelativityPassword);
        }

        [Test, Order(20)]
        public void GoToWorkspace()
        {
            // GIVEN
            var generalPage = new GeneralPage(Driver);

            // WHEN / THEN
            generalPage.ChooseWorkspace("Smoke Workspace");
        }
    }
}
