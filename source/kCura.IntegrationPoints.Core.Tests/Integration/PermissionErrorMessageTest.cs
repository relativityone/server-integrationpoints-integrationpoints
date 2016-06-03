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
    public class PermissionErrorMessageTest : WorkspaceDependentTemplate
    {

        private IObjectTypeRepository _objectTypeRepository;

        private int _userCreated;
        private int _groupCreated;
        public PermissionErrorMessageTest()
            : base("Error Source", null)
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
        public void VerifyLdapPermissionErrorMessage(List<string> obj, List<string> admin, List<string> browser, List<string> tab)
        {
            string errorMessage = Core.Constants.IntegrationPoints.PermissionErrors.INSUFFICIENT_PERMISSIONS;
            string jobError = "//div[contains(.,'Failed to submit integration job. You do not have sufficient permissions. Please contact your system administrator.')]";
            string runNowId = "_dynamicTemplate__kCuraScrollingDiv__dynamicViewFieldRenderer_ctl17_anchor";
            string okPath = "//button[contains(.,'OK')]";

            string groupName = "Permission Group" + DateTime.Now;
            Regex regex = new Regex("[^a-zA-Z0-9]");
            string tempEmail = regex.Replace(DateTime.Now.ToString(), "") + "test@kcura.com";

            PermissionProperty tempP = new PermissionProperty(){};
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
                SourceProvider = LdapProvider.ArtifactId,
                Name = "LDAP test" + DateTime.Now,
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

        [TearDown]
        public void TearDown()
        {
            User.DeleteUser(_userCreated);
            Group.DeleteGroup(_groupCreated);
            CloseSeleniumBrowser();
        }

    }
}
