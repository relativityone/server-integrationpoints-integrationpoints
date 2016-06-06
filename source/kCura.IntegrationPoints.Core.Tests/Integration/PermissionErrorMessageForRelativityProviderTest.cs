using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Templates;
using NUnit.Framework;
using System;

namespace kCura.IntegrationPoints.Core.Tests.Integration
{
    using Core.Models;
    using Data;
    using Data.Repositories;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using Group = kCura.IntegrationPoint.Tests.Core.Group;

    [Explicit]
    public class PermissionErrorMessageForRelativityProviderTest : WorkspaceDependentTemplate
    {

        private IObjectTypeRepository _objectTypeRepository;

        private int _userCreated;
        private int _groupCreated;
        public PermissionErrorMessageForRelativityProviderTest()
            : base("Error Source Workspace", "Error Target Workspace")
        {
        }

        [TestFixtureSetUp]
        public override void SetUp()
        {
            base.SetUp();
            _objectTypeRepository = Container.Resolve<IObjectTypeRepository>();
        }

        [SetUp]
        public void TestSetUp()
        {
            CloseSeleniumBrowser();
            Selenium.WebDriver = new ChromeDriver();
        }


        private static object[] PermissionCase = new[]
                                             {
                                                 new object[] { new List<string> {}, new List<string> {"Allow Import", "Allow Export"}, new List<string> { }, new List<string> {"Documents", "Integration Points"}},
                                                 new object[] { new List<string> {"Document", "Integration Point", "Search"}, new List<string> { }, new List<string> { }, new List<string> { "Documents", "Integration Points"}},
                                                 new object[] { new List<string> { }, new List<string> { }, new List<string> {"Folders", "Advanced & Saved Searches"}, new List<string> {"Documents", "Integration Points"} }
                                             };

        [Explicit]
        [Test, TestCaseSource("PermissionCase")]
        public void VerifyRelativityPermissionErrorMessage(List<string> obj, List<string> admin, List<string> browser, List<string> tab)
        {
            string errorMessage = Core.Constants.IntegrationPoints.PermissionErrors.INSUFFICIENT_PERMISSIONS;
            string jobError = "//div[contains(.,'Failed to submit integration job. You do not have sufficient permissions. Please contact your system administrator.')]";
            string runNowId = "_dynamicTemplate__kCuraScrollingDiv__dynamicViewFieldRenderer_ctl17_anchor";
            string okPath = "//button[contains(.,'OK')]";

            string groupName = "Permission Group" + DateTime.Now;
            Regex regex = new Regex("[^a-zA-Z0-9]");
            string tempEmail = regex.Replace(DateTime.Now.ToString(), "") + "test@kcura.com";

            PermissionProperty tempP = new PermissionProperty() { };
            tempP.Admin = admin;
            tempP.Tab = tab;
            tempP.Browser = browser;
            tempP.Obj = obj;

            int groupId = Group.CreateGroup(groupName);
            _groupCreated = groupId;
            Group.AddGroupToWorkspace(SourceWorkspaceArtifactId, groupId);
            Permission.SetPermissions(SourceWorkspaceArtifactId, groupId, tempP);

            UserModel user = User.CreateUser("tester", "tester", tempEmail, new[] { groupId });
            _userCreated = user.ArtifactId;


            Selenium.LogIntoRelativity(tempEmail, SharedVariables.RelativityPassword);
            Selenium.GoToWorkspace(SourceWorkspaceArtifactId);

            IntegrationModel model = new IntegrationModel()
            {
                SourceProvider = RelativityProvider.ArtifactId,
                Name = "RIP test" + DateTime.Now,
                DestinationProvider = DestinationProvider.ArtifactId,
                SourceConfiguration = CreateDefaultSourceConfig(),
                Destination = CreateDefaultDestinationConfig(),
                Map = CreateDefaultFieldMap(),
                Scheduler = new Scheduler()
                {
                    EnableScheduler = false
                },
                SelectedOverwrite = "Append Only",
            };
            model = CreateOrUpdateIntegrationPoint(model);

            int? artifactTypeId = _objectTypeRepository.RetrieveObjectTypeDescriptorArtifactTypeId(new Guid(ObjectTypeGuids.IntegrationPoint));

            Selenium.GoToObjectInstance(SourceWorkspaceArtifactId, model.ArtifactID, artifactTypeId.Value);
            Assert.IsTrue(Selenium.PageShouldContain(errorMessage));
            Selenium.WaitUntilIdIsClickable(runNowId, 10);
            Selenium.WebDriver.FindElement(By.Id(runNowId)).Click();

            Selenium.WaitUntilXpathExists(okPath, 10);
            Selenium.WebDriver.FindElement(By.XPath(okPath)).Click();
            Selenium.WaitUntilXpathExists(jobError, 10);

        }

