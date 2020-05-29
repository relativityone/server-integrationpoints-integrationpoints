using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.UITests.Common;
using kCura.IntegrationPoints.UITests.Components;
using kCura.IntegrationPoints.UITests.Pages;
using kCura.IntegrationPoints.UITests.Tests.RelativityProvider;
using NUnit.Framework;
using OpenQA.Selenium;
using Relativity.Testing.Identification;

namespace kCura.IntegrationPoints.UITests.Tests.Scheduler
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints.Scheduling]
	[Category(TestCategory.SCHEDULER)]
	public class SchedulerTests : RelativityProviderTestsBase
	{
		private RelativityProviderModel CreateRelativityProviderModel(SchedulerModel schedulerModel)
		{
			RelativityProviderModel model = new RelativityProviderModel(TestContext.CurrentContext.Test.Name)
			{
				Source = RelativityProviderModel.SourceTypeEnum.SavedSearch,
				RelativityInstance = "This Instance",
				DestinationWorkspace = $"{DestinationContext.WorkspaceName} - {DestinationContext.WorkspaceId}",
				CopyNativeFiles = RelativityProviderModel.CopyNativeFilesEnum.No,
				Scheduler = schedulerModel
			};

			return model;
		}

		[IdentifiedTest("a754eb8f-8e86-4fa6-8798-1dc4773cb261")]
		public void Scheduler_ShouldDisplayErrors_WhenFieldsAreEmpty_And_ShouldSuccessfullySave_WhenFieldValuesAreCorrect()
		{
			// Arrange
			string todayDate = DateTime.Today.ToString("MM/dd/yyyy");
			SchedulerModel schedulerModel = new SchedulerModel(enable: true)
			{
				Frequency = SchedulerModel.FrequencyDaily,
				StartDate = todayDate,
				EndDate = todayDate,
				Time = "2:57",
				TimeMeridiem = SchedulerModel.TimeMeridiemAM
			};
			RelativityProviderModel model = CreateRelativityProviderModel(schedulerModel);

			// Act & Assert
			ExportFirstPage firstPage = PointsAction.SetupSyncFirstPage(model);

			firstPage.ClickNext();

			// Assert error messages when fields are empty
			firstPage.GetGeneralErrorLabel().Text.Should().Be("Resolve all errors before proceeding"); // General error on top of the page
			List<IWebElement> errors = firstPage.GetErrorLabels();
			errors.Count(x => x.Text == "This field is required.").Should().Be(2); // Frequency and Scheduled Time
			errors.Count(x => x.Text == "Please enter a valid date.").Should().Be(1); // Start Date
			
			// Assert frequency drop-down has correct values
			IList<string> frequencyOptions = firstPage.SchedulerFrequency.Options.Select(x => x.Text).ToList();
			frequencyOptions.ShouldBeEquivalentTo(new List<string>()
			{
				SchedulerModel.FrequencySelect,
				SchedulerModel.FrequencyDaily,
				SchedulerModel.FrequencyWeekly,
				SchedulerModel.FrequencyMonthly
			});

			// Pick frequency
			firstPage.SchedulerFrequency.SelectByText(schedulerModel.Frequency);

			// Pick start date
			firstPage.SchedulerStartDateTextBox.Click();
			firstPage.IsSchedulerDatePickerVisible.Should().Be(true);
			firstPage.PickSchedulerTodayDate();

			// Pick end date
			firstPage.SchedulerEndDateTextBox.Click();
			firstPage.IsSchedulerDatePickerVisible.Should().Be(true);
			firstPage.PickSchedulerTodayDate();

			// Set time
			firstPage.SetScheduledTime(schedulerModel.Time);
			IList<string> timeMeridiemOptions = firstPage.TimeMeridiem.Options.Select(x => x.Text).ToList();

			timeMeridiemOptions.ShouldAllBeEquivalentTo(new List<string>()
			{
				SchedulerModel.TimeMeridiemAM,
				SchedulerModel.TimeMeridiemPM
			});
			firstPage.TimeMeridiem.SelectByText(schedulerModel.TimeMeridiem);

			// Verify if timezone has been auto selected
			firstPage.TimeZones.SelectedOption.Text.Should().NotBeNullOrWhiteSpace();

			// Proceed and save Integration Point
			PushToRelativitySecondPage secondPage = PointsAction.SetupPushToRelativitySecondPage(firstPage, model);
			PushToRelativityThirdPage thirdPage = PointsAction.SetupPushToRelativityThirdPage(secondPage, model);
			IntegrationPointDetailsPage detailsPage = thirdPage.SaveIntegrationPoint();

			PropertiesTable schedulerProperties = detailsPage.SelectSchedulingPropertiesTable();

			// Verify Scheduling properties on summary page
			schedulerProperties.Properties["Enable Scheduler:"].Should().Be("True");
			schedulerProperties.Properties["Frequency:"].Should().Be(schedulerModel.Frequency);

			schedulerProperties.Properties["Start Date:"].Should().Be(todayDate);
			schedulerProperties.Properties["Start Date:"].Should().Be(todayDate);

			string[] scheduledTime = schedulerProperties.Properties["Scheduled Time:"].Split(';');

			string selectedScheduledTime = scheduledTime[0];
			selectedScheduledTime.Should().Be($"{schedulerModel.Time} {schedulerModel.TimeMeridiem}");

			string selectedTimeZone = scheduledTime[1];
			selectedTimeZone.Should().NotBeNullOrWhiteSpace();
		}
	}
}