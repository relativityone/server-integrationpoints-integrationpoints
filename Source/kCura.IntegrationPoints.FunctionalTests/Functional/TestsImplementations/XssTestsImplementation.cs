using System;
using System.Collections.Generic;
using System.Linq;
using Atata;
using FluentAssertions;
using OpenQA.Selenium;
using Relativity.IntegrationPoints.Tests.Functional.Helpers;
using Relativity.IntegrationPoints.Tests.Functional.Web.Components;
using Relativity.IntegrationPoints.Tests.Functional.Web.Extensions;
using Relativity.IntegrationPoints.Tests.Functional.Web.Models;
using Relativity.IntegrationPoints.Tests.Functional.Web.Models.ExportToLoadFileOutputSettings;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api.Services;
using Relativity.Testing.Framework.Models;
using Relativity.Testing.Framework.Web.Components;
using Relativity.Testing.Framework.Web.Extensions;
using Relativity.Testing.Framework.Web.Navigation;

namespace Relativity.IntegrationPoints.Tests.Functional.TestsImplementations
{
	internal class XssTestsImplementation
	{
		private readonly ITestsImplementationTestFixture _testsImplementationTestFixture;

		private readonly string _savedSearch = nameof(XssTestsImplementation);

		public XssTestsImplementation(ITestsImplementationTestFixture testsImplementationTestFixture)
		{
			_testsImplementationTestFixture = testsImplementationTestFixture;
		}

		public void OnSetUpFixture()
		{
			CreateSavedSearch(_savedSearch);
		}

		public void IntegrationPointSaveAsProfilePreventXssInjection(string profileName)
		{
			// Arrange
			string destinationWorkspaceName = $"Test - {Guid.NewGuid()}";
			Workspace destinationWorkspace = RelativityFacade.Instance.CreateWorkspace(destinationWorkspaceName);

			_testsImplementationTestFixture.LoginAsStandardUser();

			var integrationPointListPage = Being.On<IntegrationPointListPage>(_testsImplementationTestFixture.Workspace.ArtifactID);
			var integrationPointEditPage = integrationPointListPage.NewIntegrationPoint.ClickAndGo();

			const string integrationPointName = nameof(IntegrationPointSaveAsProfilePreventXssInjection);

			var integrationPointViewPage = integrationPointEditPage.CreateSavedSearchToFolderIntegrationPoint(
				integrationPointName, destinationWorkspace, _savedSearch);

			// Act
			integrationPointViewPage
				.SaveAsProfile.ClickAndGo()
				.ApplyModel(new IntegrationPointSaveAsProfile
				{
					ProfileName = profileName
				}).SaveAsProfile.ClickAndGo();

			// Assert
			AssertXss();
		}

		public void IntegrationPointNamePreventXssInjection(string integrationPointName)
		{
			RunFirstPageXssPreventionTestCase<IntegrationPointListPage>(
				p => p.NewIntegrationPoint,
				ExportIntegrationPointEdit(
					integrationPointName));
		}

		public void IntegrationPointEmailNotificationRecipientsPreventXssInjection(string emailRecipients)
		{
			RunFirstPageXssPreventionTestCase<IntegrationPointListPage>(
				p => p.NewIntegrationPoint,
				ExportIntegrationPointEdit(
					emailRecipients: emailRecipients));
		}

		public void IntegrationPointProfileNamePreventXssInjection(string integrationPointProfileName)
		{
			RunFirstPageXssPreventionTestCase<IntegrationPointProfileListPage>(
				p => p.NewIntegrationPointProfile,
				ExportIntegrationPointEdit(
					integrationPointProfileName));
		}

		public void IntegrationPointProfileEmailNotificationRecipientsPreventXssInjection(string emailRecipients)
		{
			RunFirstPageXssPreventionTestCase<IntegrationPointProfileListPage>(
				p => p.NewIntegrationPointProfile,
				ExportIntegrationPointEdit(
					emailRecipients: emailRecipients));
		}

		public void IntegrationPointImportFromLDAPConnectionPathPreventXssInjection(string connectionPath)
		{
			RunLDAPXssPreventionTestCase(
				ImportFromLDAPConnectToSource(
					connectionPath: connectionPath));
		}

		public void IntegrationPointImportFromLDAPUsernamePreventXssInjection(string username)
		{
			RunLDAPXssPreventionTestCase(
				ImportFromLDAPConnectToSource(
					username: username));
		}

		public void IntegrationPointImportFromLDAPPasswordPreventXssInjection(string password)
		{
			RunLDAPXssPreventionTestCase(
				ImportFromLDAPConnectToSource(
					password: password));
		}

