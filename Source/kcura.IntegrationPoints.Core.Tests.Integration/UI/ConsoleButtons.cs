using System;
using System.Data;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using kCura.ScheduleQueue.Core.Data;
using kCura.ScheduleQueue.Core.Data.Queries;
using NUnit.Framework;
using OpenQA.Selenium;

namespace kCura.IntegrationPoints.Core.Tests.Integration.UI
{
	[TestFixture]
	[Ignore("Tests don't work and need fix")]
	public class ConsoleButtons : RelativityProviderTemplate
	{
		private const int _ADMIN_USER_ID = 9;
		private ISerializer _serializer;
		private IJobHistoryService _jobHistoryService;
		private IJobService _jobService;
		private IWebDriver _webDriver;
		private IObjectTypeRepository _objectTypeRepository;
		private IQueueDBContext _queueContext;
		private IJobManager _jobManager;
		private int _integrationPointArtifactTypeId;
		private long _jobId;

		private const string STOP_TRANSFER_BUTTON_XPATH = "//button[contains(.,'Stop Transfer')]";
		private const string JOBHISTORY_STATUS_STOPPING_XPATH = "//td[contains(.,'Stopping')]";
		private const string STOP_CANCEL_BUTTON_XPATH = "//button[contains(.,'Cancel')]";

		public ConsoleButtons() : base("IntegrationPointService Source", null)
		{
		}

		public override void SuiteSetup()
		{
			base.SuiteSetup();
			_serializer = Container.Resolve<ISerializer>();
			_jobHistoryService = Container.Resolve<IJobHistoryService>();
			_jobService = Container.Resolve<IJobService>();
			_objectTypeRepository = Container.Resolve<IObjectTypeRepository>();
			_queueContext = new QueueDBContext(Helper, GlobalConst.SCHEDULE_AGENT_QUEUE_TABLE_NAME);
			_jobManager = Container.Resolve<IJobManager>();
			_integrationPointArtifactTypeId = _objectTypeRepository.RetrieveObjectTypeDescriptorArtifactTypeId(new Guid(ObjectTypeGuids.IntegrationPoint));
		}

		public override void TestTeardown()
		{
			_webDriver.CloseSeleniumBrowser();
			_jobManager.DeleteJob(_jobId);
		}

		[TestCase(TestBrowser.Chrome)]
		public void StopButton_SetsStopStateOnClick_Success(TestBrowser browser)
		{
			//Arrange
			IntegrationPointModel integrationModel = CreateDefaultIntegrationPointModel(ImportOverwriteModeEnum.AppendOnly, "testing", "Append Only");
			IntegrationPointModel integrationPoint = CreateOrUpdateIntegrationPoint(integrationModel);
			Guid batchInstance = Guid.NewGuid();
			string jobDetails = string.Format(@"{{""BatchInstance"":""{0}"",""BatchParameters"":null}}", batchInstance.ToString());
			JobHistory jobHistory = CreateJobHistoryOnIntegrationPoint(integrationPoint.ArtifactID, batchInstance, JobTypeChoices.JobHistoryRun);

			DataRow row;
		    using (DataTable dataTable = new CreateScheduledJob(_queueContext).Execute(
		        workspaceID: SourceWorkspaceArtifactId,
		        relatedObjectArtifactID: integrationPoint.ArtifactID,
		        taskType: "ExportService",
		        nextRunTime: DateTime.MaxValue,
		        AgentTypeID: 1,
		        scheduleRuleType: null,
		        serializedScheduleRule: null,
		        jobDetails: jobDetails,
		        jobFlags: 0,
		        SubmittedBy: 777,
		        rootJobID: 1,
		        parentJobID: 1))
		    {
		        row = dataTable.Rows[0];
		    }

                Job tempJob = new Job(row);
			_jobId = tempJob.JobId;

			string runAndStopId = "_dynamicTemplate__kCuraScrollingDiv__dynamicViewFieldRenderer_ctl17_anchor";
			string runAndStopButtonOnClickStopXpath =
				$@"//a[@onclick='IP.stopJob({integrationPoint.ArtifactID},{SourceWorkspaceArtifactId})']";
			string warningDialogId = "ui-dialog-title-msgDiv";
			string consoleControlXpath = "//div[contains(@class,'ConsoleControlTitle')]";
			string warningMessage = "Stopping this transfer will not remove any data that was transferred. When re-running this transfer, make sure that your overwrite settings will return expected results.";


			_webDriver = Selenium.GetWebDriver(browser);

			//Act and Assert
			_webDriver.LogIntoRelativity(SharedVariables.RelativityUserName, SharedVariables.RelativityPassword);
			_webDriver.SetFluidStatus(_ADMIN_USER_ID);
			_webDriver.GoToWorkspace(SourceWorkspaceArtifactId);
			_webDriver.GoToObjectInstance(SourceWorkspaceArtifactId, integrationPoint.ArtifactID, _integrationPointArtifactTypeId);
			_webDriver.WaitUntilElementIsVisible(ElementType.Xpath, consoleControlXpath, 10);
			_webDriver.WaitUntilElementIsClickable(ElementType.Id, runAndStopId, 5);
			_webDriver.WaitUntilElementIsVisible(ElementType.Xpath, runAndStopButtonOnClickStopXpath, 5);

			Assert.IsFalse(_webDriver.PageShouldContain(warningMessage));
			_webDriver.FindElement(By.Id(runAndStopId)).Click();
			_webDriver.WaitUntilElementExists(ElementType.Id, warningDialogId, 10);
			Assert.IsTrue(_webDriver.PageShouldContain(warningMessage));

			_webDriver.WaitUntilElementIsClickable(ElementType.Xpath, STOP_TRANSFER_BUTTON_XPATH, 2);
			_webDriver.WaitUntilElementIsClickable(ElementType.Xpath, STOP_CANCEL_BUTTON_XPATH, 2);
			_webDriver.FindElement(By.XPath(STOP_TRANSFER_BUTTON_XPATH)).Click();
			_webDriver.WaitUntilElementIsVisible(ElementType.Xpath, JOBHISTORY_STATUS_STOPPING_XPATH, 10);

			Job updatedJob = _jobService.GetJob(_jobId);

			//Assert
			Assert.IsTrue((int)updatedJob.StopState == (int) StopState.Stopping);
		}

