﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Synchronizers.RDO;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace kCura.IntegrationPoints.Core.Tests.Integration
{
	[TestFixture]
	[Category(kCura.IntegrationPoint.Tests.Core.Constants.INTEGRATION_CATEGORY)]
	public class PermissionErrorMessageTest : RelativityProviderTemplate
	{
		private IObjectTypeRepository _objectTypeRepository;
		private IWebDriver _webDriver;

		private int _userCreated;
		private string _email;
		private int _groupId;

		public PermissionErrorMessageTest() : base("Error Source", "Error Target Workspace")
		{
		}

		public override void SuiteSetup()
		{
			InstanceSetting.UpdateAndReturnOldValue("Relativity.Authentication", "AdminsCanSetPasswords", "True");
			_objectTypeRepository = Container.Resolve<IObjectTypeRepository>();
		}

		public override void TestSetup()
		{
			_webDriver = new ChromeDriver();
			string groupName = "Permission Group" + DateTime.Now;
			Regex regex = new Regex("[^a-zA-Z0-9]");
			_email = regex.Replace(DateTime.Now.ToString(), "") + "test@kcura.com";
			_groupId = IntegrationPoint.Tests.Core.Group.CreateGroup(groupName);
			IntegrationPoint.Tests.Core.Group.AddGroupToWorkspace(SourceWorkspaceArtifactId, _groupId);

			UserModel user = User.CreateUser("tester", "tester", _email, new[] { _groupId });
			_userCreated = user.ArtifactId;
		}

		public override void TestTeardown()
		{
			_webDriver.CloseSeleniumBrowser();
			User.DeleteUser(_userCreated);
			IntegrationPoint.Tests.Core.Group.DeleteGroup(_groupId);
		}

		private static IEnumerable<object[]> PermissionCase
		{
			get
			{
				yield return new object[] { true, new List<string> { }, new List<string> { "Allow Import", "Allow Export" }, new List<string> { }, new List<string> { "Documents", "Integration Points" } };
				yield return new object[] { true, new List<string> { "Document", "Integration Point", "Search" }, new List<string> { }, new List<string> { }, new List<string> { "Documents", "Integration Points" } };
				yield return new object[] { true, new List<string> { }, new List<string> { }, new List<string> { "Folders", "Advanced & Saved Searches" }, new List<string> { "Documents", "Integration Points" } };
				yield return new object[] { false, new List<string> { }, new List<string> { "Allow Import", "Allow Export" }, new List<string> { }, new List<string> { "Documents", "Integration Points" } };
				yield return new object[] { false, new List<string> { "Document", "Integration Point", "Search" }, new List<string> { }, new List<string> { }, new List<string> { "Documents", "Integration Points" } };
				yield return new object[] { false, new List<string> { }, new List<string> { }, new List<string> { "Folders", "Advanced & Saved Searches" }, new List<string> { "Documents", "Integration Points" } };
			}
		}

		[Test, TestCaseSource(nameof(PermissionCase))]
		public void VerifyPermissionErrorMessage(bool useRelativityProvider, List<string> obj, List<string> admin, List<string> browser, List<string> tab)
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
				//todo: fix this!
				//SourceProvider = useRelativityProvider ? RelativityProvider.ArtifactId : LdapProvider.ArtifactId,
				SourceProvider = RelativityProvider.ArtifactId,
				Name = ( useRelativityProvider ? "RIP test"  : "LDAP test" ) + DateTime.Now,
				DestinationProvider = DestinationProvider.ArtifactId,
				SourceConfiguration = CreateDefaultSourceConfig(),
				Destination = CreateDestinationConfig(ImportOverwriteModeEnum.AppendOnly),
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

			_webDriver.WaitUntilElementIsClickable(ElementType.Xpath, okPath, 15);
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

		private static object[] PermissionNoExport = new[]
		{
			new object[] {
				new List<string> {"Document", "Integration Point", "Search"},
				new List<string> {"Allow Import"},
				new List<string> {"Folders", "Advanced & Saved Searches"},
				new List<string> { "Documents", "Integration Points"}}
		};

		[Test, TestCaseSource(nameof(PermissionNoExport))]
		public void VerifyNoExportPermissionErrorMessageOnRelativityProvider(List<string> obj, List<string> admin, List<string> browser, List<string> tab)
		{
			//Arrange
			string errorMessage = Constants.IntegrationPoints.PermissionErrors.INTEGRATION_POINT_SAVE_FAILURE_USER_MESSAGE;
			string newIntegraionPoint = "//a[@title='New Integration Point']";
			string templateFrame = "ListTemplateFrame";
			string externalPage = "_externalPage";
			string next = "next";
			string addSourceField = "add-source-field";

			PermissionProperty tempP = new PermissionProperty
			{
				Admin = admin,
				Tab = tab,
				Browser = browser,
				Obj = obj
			};

			IntegrationPoint.Tests.Core.Group.AddGroupToWorkspace(TargetWorkspaceArtifactId, _groupId);
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

			_webDriver.WaitUntilElementExists(ElementType.Id, "name", 10);
			_webDriver.FindElement(By.Id("name")).SendKeys("RIP" + DateTime.Now);
			_webDriver.SelectFromDropdownList("sourceProvider", "Relativity");
			_webDriver.SelectFromDropdownList("destinationRdo", "Document");

			_webDriver.WaitUntilElementIsClickable(ElementType.Id, "next", 10);
			_webDriver.FindElement(By.Id("next")).Click();

			_webDriver.WaitUntilElementExists(ElementType.Id, "configurationFrame", 10);
			_webDriver.SwitchTo().Frame("configurationFrame");

			_webDriver.WaitUntilElementExists(ElementType.Id, "workspaceSelector", 10);
			string target = "Error Target Workspace" + " - " + TargetWorkspaceArtifactId;

			_webDriver.SelectFromDropdownList("workspaceSelector", target);
			_webDriver.SelectFromDropdownList("savedSearchSelector", "All documents");

			_webDriver.SwitchTo().DefaultContent();

			_webDriver.WaitUntilElementExists(ElementType.Id, externalPage, 5);
			_webDriver.SwitchTo().Frame(externalPage);

			_webDriver.WaitUntilElementIsClickable(ElementType.Id, next, 5);
			_webDriver.FindElement(By.Id(next)).Click();

			_webDriver.WaitUntilIdExists("fieldMappings", 10);
			string sourceField = "//select[@id=\"source-fields\"]/option[contains(.,'[Object Identifier]')]";
			_webDriver.WaitUntilElementIsClickable(ElementType.Xpath, sourceField, 30); // loading field mappings may takes time
			_webDriver.FindElement(By.XPath(sourceField)).Click();

			_webDriver.WaitUntilElementIsClickable(ElementType.Id, addSourceField, 5);
			_webDriver.FindElement(By.Id(addSourceField)).Click();

			string workspaceField = "//select[@id=\"workspace-fields\"]/option[contains(.,'[Object Identifier]')]";
			_webDriver.WaitUntilElementIsClickable(ElementType.Xpath, workspaceField, 5);
			_webDriver.FindElement(By.XPath(workspaceField)).Click();

			_webDriver.WaitUntilElementIsClickable(ElementType.Id, "add-workspace-field", 5);
			_webDriver.FindElement(By.Id("add-workspace-field")).Click();

			_webDriver.WaitUntilElementIsClickable(ElementType.Id, "save", 5);
			_webDriver.FindElement(By.Id("save")).Click();

			_webDriver.WaitUntilElementExists(ElementType.Xpath, "//div[contains(@class,'page-message page-error')]", 10);
			Assert.IsTrue(_webDriver.PageShouldContain(errorMessage));
		}
	}
}