		private void RunFirstPageXssPreventionTestCase<T>(Func<T, Button<IntegrationPointEditPage, T>> newButtonFunc, IntegrationPointEdit integrationPointEdit) 
			where T: WorkspacePage<T>, new()
		{
			// Arrange
			_testsImplementationTestFixture.LoginAsStandardUser();

			// Act
			T page = Being.On<T>(_testsImplementationTestFixture.Workspace.ArtifactID);

			newButtonFunc(page).ClickAndGo()
				.ApplyModel(integrationPointEdit)
				.RelativityProviderNext.Click();

			// Assert
			AssertXss();
		}

		private void RunLDAPXssPreventionTestCase(ImportFromLDAPConnectToSource importFromLdapConnectToSource)
		{
			// Arrange
			_testsImplementationTestFixture.LoginAsStandardUser();

			// Act
			Being.On<IntegrationPointListPage>(_testsImplementationTestFixture.Workspace.ArtifactID)
				.NewIntegrationPoint.ClickAndGo()
				.ApplyModel(new IntegrationPointEditImport
				{
					Name = nameof(RunLDAPXssPreventionTestCase),
					Source = IntegrationPointSources.LDAP,
					TransferredObject = IntegrationPointTransferredObjects.Document,
				}).ImportFromLDAPNext.ClickAndGo()
				.ApplyModel(importFromLdapConnectToSource).Next.ClickAndGo();

			// Assert
			AssertXss();
		}

		private void RunExportLoadFileXssPreventionTestCase(
			Action<ExportToLoadFileDestinationInformationPage, ExportToLoadFileOutputSettingsModel> destinationInformationPageAction,
			ExportToLoadFileOutputSettingsModel model)
		{
			// Arrange
			_testsImplementationTestFixture.LoginAsStandardUser();

			// Act
			var exportToLoadFileDestinationInformation =
				Being.On<IntegrationPointListPage>(_testsImplementationTestFixture.Workspace.ArtifactID)
					.NewIntegrationPoint.ClickAndGo()
					.ApplyModel(new IntegrationPointEditExport
					{
						Name = nameof(RunExportLoadFileXssPreventionTestCase),
						Destination = IntegrationPointDestinations.LoadFile
					}).ExportToLoadFileNext.ClickAndGo()
					.ApplyModel(new ExportToLoadFileConnectToSavedSearchSource()
					{
						StartExportAtRecord = 1,
						SavedSearch = _savedSearch
					}).Next.ClickAndGo();

			destinationInformationPageAction(exportToLoadFileDestinationInformation, model);

			exportToLoadFileDestinationInformation
				.Save.Wait(Until.Visible)
				.Save.ClickAndGo();

			// Assert
			AssertXss();
		}

		private static IntegrationPointEdit ExportIntegrationPointEdit(string name = null, string emailRecipients = "")
		{
			return new IntegrationPointEditExport
			{
				Name = name ?? $" XSS Integration Point - {Guid.NewGuid()}",
				Destination = IntegrationPointDestinations.Relativity,
				EmailRecipients = emailRecipients
			};
		}

		private static ImportFromLDAPConnectToSource ImportFromLDAPConnectToSource(
			string connectionPath = null, string username = null, string password = null)
		{
			return new ImportFromLDAPConnectToSource
			{
				ConnectionPath = connectionPath ?? Const.LDAP.OPEN_LDAP_CONNECTION_PATH,
				Username = username ?? Const.LDAP.OPEN_LDAP_USERNAME,
				Password = password ?? Const.LDAP.OPEN_LDAP_PASSWORD
			};
		}

		private static ExportToLoadFileOutputSettingsModel ExportToLoadFileSettingsModel(
			string filePathUserPrefix = null, string imageSubdirectoryPrefix = null,
			string nativeSubdirectoryPrefix = null, string textSubdirectoryPrefix = null,
			string volumePrefix = null)
		{
			return null;
		}

		private void CreateSavedSearch(string name)
		{
			RelativityFacade.Instance.Resolve<IKeywordSearchService>()
				.Require(_testsImplementationTestFixture.Workspace.ArtifactID,
					new KeywordSearch
					{
						Name = name,
						SearchCriteria = new CriteriaCollection
						{
							Conditions = new List<BaseCriteria>()
						}
					});
		}

		private static void AssertXss()
		{
			object scriptResult = AtataContext.Current.Driver.ExecuteScript("return window.relativityXSS === undefined");
			scriptResult.Should().BeAssignableTo<bool>().Which.Should().BeTrue("XSS attack should not execute malicious code");

			string errors = string.Join(Environment.NewLine, AtataContext.Current.Driver
				.Manage()
				.Logs
				.GetLog(LogType.Browser)
				.Where(x => x.Level == OpenQA.Selenium.LogLevel.Severe)
				.Where(x => x.Message.Contains($"/{Const.INTEGRATION_POINTS_APPLICATION_GUID}/"))
				.Select(x => x.Message));
			errors.Should().BeNullOrWhiteSpace("XSS attack should not cause JavaScript errors");
		}
	}
}