        private static object[] PermissionNoExport = new[]
                                             {
                                                 new object[] {
                                                     new List<string> {"Document", "Integration Point", "Search"},
                                                     new List<string> {"Allow Import"},
                                                     new List<string> {"Folders", "Advanced & Saved Searches"},
                                                     new List<string> { "Documents", "Integration Points"}}
                                             };

        [Explicit]
        [Test, TestCaseSource("PermissionNoExport")]
        public void VerifyNoExportPermissionErrorMessage(List<string> obj, List<string> admin, List<string> browser, List<string> tab)
        {
            //Arrange
            string errorMessage = Core.Constants.IntegrationPoints.PermissionErrors.INTEGRATION_POINT_SAVE_FAILURE_USER_MESSAGE;
            string newIntegraionPoint = "//a[@title='New Integration Point']";
            string errorPopup = "notEnoughPermission";
            string errorBar = "//div[contains(@class,'page-message page-error')]";

            string groupName = "Permission Group" + DateTime.Now;
            Regex regex = new Regex("[^a-zA-Z0-9]");
            string tempEmail = regex.Replace(DateTime.Now.ToString(), "") + "test@kcura.com";

            PermissionProperty tempP = new PermissionProperty() { };
            tempP.Admin = admin;
            tempP.Tab = tab;
            tempP.Browser = browser;
            tempP.Obj = obj;

            int groupId = Group.CreateGroup(groupName);
            _groupCreated = groupId;
            Group.AddGroupToWorkspace(SourceWorkspaceArtifactId, groupId);
            Group.AddGroupToWorkspace(TargetWorkspaceArtifactId, groupId);
            Permission.SetPermissions(SourceWorkspaceArtifactId, groupId, tempP);

            UserModel user = User.CreateUser("tester", "tester", tempEmail, new[] { groupId });
            _userCreated = user.ArtifactId;

            //Act
            Selenium.LogIntoRelativity(tempEmail, SharedVariables.RelativityPassword);
            Selenium.GoToWorkspace(SourceWorkspaceArtifactId);
            Selenium.GoToTab("Integration Points");
            Selenium.WebDriver.SwitchTo().Frame("ListTemplateFrame");
            Selenium.WaitUntilXpathVisible(newIntegraionPoint, 10);
            Selenium.WebDriver.FindElement(By.XPath(newIntegraionPoint)).Click();
            Selenium.WebDriver.SwitchTo().Frame("_externalPage");

            Selenium.WaitUntilIdExists("name", 10);
            Selenium.WebDriver.FindElement(By.Id("name")).SendKeys("ip" + DateTime.Now);
            Selenium.SelectFromDropdownList("sourceProvider", "Relativity");
            Selenium.SelectFromDropdownList("destinationRdo", "Document");
            Selenium.WebDriver.FindElement(By.Id("next")).Click();

            Selenium.WaitUntilIdExists("configurationFrame", 10);
            Selenium.WebDriver.SwitchTo().Frame("configurationFrame");
            Selenium.WaitUntilIdExists("workspaceSelector", 10);
            string target = "Error Target Workspace" + " - " + TargetWorkspaceArtifactId;
            Selenium.SelectFromDropdownList("workspaceSelector", target);
            Selenium.SelectFromDropdownList("savedSearchSelector", "All documents");

            Selenium.WebDriver.SwitchTo().DefaultContent();
            Selenium.WebDriver.SwitchTo().Frame("_externalPage");
            Selenium.WebDriver.FindElement(By.Id("next")).Click();

            Selenium.WaitUntilIdExists("fieldMappings", 10);

            string sourceField = "//select[@id=\"source-fields\"]/option[contains(.,'[Object Identifier]')]";
            Selenium.WaitUntilXpathVisible(sourceField, 10);
            Selenium.WebDriver.FindElement(By.XPath(sourceField)).Click();
            Selenium.WebDriver.FindElement(By.Id("add-source-field")).Click();

            string workspaceField = "//select[@id=\"workspace-fields\"]/option[contains(.,'[Object Identifier]')]";
            Selenium.WebDriver.FindElement(By.XPath(workspaceField)).Click();
            Selenium.WebDriver.FindElement(By.Id("add-workspace-field")).Click();

            Selenium.WebDriver.FindElement(By.Id("save")).Click();

            Selenium.WaitUntilXpathExists("//div[contains(@class,'page-message page-error')]", 10);
            Assert.IsTrue(Selenium.PageShouldContain(errorMessage));

        }

        [TearDown]
        public void TearDown()
        {
            User.DeleteUser(_userCreated);
            Group.DeleteGroup(_groupCreated);
            CloseSeleniumBrowser();
        }

    }
}
