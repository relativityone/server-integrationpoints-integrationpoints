using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Data;
using kCura.ScheduleQueue.Core.Data.Queries;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Data;

namespace kCura.IntegrationPoints.Core.Tests.Integration
{
	[TestFixture]
	[Category(kCura.IntegrationPoint.Tests.Core.Constants.INTEGRATION_CATEGORY)]
	public class StopWarningMessageTests : RelativityProviderTemplate
	{
		private IObjectTypeRepository _objectTypeRepository;
		private IWebDriver _webDriver;
		private IQueueDBContext _queueContext;
		private IJobHistoryService _jobHistoryService;
		private IChoiceQuery _choiceQuery;
		private IJobManager _jobManager;

		private long _jobId;
		private const string STOP_TRANSFER_BUTTON_XPATH = "//button[contains(.,'Stop Transfer')]";
		private const string STOP_CANCEL_BUTTON_XPATH = "//button[contains(.,'Cancel')]";

		public StopWarningMessageTests() : base("MSG Source Workspace", null)
		{
		}

		public override void SuiteSetup()
		{
			InstanceSetting.UpsertAndReturnOldValueIfExists("Relativity.Authentication", "AdminsCanSetPasswords", "True");
			_objectTypeRepository = Container.Resolve<IObjectTypeRepository>();
			_jobHistoryService = Container.Resolve<IJobHistoryService>();
			_queueContext = new QueueDBContext(Helper, GlobalConst.SCHEDULE_AGENT_QUEUE_TABLE_NAME);
			_choiceQuery = Container.Resolve<IChoiceQuery>();
			_jobManager = Container.Resolve<IJobManager>();
		}

		public override void TestSetup()
		{
			_webDriver = new ChromeDriver();
		}

		public override void TestTeardown()
		{
			_webDriver.CloseSeleniumBrowser();
			_jobManager.DeleteJob(_jobId);
		}

		[Test]
		public void VerifyStopWarningMessageShowsUp()
		{
			//arrange
			IntegrationModel integrationModel = CreateDefaultIntegrationPointModel(ImportOverwriteModeEnum.AppendOnly, "testing", "Append Only");
			IntegrationModel integrationPoint = CreateOrUpdateIntegrationPoint(integrationModel);
			IList<Relativity.Client.DTOs.Choice> choices = _choiceQuery.GetChoicesOnField(Guid.Parse(IntegrationPointFieldGuids.OverwriteFields));
			Data.IntegrationPoint IP = integrationPoint.ToRdo(choices);

			string jobDetails =
				"{\"Subject\":\"testing\",\"MessageBody\":\"nothing \",\"Emails\":[\"kwu@kcura.com\"]}";
			Guid batchInstance = Guid.NewGuid();
			DateTime dateTime = DateTime.Now.AddDays(30);
			JobHistory jobHistory = _jobHistoryService.GetOrCreateSchduleRunHistoryRdo(IP, batchInstance, dateTime);
			DataRow row = new CreateScheduledJob(_queueContext).Execute(
				SourceWorkspaceArtifactId,
				integrationPoint.ArtifactID,
				"ExportService",
				dateTime,
				1,
				null,
				null,
				jobDetails,
				0,
				777,
				1);
			Job tempJob = new Job(row);
			_jobId = tempJob.JobId;

			string runAndStopId = "_dynamicTemplate__kCuraScrollingDiv__dynamicViewFieldRenderer_ctl17_anchor";
			string runAndStopButtonOnClickStopXpath = string.Format(@"//a[@onclick='IP.stopJob({0},{1})']", integrationPoint.ArtifactID, SourceWorkspaceArtifactId);
			string warningMessage = "Stopping this transfer will not remove any data that was transferred. When re-running this transfer, make sure that your overwrite settings will return expected results.";
			string warningDialogId = "ui-dialog-title-msgDiv";

			_webDriver.SetFluidStatus(9);
			_webDriver.LogIntoRelativity("relativity.admin@kcura.com", SharedVariables.RelativityPassword);
			_webDriver.GoToWorkspace(SourceWorkspaceArtifactId);
			int? artifactTypeId = _objectTypeRepository.RetrieveObjectTypeDescriptorArtifactTypeId(new Guid(ObjectTypeGuids.IntegrationPoint));

			_webDriver.GoToObjectInstance(SourceWorkspaceArtifactId, integrationPoint.ArtifactID, artifactTypeId.Value);
			Assert.IsFalse(_webDriver.PageShouldContain(warningMessage));

			_webDriver.WaitUntilElementIsClickable(ElementType.Id, runAndStopId, 10);
			_webDriver.WaitUntilElementIsVisible(ElementType.Xpath, runAndStopButtonOnClickStopXpath, 5);
			_webDriver.FindElement(By.Id(runAndStopId)).Click();
			_webDriver.WaitUntilElementExists(ElementType.Id, warningDialogId, 10);
			Assert.IsTrue(_webDriver.PageShouldContain(warningMessage));

			_webDriver.WaitUntilElementIsClickable(ElementType.Xpath, STOP_TRANSFER_BUTTON_XPATH, 2);
			_webDriver.WaitUntilElementIsClickable(ElementType.Xpath, STOP_CANCEL_BUTTON_XPATH, 2);

			string unstoppableJobMessage = "The transfer cannot be stopped at this point in the process";
			_webDriver.FindElement(By.XPath(STOP_TRANSFER_BUTTON_XPATH)).Click();
			_webDriver.WaitUntilElementExists(ElementType.Id, warningDialogId, 10);
			Assert.IsTrue(_webDriver.PageShouldContain(unstoppableJobMessage));
		}
	}
}