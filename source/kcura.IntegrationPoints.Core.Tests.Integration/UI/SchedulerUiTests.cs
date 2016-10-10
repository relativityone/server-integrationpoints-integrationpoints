using System;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Data;
using NUnit.Framework;
using OpenQA.Selenium;

namespace kCura.IntegrationPoints.Core.Tests.Integration.UI
{
	[TestFixture]
	[Ignore("Tests don't work and need fix")]
	public class SchedulerUiTests : RelativityProviderTemplate
	{
		private const int _ADMIN_USER_ID = 9;
		private const string _INTEGRATION_POINTS_TAB = "Integration Points";
		private ISerializer _serializer;
		private IJobHistoryService _jobHistoryService;
		private IJobService _jobService;
		private IWebDriver _webDriver;
		private IObjectTypeRepository _objectTypeRepository;
		private IQueueDBContext _queueContext;
		private IJobManager _jobManager;

		private int _integrationPointArtifactTypeId;
		private long _jobId = 0;
		private string nextButtonId = "next";
		private string enableSchedulerXpath = "//input[@data-bind='checked: enableScheduler']";

		public SchedulerUiTests() : base("IntegrationPoint UI Source", null)
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
		public void VerifyDailySchedulingIsCorrect(TestBrowser browser)
		{
			//Arrange
			_webDriver = Selenium.GetWebDriver(browser);
			string frequencyOptionDropdownId = "frequency";
			GoToCreateAnIntegrationPointPageAndEnableScheduler();

			//act

			_webDriver.SelectFromDropdownList(frequencyOptionDropdownId, "Daily");

			//assert
			VerifySchedulerDateAndTimeArePresented();
		}

		[TestCase(TestBrowser.Chrome)]
		public void VerifyWeeklySchedulingIsCorrect(TestBrowser browser)
		{
			//arrange
			_webDriver = Selenium.GetWebDriver(browser);
			string frequencyOptionDropdownId = "frequency";
			GoToCreateAnIntegrationPointPageAndEnableScheduler();

			//act
			_webDriver.SelectFromDropdownList(frequencyOptionDropdownId, "Weekly");

			//assert
			VerifyWeeklyOptionsArePresented();
			VerifySchedulerDateAndTimeArePresented();
		}

		[TestCase(TestBrowser.Chrome)]
		public void VerifyMonthlySchedulingIsCorrect(TestBrowser browser)
		{
			//arrange
			_webDriver = Selenium.GetWebDriver(browser);
			string frequencyOptionDropdownId = "frequency";
			GoToCreateAnIntegrationPointPageAndEnableScheduler();

			//act
			_webDriver.SelectFromDropdownList(frequencyOptionDropdownId, "Monthly");

			//assert
			VerifyMonthlyOptionsArePresented();
			VerifySchedulerDateAndTimeArePresented();
		}

		[TestCase(TestBrowser.Chrome)]
		public void VerifySchedulerUiIsConsistent(TestBrowser browser)
		{
			//Arrange
			_webDriver = Selenium.GetWebDriver(browser);
			string frequencyOptionDropdownId = "frequency";
			GoToCreateAnIntegrationPointPageAndEnableScheduler();
			_webDriver.WaitUntilElementExists(ElementType.Id, "name", 10);
			_webDriver.FindElement(By.Id("name")).SendKeys("RIP" + DateTime.Now);
			_webDriver.SelectFromDropdownList("sourceProvider", "Relativity");
			_webDriver.SelectFromDropdownList("destinationRdo", "Document");

			_webDriver.SelectFromDropdownList(frequencyOptionDropdownId, "Daily");

			_webDriver.FindElement(By.Id("scheduleRulesStartDate")).SendKeys("08/24/2016");
			_webDriver.FindElement(By.Id("scheduledTime")).SendKeys("12:12");

			//Go to the 2nd page
			_webDriver.WaitUntilElementIsClickable(ElementType.Id, "next", 10);
			_webDriver.FindElement(By.Id("next")).Click();

			//Go back to the 1st page
			_webDriver.SwitchTo().DefaultContent();
			_webDriver.WaitUntilElementExists(ElementType.Id, "_externalPage", 10);
			_webDriver.SwitchTo().Frame("_externalPage");

			_webDriver.WaitUntilElementIsClickable(ElementType.Id, "back", 10);
			_webDriver.FindElement(By.Id("back")).Click();
			_webDriver.SwitchTo().DefaultContent();
			_webDriver.WaitUntilElementExists(ElementType.Id, "_externalPage", 5);
			_webDriver.SwitchTo().Frame("_externalPage");

			VerifySchedulerDateAndTimeArePresented();

			_webDriver.SelectFromDropdownList(frequencyOptionDropdownId, "Weekly");
			VerifyWeeklyOptionsArePresented();

			_webDriver.SelectFromDropdownList(frequencyOptionDropdownId, "Monthly");
			VerifyMonthlyOptionsArePresented();
		}