		[TestCase(TestBrowser.Chrome)]
		public void StopButton_ErrorMessageWhenJobIsNotValid(TestBrowser browser)
		{
			//Arrange
			IntegrationPointModel integrationModel = CreateDefaultIntegrationPointModel(ImportOverwriteModeEnum.AppendOnly, "testing", "Append Only");
			IntegrationPointModel integrationPoint = CreateOrUpdateIntegrationPoint(integrationModel);
			Guid batchInstance = Guid.NewGuid();
			string jobDetails = string.Format(@"{{""BatchInstance"":""{0}"",""BatchParameters"":null}}", batchInstance.ToString());
			JobHistory jobHistory = CreateJobHistoryOnIntegrationPoint(integrationPoint.ArtifactID, batchInstance, JobTypeChoices.JobHistoryRun);

			DataRow row;
            using (DataTable dataTable = new CreateScheduledJob(_queueContext).Execute(
                    workspaceID: SourceWorkspaceArtifactId,
                    relatedObjectArtifactID: integrationPoint.ArtifactID,
                    taskType: "ExportService",
                    nextRunTime: DateTime.MaxValue,
                    AgentTypeID: 1,
                    scheduleRuleType: null,
                    serializedScheduleRule: null,
                    jobDetails: jobDetails,
                    jobFlags: 0,
                    SubmittedBy: 777,
                    rootJobID: 1,
                    parentJobID: 1))
            {
                row = dataTable.Rows[0];
            }

                Job tempJob = new Job(row);
			_jobId = tempJob.JobId;

			string runAndStopId = "_dynamicTemplate__kCuraScrollingDiv__dynamicViewFieldRenderer_ctl17_anchor";
			string runAndStopButtonOnClickStopXpath =
				$@"//a[@onclick='IP.stopJob({integrationPoint.ArtifactID},{SourceWorkspaceArtifactId})']";
			string warningDialogId = "ui-dialog-title-msgDiv";
			string consoleControlXpath = "//div[contains(@class,'ConsoleControlTitle')]";
			string errorMessage = "Unable to retrieve job(s) in the queue. Please contact your system administrator.";


			_webDriver = Selenium.GetWebDriver(browser);

			//Act and Assert
			_webDriver.LogIntoRelativity(SharedVariables.RelativityUserName, SharedVariables.RelativityPassword);
			_webDriver.SetFluidStatus(_ADMIN_USER_ID);
			_webDriver.GoToWorkspace(SourceWorkspaceArtifactId);
			_webDriver.GoToObjectInstance(SourceWorkspaceArtifactId, integrationPoint.ArtifactID, _integrationPointArtifactTypeId);
			_webDriver.WaitUntilElementIsVisible(ElementType.Xpath, consoleControlXpath, 10);
			_webDriver.WaitUntilElementIsClickable(ElementType.Id, runAndStopId, 5);
			_webDriver.WaitUntilElementIsVisible(ElementType.Xpath, runAndStopButtonOnClickStopXpath, 5);
			_webDriver.WaitUntilElementExists(ElementType.Id, warningDialogId, 10);

			//Remove the job from the queue table before the stop attempt
			_jobService.DeleteJob(_jobId);
			
			_webDriver.FindElement(By.XPath(STOP_TRANSFER_BUTTON_XPATH)).Click();
			_webDriver.WaitUntilElementIsVisible(ElementType.Xpath, JOBHISTORY_STATUS_STOPPING_XPATH, 10);

			//Assert
			Assert.IsTrue(_webDriver.PageShouldContain(errorMessage));
		}
	}
}