using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core.Data;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;

namespace kCura.IntegrationPoints.Core.Tests.Integration
{
	[TestFixture]
	[Category("Integration Tests")]
	public class StopWarningMessageTests : RelativityProviderTemplate
	{
		private IObjectTypeRepository _objectTypeRepository;
		private IWebDriver _webDriver;

		private int _userCreated;
		private string _email;
		private int _groupId;
		private IQueueDBContext _queueContext;
		private long _jobId;
		private IJobHistoryService _jobHistoryService;
		private IChoiceQuery _choiceQuery;

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
		}

		public override void TestSetup()
		{
			_webDriver = new ChromeDriver();
			//string groupName = "Permission Group" + DateTime.Now;
			//Regex regex = new Regex("[^a-zA-Z0-9]");
			//_email = regex.Replace(DateTime.Now.ToString(), "") + "test@kcura.com";
			//_groupId = IntegrationPoint.Tests.Core.Group.CreateGroup(groupName);
			//IntegrationPoint.Tests.Core.Group.AddGroupToWorkspace(SourceWorkspaceArtifactId, _groupId);

			//UserModel user = User.CreateUser("tester", "tester", _email, new[] { _groupId });
			//_userCreated = user.ArtifactId;
		}

		public override void TestTeardown()
		{
			_webDriver.CloseSeleniumBrowser();
			//User.DeleteUser(_userCreated);
			//IntegrationPoint.Tests.Core.Group.DeleteGroup(_groupId);
		}

		[Test]
		public void VerifyNoExportPermissionErrorMessageOnRelativityProvider()
		{
			//arrange
			IntegrationModel integrationModel = CreateDefaultIntegrationPointModel(ImportOverwriteModeEnum.AppendOnly, "testing", "Append Only");
			IntegrationModel integrationPoint = CreateOrUpdateIntegrationPoint(integrationModel);
			IList<Relativity.Client.DTOs.Choice> choices = _choiceQuery.GetChoicesOnField(Guid.Parse(IntegrationPointFieldGuids.OverwriteFields));
			Data.IntegrationPoint IP = integrationPoint.ToRdo(choices);

			string jobDetails =
				"{\"Subject\":\"testing\",\"MessageBody\":\"nothing \",\"Emails\":[\"kwu@kcura.com\"]}";
			string scheduleRule = "Rule";
			Guid batchInstance = Guid.NewGuid();
			DateTime dateTime = DateTime.Now.AddDays(30);
			JobHistory jobHistory = _jobHistoryService.CreateRdo(IP, batchInstance, dateTime);

			string runNowId = "_dynamicTemplate__kCuraScrollingDiv__dynamicViewFieldRenderer_ctl17_anchor";
			string okPath = "//button[contains(.,'OK')]";
			string stopButton = "_dynamicTemplate__kCuraScrollingDiv__dynamicViewFieldRenderer_ctl18_anchor";
			string warningMessage = "Stopping this transfer will not remove any data that was transferred. When re-running this transfer, make sure that your overwrite settings will return expected results.";
			string stopDialogId = "ui-dialog-title-msgDiv";
			string stopTransferButtonXpath = "//button[contains(.,'Stop Transfer')]";
			string stopCancelButtonXpath = "//button[contains(.,'Cancel')]";

			_webDriver.LogIntoRelativity("relativity.admin@kcura.com", SharedVariables.RelativityPassword);
			_webDriver.GoToWorkspace(SourceWorkspaceArtifactId);
			int? artifactTypeId = _objectTypeRepository.RetrieveObjectTypeDescriptorArtifactTypeId(new Guid(ObjectTypeGuids.IntegrationPoint));

			_webDriver.GoToObjectInstance(SourceWorkspaceArtifactId, integrationModel.ArtifactID, artifactTypeId.Value);
			Assert.IsFalse(_webDriver.PageShouldContain(warningMessage));

			_webDriver.WaitUntilElementIsClickable(ElementType.Id, stopButton, 10);
			_webDriver.FindElement(By.Id(stopButton)).Click();
			_webDriver.WaitUntilElementExists(ElementType.Id, stopDialogId, 10);
			Assert.IsTrue(_webDriver.PageShouldContain(warningMessage));

			_webDriver.WaitUntilElementIsClickable(ElementType.Xpath, stopTransferButtonXpath, 5);
			_webDriver.WaitUntilElementIsClickable(ElementType.Xpath, stopCancelButtonXpath, 5);

			//DataRow row = new CreateScheduledJob(_queueContext).Execute(
			//	SourceWorkspaceArtifactId,
			//	integrationPoint.ArtifactID,
			//	"ExportService",
			//	dateTime,
			//	1,
			//	null,
			//	null,
			//	jobDetails,
			//	0,
			//	777,
			//	1,
			//	1);

			//Job tempJob = new Job(row);
			//_jobId = tempJob.JobId;
		}
	}
}