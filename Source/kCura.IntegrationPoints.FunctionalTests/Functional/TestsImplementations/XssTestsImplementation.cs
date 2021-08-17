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

		public XssTestsImplementation(ITestsImplementationTestFixture testsImplementationTestFixture)
		{
			_testsImplementationTestFixture = testsImplementationTestFixture;
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

			var keywordSearch = CreateSavedSearch(nameof(IntegrationPointSaveAsProfilePreventXssInjection));

			var integrationPointViewPage = integrationPointEditPage.CreateSavedSearchToFolderIntegrationPoint(
				integrationPointName, destinationWorkspace, keywordSearch);

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
			RunXssPreventionTestCase<IntegrationPointListPage>(
				p => p.NewIntegrationPoint,
				ExportIntegrationPointEdit(
					integrationPointName));
		}

		public void IntegrationPointEmailNotificationRecipientsPreventXssInjection(string emailRecipients)
		{
			RunXssPreventionTestCase<IntegrationPointListPage>(
				p => p.NewIntegrationPoint,
				ExportIntegrationPointEdit(
					emailRecipients: emailRecipients));
		}

		public void IntegrationPointProfileNamePreventXssInjection(string integrationPointProfileName)
		{
			RunXssPreventionTestCase<IntegrationPointProfileListPage>(
				p => p.NewIntegrationPointProfile,
				ExportIntegrationPointEdit(
					integrationPointProfileName));
		}

		public void IntegrationPointProfileEmailNotificationRecipientsPreventXssInjection(string emailRecipients)
		{
			RunXssPreventionTestCase<IntegrationPointProfileListPage>(
				p => p.NewIntegrationPointProfile,
				ExportIntegrationPointEdit(
					emailRecipients: emailRecipients));
		}

		private void RunXssPreventionTestCase<T>(Func<T, Button<IntegrationPointEditPage, T>> newButtonFunc, IntegrationPointEdit integrationPointEdit) 
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

		private IntegrationPointEdit ExportIntegrationPointEdit(string name = null, string emailRecipients = "")
		{
			return new IntegrationPointEdit
			{
				Name = name ?? $" XSS Integration Point - {Guid.NewGuid()}",
				Type = IntegrationPointTypes.Export,
				Destination = IntegrationPointDestinations.Relativity,
				EmailRecipients = emailRecipients
			};
		}

		private KeywordSearch CreateSavedSearch(string name)
		{
			return RelativityFacade.Instance.Resolve<IKeywordSearchService>()
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

		private void AssertXss()
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
			errors.Should().BeNullOrWhiteSpace($"XSS attack should not cause JavaScript errors");
		}
	}
}
