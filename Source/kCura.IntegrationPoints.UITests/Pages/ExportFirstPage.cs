using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
		protected IWebElement DestinationSelectWebElement => Driver.FindElementEx(By.Id("destinationProviderType"));

		protected IWebElement TransferedObjectSelectWebElement => Driver.FindElementEx(By.Id("destinationRdo"));

		protected IWebElement EnableSchedulerButton => Driver.FindElementEx(By.Id("enableSchedulerRadioButton"));

		protected IWebElement DisableSchedulerButton => Driver.FindElementEx(By.Id("disableSchedulerRadioButton"));

		protected IWebElement SchedulerFrequencySelect => Driver.FindElementEx(By.Id("frequency"));

		public IWebElement SchedulerStartDateTextBox => Driver.FindElementEx(By.Id("scheduleRulesStartDate"));
		public IWebElement SchedulerEndDateTextBox => Driver.FindElementEx(By.Id("scheduleRulesEndDate"));

		protected IWebElement SchedulerDatePicker => Driver.FindElementEx(By.Id("ui-datepicker-div"));

		protected IWebElement ScheduledTime => Driver.FindElementEx(By.Id("scheduledTime"));

		protected IWebElement TimeMeridiemSelect => Driver.FindElementEx(By.Id("timeMeridiem"));

		protected IWebElement TimeZonesSelect => Driver.FindElementEx(By.Id("timeZones"));

		protected SelectElement DestinationSelect => new SelectElement(DestinationSelectWebElement);
		protected SelectElement TransferedObjectSelect => new SelectElement(TransferedObjectSelectWebElement);

		public SelectElement SchedulerFrequency => new SelectElement(SchedulerFrequencySelect);
		public SelectElement TimeMeridiem => new SelectElement(TimeMeridiemSelect);
		public SelectElement TimeZones => new SelectElement(TimeZonesSelect);

		public string Destination
		{
			get { return DestinationSelect.SelectedOption.Text; }
			set { DestinationSelect.SelectByTextEx(value, Driver); }
		}

		public string TransferedObject
		{
			get { return TransferedObjectSelect.SelectedOption.Text; }
			set { TransferedObjectSelect.SelectByTextEx(value, Driver); }
		}

		public bool IsSchedulerDatePickerVisible => SchedulerDatePicker.Displayed;

		public void PickSchedulerTodayDate()
		{
			SchedulerDatePicker.FindElementEx(By.CssSelector(".ui-state-default.ui-state-highlight")).Click();
		}

		public void SetScheduledTime(string time)
		{
			ScheduledTime.SetTextEx(time, Driver);
		}

		public List<IWebElement> GetErrorLabels()
		{
			return Driver
				.FindElementsEx(By.CssSelector(".icon-error.legal-hold.field-validation-error"))
				.Where(element => !string.IsNullOrWhiteSpace(element.Text))
				.ToList();
		}

		public IWebElement GetGeneralErrorLabel()
		{
			return Driver.FindElementEx(By.CssSelector(".page-message.page-error"));
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
			driver.SwitchToFrameEx("externalPage");
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
			Thread.Sleep(1000);
			NextButton.ClickEx(Driver);
			WaitForPage();
		}
	}
}
