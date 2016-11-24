using System;
using System.Data;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
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
	public class OtherProvidersConsoleButtonTests : OtherProvidersTemplate
	{
		private const int _ADMIN_USER_ID = 9;
		private IJobService _jobService;
		private IWebDriver _webDriver;
		private IObjectTypeRepository _objectTypeRepository;
		private IQueueDBContext _queueContext;
		private IJobManager _jobManager;
		private int _integrationPointArtifactTypeId;

		private const string _STOP_TRANSFER_BUTTON_XPATH = "//button[contains(.,'Stop Transfer')]";
		private const string _JOBHISTORY_STATUS_STOPPING_XPATH = "//td[contains(.,'Stopping')]";

		public OtherProvidersConsoleButtonTests() : base("OtherProvidersConsoleButtonTests")
		{
		}

		public override void SuiteSetup()
		{
			base.SuiteSetup();
			_jobService = Container.Resolve<IJobService>();
			_objectTypeRepository = Container.Resolve<IObjectTypeRepository>();
			_queueContext = new QueueDBContext(Helper, GlobalConst.SCHEDULE_AGENT_QUEUE_TABLE_NAME);
			_jobManager = Container.Resolve<IJobManager>();
			_integrationPointArtifactTypeId = _objectTypeRepository.RetrieveObjectTypeDescriptorArtifactTypeId(new Guid(ObjectTypeGuids.IntegrationPoint));
		}
		
		public override void TestTeardown()
		{
			_webDriver.CloseSeleniumBrowser();
		}

		[TestCase(TestBrowser.Chrome)]
		public void Ldap_StopButton_SetsStopStateOnClick_Success(TestBrowser browser)
		{
			long jobId = 0;
			try
			{
				//Arrange
				IntegrationPointModel integrationModel = CreateDefaultLdapIntegrationModel("Ldap_StopButton_SetsStopStateOnClick_Success");
				IntegrationPointModel integrationPoint = CreateOrUpdateIntegrationPoint(integrationModel);

				Guid batchInstance = Guid.NewGuid();
				string jobDetails = $@"{{""BatchInstance"":""{batchInstance}"",""BatchParameters"":null}}";
				CreateJobHistoryOnIntegrationPoint(integrationPoint.ArtifactID, batchInstance, JobTypeChoices.JobHistoryRun);
				
				DataRow row = new CreateScheduledJob(_queueContext).Execute(
					workspaceID: WorkspaceArtifactId,
					relatedObjectArtifactID: integrationPoint.ArtifactID,
					taskType: "SyncManager",
					nextRunTime: DateTime.MaxValue,
					AgentTypeID: 1,
					scheduleRuleType: null,
					serializedScheduleRule: null,
					jobDetails: jobDetails,
					jobFlags: 0,
					SubmittedBy: 777,
					rootJobID: 1,
					parentJobID: 1);

				Job tempJob = new Job(row);
				jobId = tempJob.JobId;

				string runAndStopId = "_dynamicTemplate__kCuraScrollingDiv__dynamicViewFieldRenderer_ctl17_anchor";
				string runAndStopButtonOnClickStopXpath =
					$@"//a[@onclick='IP.stopJob({integrationPoint.ArtifactID},{WorkspaceArtifactId})']";
				string warningDialogId = "ui-dialog-title-msgDiv";

				_webDriver = Selenium.GetWebDriver(browser);
				_webDriver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(60));

				//Act
				_webDriver.LogIntoRelativity(SharedVariables.RelativityUserName, SharedVariables.RelativityPassword);
				_webDriver.SetFluidStatus(_ADMIN_USER_ID);
				_webDriver.GoToWorkspace(WorkspaceArtifactId);
				_webDriver.GoToObjectInstance(WorkspaceArtifactId, integrationPoint.ArtifactID, _integrationPointArtifactTypeId);
				_webDriver.WaitUntilElementIsClickable(ElementType.Id, runAndStopId, 60);
				_webDriver.WaitUntilElementIsVisible(ElementType.Xpath, runAndStopButtonOnClickStopXpath, 60);
				_webDriver.FindElement(By.Id(runAndStopId)).Click();
				_webDriver.WaitUntilElementExists(ElementType.Id, warningDialogId, 60);
				_webDriver.FindElement(By.XPath(_STOP_TRANSFER_BUTTON_XPATH)).Click();
				_webDriver.WaitUntilElementIsVisible(ElementType.Xpath, _JOBHISTORY_STATUS_STOPPING_XPATH, 60);

				Job updatedJob = _jobService.GetJob(jobId);

				//Assert
				Assert.IsTrue((int) updatedJob.StopState == (int) StopState.Stopping);
			}
			finally
			{
				_jobManager.DeleteJob(jobId);
			}
		}
	}
}