		private void GoToCreateAnIntegrationPointPageAndEnableScheduler()
		{
			_webDriver.SetFluidStatus(_ADMIN_USER_ID);
			_webDriver.LogIntoRelativity("relativity.admin@kcura.com", SharedVariables.RelativityPassword);
			_webDriver.GoToWorkspace(SourceWorkspaceArtifactId);
			_webDriver.GoToTab(_INTEGRATION_POINTS_TAB);
			_webDriver.ClickNewIntegrationPoint();

			_webDriver.WaitUntilElementIsClickable(ElementType.Id, nextButtonId, 10);
			_webDriver.FindElement(By.XPath(enableSchedulerXpath)).Click();
		}

		private void VerifySchedulerDateAndTimeArePresented()
		{
			Assert.IsTrue(_webDriver.FindElement(By.Id("scheduleRulesStartDate")).Displayed);
			Assert.IsTrue(_webDriver.FindElement(By.Id("scheduleRulesEndDate")).Displayed);
			Assert.IsTrue(_webDriver.FindElement(By.Id("scheduledTime")).Displayed);
			Assert.IsTrue(_webDriver.FindElement(By.Id("timeFormat")).Displayed);
		}

		private void VerifyWeeklyOptionsArePresented()
		{
			string reoccurXpath = "//div[contains(.,'Reoccur:')]";
			string mondayXpath = "//input[@value='Monday']";
			string tuesdayXpath = "//input[@value='Tuesday']";
			string wednesdayXpath = "//input[@value='Wednesday']";
			string thursdayXpath = "//input[@value='Thursday']";
			string fridayXpath = "//input[@value='Friday']";
			string saturdayXpath = "//input[@value='Saturday']";
			string sundayXpath = "//input[@value='Sunday']";

			Assert.IsTrue(_webDriver.FindElement(By.XPath(reoccurXpath)).Displayed);
			Assert.IsTrue(_webDriver.FindElement(By.XPath(mondayXpath)).Displayed);
			Assert.IsTrue(_webDriver.FindElement(By.XPath(tuesdayXpath)).Displayed);
			Assert.IsTrue(_webDriver.FindElement(By.XPath(wednesdayXpath)).Displayed);
			Assert.IsTrue(_webDriver.FindElement(By.XPath(thursdayXpath)).Displayed);
			Assert.IsTrue(_webDriver.FindElement(By.XPath(fridayXpath)).Displayed);
			Assert.IsTrue(_webDriver.FindElement(By.XPath(saturdayXpath)).Displayed);
			Assert.IsTrue(_webDriver.FindElement(By.XPath(sundayXpath)).Displayed);
		}

		private void VerifyMonthlyOptionsArePresented()
		{
			string reoccurXpath = "//div[contains(.,'Reoccur:')]";
			Assert.IsTrue(_webDriver.FindElement(By.XPath(reoccurXpath)).Displayed);
			Assert.IsTrue(_webDriver.FindElement(By.Id("day-select")).Displayed);
			Assert.IsTrue(_webDriver.FindElement(By.Id("dayType")).Displayed);
			Assert.IsTrue(_webDriver.FindElement(By.Id("dayOfMonth")).Displayed);
		}
	}
}