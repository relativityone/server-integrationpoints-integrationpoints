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
using kCura.ScheduleQueue.Core.Data;
using kCura.ScheduleQueue.Core.Data.Queries;
using NUnit.Framework;
using OpenQA.Selenium;
using System;
using System.Data;

namespace kCura.IntegrationPoints.Core.Tests.UI
{
	[TestFixture]
	[Category(kCura.IntegrationPoint.Tests.Core.Constants.INTEGRATION_CATEGORY)]
	public class ConsoleButtons : RelativityProviderTemplate
	{
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
			IntegrationModel integrationModel = CreateDefaultIntegrationPointModel(ImportOverwriteModeEnum.AppendOnly, "testing", "Append Only");
			IntegrationModel integrationPoint = CreateOrUpdateIntegrationPoint(integrationModel);
			Guid batchInstance = Guid.NewGuid();
			string jobDetails = string.Format(@"{{""BatchInstance"":""{0}"",""BatchParameters"":null}}", batchInstance.ToString());
			JobHistory jobHistory = CreateJobHistoryOnIntegrationPoint(integrationPoint.ArtifactID, batchInstance);
			DataRow row = new CreateScheduledJob(_queueContext).Execute(
				SourceWorkspaceArtifactId,
				integrationPoint.ArtifactID,
				"ExportService",
				DateTime.MaxValue,
				1,
				null,
				null,
				jobDetails,
				0,
				777,
				1,
				1);
			Job tempJob = new Job(row);
			_jobId = tempJob.JobId;

			string runAndStopId = "_dynamicTemplate__kCuraScrollingDiv__dynamicViewFieldRenderer_ctl17_anchor";
			string runAndStopButtonOnClickStopXpath = string.Format(@"//a[@onclick='IP.stopJob({0},{1})']", integrationPoint.ArtifactID, SourceWorkspaceArtifactId);
			string warningDialogId = "ui-dialog-title-msgDiv";
			string consoleControlXpath = "//div[contains(@class,'ConsoleControlTitle')]";


			_webDriver = Selenium.GetWebDriver(browser);

			//Act
			_webDriver.LogIntoRelativity(SharedVariables.RelativityUserName, SharedVariables.RelativityPassword);
			_webDriver.SetFluidStatus(9);
			_webDriver.GoToWorkspace(SourceWorkspaceArtifactId);
			_webDriver.GoToObjectInstance(SourceWorkspaceArtifactId, integrationPoint.ArtifactID, _integrationPointArtifactTypeId);
			_webDriver.WaitUntilElementIsVisible(ElementType.Xpath, consoleControlXpath, 10);
			_webDriver.WaitUntilElementIsClickable(ElementType.Id, runAndStopId, 5);
			_webDriver.WaitUntilElementIsVisible(ElementType.Xpath, runAndStopButtonOnClickStopXpath, 5);
			_webDriver.FindElement(By.Id(runAndStopId)).Click();
			_webDriver.WaitUntilElementExists(ElementType.Id, warningDialogId, 10);
			_webDriver.FindElement(By.XPath(STOP_TRANSFER_BUTTON_XPATH)).Click();
			_webDriver.WaitUntilElementIsVisible(ElementType.Xpath, JOBHISTORY_STATUS_STOPPING_XPATH, 10);

			Job updatedJob = _jobService.GetJob(_jobId);

			//Assert
			Assert.IsTrue(((int)(updatedJob.StopState) == 1));
		}
	}
}