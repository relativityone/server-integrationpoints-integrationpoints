using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.UITests.Driver;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using SeleniumExtras.PageObjects;
using OpenQA.Selenium.Support.UI;
using kCura.IntegrationPoints.UITests.Common;

namespace kCura.IntegrationPoints.UITests.Pages
{
	public class ExportFirstPage : FirstPage
	{
		[FindsBy(How = How.Id, Using = "destinationProviderType")]
		protected IWebElement DestinationSelectWebElement { get; set; }

		[FindsBy(How = How.Id, Using = "destinationRdo")]
		protected IWebElement TransferedObjectSelectWebElement { get; set; }

		[FindsBy(How = How.Id, Using = "enableSchedulerRadioButton")]
		protected IWebElement EnableSchedulerButton { get; set; }

		[FindsBy(How = How.Id, Using = "disableSchedulerRadioButton")]
		protected IWebElement DisableSchedulerButton { get; set; }

		[FindsBy(How = How.Id, Using = "frequency")]
		protected IWebElement SchedulerFrequencySelect { get; set; }

		[FindsBy(How = How.Id, Using = "scheduleRulesStartDate")]
		public IWebElement SchedulerStartDateTextBox { get; set; }

		[FindsBy(How = How.Id, Using = "scheduleRulesEndDate")]
		public IWebElement SchedulerEndDateTextBox { get; set; }

		[FindsBy(How = How.Id, Using = "ui-datepicker-div")]
		protected IWebElement SchedulerDatePicker { get; set; }

		[FindsBy(How = How.Id, Using = "scheduledTime")]
		protected IWebElement ScheduledTime { get; set; }

		[FindsBy(How = How.Id, Using = "timeMeridiem")]
		protected IWebElement TimeMeridiemSelect { get; set; }

		[FindsBy(How = How.Id, Using = "timeZones")]
		protected IWebElement TimeZonesSelect { get; set; }

		protected SelectElement DestinationSelect => new SelectElement(DestinationSelectWebElement);
		protected SelectElement TransferedObjectSelect => new SelectElement(TransferedObjectSelectWebElement);

		public SelectElement SchedulerFrequency => new SelectElement(SchedulerFrequencySelect);
		public SelectElement TimeMeridiem => new SelectElement(TimeMeridiemSelect);
		public SelectElement TimeZones => new SelectElement(TimeZonesSelect);

		public string Destination
		{
			get { return DestinationSelect.SelectedOption.Text; }
			set { DestinationSelect.SelectByText(value); }
		}

		public string TransferedObject
		{
			get { return TransferedObjectSelect.SelectedOption.Text; }
			set { TransferedObjectSelect.SelectByText(value); }
		}

		public bool IsSchedulerDatePickerVisible => SchedulerDatePicker.Displayed;

		public void PickSchedulerTodayDate()
		{
			SchedulerDatePicker.FindElement(By.CssSelector(".ui-state-default.ui-state-highlight")).Click();
		}

		public void SetScheduledTime(string time)
		{
			ScheduledTime.SetText(time);
		}

		public List<IWebElement> GetErrorLabels()
		{
			return Driver
				.FindElementsByCssSelector(".icon-error.legal-hold.field-validation-error")
				.Where(element => !string.IsNullOrWhiteSpace(element.Text))
				.ToList();
		}

		public IWebElement GetGeneralErrorLabel()
		{
			return Driver.FindElementByCssSelector(".page-message.page-error");
		}

		public void ToggleScheduler(bool enable)
		{
			if (enable)
			{
				EnableSchedulerButton.Click();
			}
			else
			{
				DisableSchedulerButton.Click();
			}
		}

		public ExportFirstPage(RemoteWebDriver driver) : base(driver)
		{
			PageFactory.InitElements(driver, this);
			Driver.SwitchTo().Frame("externalPage");
			WaitForPage();
		}

		public bool IsEntityTransferredObjectOptionAvailable() =>
			CustodianToEntityUtils.IsEntityOptionAvailable(TransferedObjectSelect);
		
		public ExportToFileSecondPage GoToNextPage()
		{
			ClickNext();
			return new ExportToFileSecondPage(Driver);
		}

		public PushToRelativitySecondPage GoToNextPagePush()
		{
			ClickNext();
			return new PushToRelativitySecondPage(Driver);
		}

		public ExportEntityToFileSecondPage GotoNextPageEntity()
		{
			ClickNext();
			return new ExportEntityToFileSecondPage(Driver);
		}

		public void ClickNext()
		{
			NextButton.ClickEx();
		}
	}
}
