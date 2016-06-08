using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace kCura.IntegrationPoints.Core.Tests.Integration
{
	[TestFixture]
	[Explicit]
	public class PermissionErrorMessageForRelativityProviderTest : WorkspaceDependentTemplate
	{
		private IObjectTypeRepository _objectTypeRepository;
		private IWebDriver _webDriver;

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
			_webDriver = new ChromeDriver();
		}

		private static object[] PermissionCase = new[]
											 {
												 new object[] { new List<string> {}, new List<string> {"Allow Import", "Allow Export"}, new List<string> {}, new List<string> {"Documents", "Integration Points"}},
												 new object[] { new List<string> {"Document", "Integration Point", "Search"}, new List<string> {}, new List<string> {}, new List<string> { "Documents", "Integration Points"}},
												 new object[] { new List<string> {}, new List<string> {}, new List<string> {"Folders", "Advanced & Saved Searches"}, new List<string> {"Documents", "Integration Points"} }
											 };

		[Test, TestCaseSource("PermissionCase")]
		public void VerifyRelativityPermissionErrorMessage(List<string> obj, List<string> admin, List<string> browser, List<string> tab)
		{
			//Arrange
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

			int groupId = kCura.IntegrationPoint.Tests.Core.Group.CreateGroup(groupName);
			_groupCreated = groupId;
			kCura.IntegrationPoint.Tests.Core.Group.AddGroupToWorkspace(SourceWorkspaceArtifactId, groupId);
			Permission.SetPermissions(SourceWorkspaceArtifactId, groupId, tempP);

			UserModel user = User.CreateUser("tester", "tester", tempEmail, new[] { groupId });
			_userCreated = user.ArtifactId;

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

			//Act
			_webDriver.LogIntoRelativity(tempEmail, SharedVariables.RelativityPassword);
			_webDriver.GoToWorkspace(SourceWorkspaceArtifactId);

			_webDriver.GoToObjectInstance(SourceWorkspaceArtifactId, model.ArtifactID, artifactTypeId.Value);

			//Assert
			Assert.IsTrue(_webDriver.PageShouldContain(errorMessage));

			_webDriver.WaitUntilElementIsClickable(ElementType.Id, runNowId, 10);
			_webDriver.FindElement(By.Id(runNowId)).Click();

			_webDriver.WaitUntilElementExists(ElementType.Xpath, okPath, 10);
			_webDriver.FindElement(By.XPath(okPath)).Click();
			_webDriver.WaitUntilElementExists(ElementType.Xpath, jobError, 10);
		}

		private static object[] PermissionNoExport = new[]
											 {
												 new object[] {
													 new List<string> {"Document", "Integration Point", "Search"},
													 new List<string> {"Allow Import"},
													 new List<string> {"Folders", "Advanced & Saved Searches"},
													 new List<string> { "Documents", "Integration Points"}}
											 };

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

			int groupId = kCura.IntegrationPoint.Tests.Core.Group.CreateGroup(groupName);
			_groupCreated = groupId;
			kCura.IntegrationPoint.Tests.Core.Group.AddGroupToWorkspace(SourceWorkspaceArtifactId, groupId);
			kCura.IntegrationPoint.Tests.Core.Group.AddGroupToWorkspace(TargetWorkspaceArtifactId, groupId);
			Permission.SetPermissions(SourceWorkspaceArtifactId, groupId, tempP);

			UserModel user = User.CreateUser("tester", "tester", tempEmail, new[] { groupId });
			_userCreated = user.ArtifactId;

			//Act
			_webDriver.LogIntoRelativity(tempEmail, SharedVariables.RelativityPassword);
			_webDriver.GoToWorkspace(SourceWorkspaceArtifactId);
			_webDriver.GoToTab("Integration Points");
			_webDriver.SwitchTo().Frame("ListTemplateFrame");
			_webDriver.WaitUntilElementIsVisible(ElementType.Xpath, newIntegraionPoint, 10);
			_webDriver.FindElement(By.XPath(newIntegraionPoint)).Click();
			_webDriver.SwitchTo().Frame("_externalPage");

			_webDriver.WaitUntilElementExists(ElementType.Id, "name", 10);
			_webDriver.FindElement(By.Id("name")).SendKeys("RIP" + DateTime.Now);
			_webDriver.SelectFromDropdownList("sourceProvider", "Relativity");
			_webDriver.SelectFromDropdownList("destinationRdo", "Document");
			_webDriver.FindElement(By.Id("next")).Click();

			_webDriver.WaitUntilElementExists(ElementType.Id, "configurationFrame", 10);
			_webDriver.SwitchTo().Frame("configurationFrame");
			_webDriver.WaitUntilElementExists(ElementType.Id, "workspaceSelector", 10);
			string target = "Error Target Workspace" + " - " + TargetWorkspaceArtifactId;
			_webDriver.SelectFromDropdownList("workspaceSelector", target);
			_webDriver.SelectFromDropdownList("savedSearchSelector", "All documents");

			_webDriver.SwitchTo().DefaultContent();
			_webDriver.SwitchTo().Frame("_externalPage");
			_webDriver.FindElement(By.Id("next")).Click();

			_webDriver.WaitUntilIdExists("fieldMappings", 10);
			string sourceField = "//select[@id=\"source-fields\"]/option[contains(.,'[Object Identifier]')]";
			_webDriver.WaitUntilElementIsVisible(ElementType.Xpath, sourceField, 10);
			_webDriver.FindElement(By.XPath(sourceField)).Click();
			_webDriver.FindElement(By.Id("add-source-field")).Click();

			string workspaceField = "//select[@id=\"workspace-fields\"]/option[contains(.,'[Object Identifier]')]";
			_webDriver.FindElement(By.XPath(workspaceField)).Click();
			_webDriver.FindElement(By.Id("add-workspace-field")).Click();

			_webDriver.FindElement(By.Id("save")).Click();

			_webDriver.WaitUntilElementExists(ElementType.Xpath, "//div[contains(@class,'page-message page-error')]", 10);
			Assert.IsTrue(_webDriver.PageShouldContain(errorMessage));
		}

		[TearDown]
		public void TearDown()
		{
			User.DeleteUser(_userCreated);
			kCura.IntegrationPoint.Tests.Core.Group.DeleteGroup(_groupCreated);
			_webDriver.CloseSeleniumBrowser();
		}
	}
}