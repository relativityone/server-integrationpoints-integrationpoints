using System;
using System.Data;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Data;
using kCura.ScheduleQueue.Core.Data.Queries;
using NUnit.Framework;
using OpenQA.Selenium;

namespace kCura.IntegrationPoints.Core.Tests.UI
{
	[TestFixture]
	[Category("Integration Tests")]
	public class OtherProvidersConsoleButtonTests : OtherProvidersTemplate
	{
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
				IntegrationModel integrationModel = CreateDefaultLdapIntegrationModel("Ldap_StopButton_SetsStopStateOnClick_Success");
				IntegrationModel integrationPoint = CreateOrUpdateIntegrationPoint(integrationModel);

				Guid batchInstance = Guid.NewGuid();
				CreateJobHistoryOnIntegrationPoint(integrationPoint.ArtifactID, batchInstance);

				DataRow row = new CreateScheduledJob(_queueContext).Execute(
					WorkspaceArtifactId,
					integrationPoint.ArtifactID,
					"SyncManager",
					DateTime.MaxValue,
					1,
					null,
					null,
					null,
					0,
					777,
					1,
					1);
				Job tempJob = new Job(row);
				jobId = tempJob.JobId;

				string runAndStopId = "_dynamicTemplate__kCuraScrollingDiv__dynamicViewFieldRenderer_ctl17_anchor";
				string runAndStopButtonOnClickStopXpath =
					$@"//a[@onclick='IP.stopJob({integrationPoint.ArtifactID},{WorkspaceArtifactId})']";
				string warningDialogId = "ui-dialog-title-msgDiv";

				_webDriver = Selenium.GetWebDriver(browser);

				//Act
				_webDriver.LogIntoRelativity(SharedVariables.RelativityUserName, SharedVariables.RelativityPassword);
				_webDriver.SetFluidStatus(9);
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
				Assert.IsTrue((int)updatedJob.StopState == 1);
			}
			finally
			{
				_jobManager.DeleteJob(jobId);
			}
		}
	}
}
