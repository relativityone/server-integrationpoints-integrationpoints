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
    using System.Threading;

    [Explicit]
    public class PermissionErrorMessageTest : WorkspaceDependentTemplate
    {

        private IObjectTypeRepository _objectTypeRepository;
        public PermissionErrorMessageTest()
            : base("Error Source", null)
        {
        }

        [TestFixtureSetUp]
        public override void SetUp()
        {
            base.SetUp();
            ResolveContainer();
        }


        [Test]
        [Explicit]
        public void VerifyPermissionErrorMessage()
        {
            string errorMessage = Core.Constants.IntegrationPoints.PermissionErrors.INSUFFICIENT_PERMISSIONS;
            //string jobError = "//div[contains(.,'Failed to submit integration job. \"You do not have sufficient permissions.Please contact your system administrator.\"')]";
            string jobError = "Failed to submit integration job";
            string runNowId = "_dynamicTemplate__kCuraScrollingDiv__dynamicViewFieldRenderer_ctl17_anchor";
            string okPath = "//button[contains(.,'OK')]";

            string groupName = "Permission Group" + DateTime.Now;
            string userFirstName = "New" + DateTime.Now;
            string userLastName = "Test";
            string tempEmail = "test@kcura.com";

            TimeSpan waitTime = new TimeSpan(0, 0, 0, 10);

            int groupId = Group.CreateGroup(groupName);
            Group.AddGroupToWorkspace(SourceWorkspaceArtifactId, groupId);
            Permission.SetMinimumRelativityProviderPermissions(SourceWorkspaceArtifactId, groupId, admin:false);

            UserModel user = User.CreateUser(userFirstName, userLastName, tempEmail, new[] { groupId });


            Selenium.LogIntoRelativity(tempEmail, SharedVariables.RelativityPassword);
            Selenium.GoToWorkspace(SourceWorkspaceArtifactId);

            IntegrationModel model = new IntegrationModel()
            {
                SourceProvider = LdapProvider.ArtifactId,
                Name = "LDAP test",
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
            Selenium.WaitUntilIdExists(runNowId, 10);
            Selenium.WebDriver.FindElement(By.Id(runNowId)).Click();

            Selenium.WaitUntilXpathExists(okPath, 10);
            Selenium.WebDriver.FindElement(By.XPath(okPath)).Click();

            Thread.Sleep(5000);
            //Selenium.WaitUntilXpathExists(jobError, 10);
            Assert.IsTrue(Selenium.PageShouldContain(jobError));

            // cleanup
            User.DeleteUser(user.ArtifactId);
            Group.DeleteGroup(groupId);

        }

        private void ResolveContainer()
        {
            _objectTypeRepository = Container.Resolve<IObjectTypeRepository>();
        }
        
    }
}
