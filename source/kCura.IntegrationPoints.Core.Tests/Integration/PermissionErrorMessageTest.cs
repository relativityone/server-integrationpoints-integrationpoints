using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.IE;

namespace kCura.IntegrationPoints.Core.Tests.Integration
{
	[TestFixture]
	[Category("Integration Tests")]
	public class PermissionErrorMessageTest : WorkspaceDependentTemplate
	{
		private IObjectTypeRepository _objectTypeRepository;
		private IWebDriver _webDriver;

		private int _userCreated;
		private int _groupCreated;
		private string _groupName;
		private string _email;
		private int _groupId;

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
			_webDriver = new ChromeDriver();
			_groupName = "Permission Group" + DateTime.Now;
			Regex regex = new Regex("[^a-zA-Z0-9]");
			_email = regex.Replace(DateTime.Now.ToString(), "") + "test@kcura.com";
			_groupId = IntegrationPoint.Tests.Core.Group.CreateGroup(_groupName);
			kCura.IntegrationPoint.Tests.Core.Group.AddGroupToWorkspace(SourceWorkspaceArtifactId, _groupId);

			UserModel user = User.CreateUser("tester", "tester", _email, new[] { _groupId });
			_userCreated = user.ArtifactId;
		}

		private static IEnumerable<object[]> PermissionCase
		{
			get
			{
				yield return new object[] { new List<string> { }, new List<string> { "Allow Import", "Allow Export" }, new List<string> { }, new List<string> { "Documents", "Integration Points" } };
				yield return new object[] { new List<string> { "Document", "Integration Point", "Search" }, new List<string> { }, new List<string> { }, new List<string> { "Documents", "Integration Points" } };
				yield return new object[] { new List<string> { }, new List<string> { }, new List<string> { "Folders", "Advanced & Saved Searches" }, new List<string> { "Documents", "Integration Points" } };
			}
		}

		[Test, TestCaseSource(nameof(PermissionCase))]
		public void VerifyLdapPermissionErrorMessage(List<string> obj, List<string> admin, List<string> browser, List<string> tab)
		{
			string errorMessage = Core.Constants.IntegrationPoints.PermissionErrors.INSUFFICIENT_PERMISSIONS;
			string jobError = "//div[contains(.,'Failed to submit integration job. You do not have sufficient permissions. Please contact your system administrator.')]";
			string runNowId = "_dynamicTemplate__kCuraScrollingDiv__dynamicViewFieldRenderer_ctl17_anchor";
			string okPath = "//button[contains(.,'OK')]";

			PermissionProperty tempP = new PermissionProperty
			{
				Admin = admin,
				Tab = tab,
				Browser = browser,
				Obj = obj
			};

			Permission.SetPermissions(SourceWorkspaceArtifactId, _groupId, tempP);

			_webDriver.LogIntoRelativity(_email, SharedVariables.RelativityPassword);
			_webDriver.GoToWorkspace(SourceWorkspaceArtifactId);

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

			_webDriver.GoToObjectInstance(SourceWorkspaceArtifactId, model.ArtifactID, artifactTypeId.Value);
			Assert.IsTrue(_webDriver.PageShouldContain(errorMessage));
			_webDriver.WaitUntilElementIsClickable(ElementType.Id, runNowId, 10);
			_webDriver.FindElement(By.Id(runNowId)).Click();

			_webDriver.WaitUntilElementExists(ElementType.Xpath, okPath, 10);
			_webDriver.FindElement(By.XPath(okPath)).Click();
			_webDriver.WaitUntilElementExists(ElementType.Xpath, jobError, 10);
		}

		private static object[] PermissionNoImport = new[]
		{
			new object[] { new List<string> {"Document", "Integration Point", "Search"}, new List<string> { }, new List<string> { }, new List<string> { "Documents", "Integration Points"}}
		};

		[Test, TestCaseSource(nameof(PermissionNoImport))]
		public void VerifyNoImportPermissionErrorMessage(List<string> obj, List<string> admin, List<string> browser, List<string> tab)
		{
			//Arrange
			string errorMessage = "You do not have permission to import. Please contact your administrator for the correct permissions.";
			string newIntegraionPoint = "//a[@title='New Integration Point']";
			string errorPopup = "notEnoughPermission";
			string templateFrame = "ListTemplateFrame";
			string externalPage = "_externalPage";

			PermissionProperty tempP = new PermissionProperty
			{
				Admin = admin,
				Tab = tab,
				Browser = browser,
				Obj = obj
			};

			Permission.SetPermissions(SourceWorkspaceArtifactId, _groupId, tempP);

			//Act
			_webDriver.LogIntoRelativity(_email, SharedVariables.RelativityPassword);
			_webDriver.GoToWorkspace(SourceWorkspaceArtifactId);
			_webDriver.GoToTab("Integration Points");
			_webDriver.WaitUntilElementExists(ElementType.Id, templateFrame, 5);
			_webDriver.SwitchTo().Frame(templateFrame);
			_webDriver.WaitUntilElementIsClickable(ElementType.Xpath, newIntegraionPoint, 10);
			_webDriver.FindElement(By.XPath(newIntegraionPoint)).Click();
			_webDriver.WaitUntilElementExists(ElementType.Id, externalPage, 5);
			_webDriver.SwitchTo().Frame(externalPage);

			//Assert
			_webDriver.WaitUntilElementExists(ElementType.Id, errorPopup, 10);
			Assert.IsTrue(_webDriver.PageShouldContain(errorMessage));
		}

		[TearDown]
		public void TearDown()
		{
			_webDriver.CloseSeleniumBrowser();
			User.DeleteUser(_userCreated);
			IntegrationPoint.Tests.Core.Group.DeleteGroup(_groupId);
		}
	}
}