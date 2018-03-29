using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;

namespace kCura.IntegrationPoints.UITests.Pages
{
	public class ImportThirdPage : GeneralPage
	{
		[FindsBy(How = How.Id, Using = "source-fields")]
		protected IWebElement SourceFieldsElement { get; set; }

		protected SelectElement SelectSourceFieldsElement => new SelectElement(SourceFieldsElement);

		[FindsBy(How = How.Id, Using = "add-source-field")]
		protected IWebElement AddSourceFieldElement { get; set; }

		[FindsBy(How = How.Id, Using = "add-all-source-fields")]
		protected IWebElement AddAllSourceFieldsElement { get; set; }

		[FindsBy(How = How.Id, Using = "workspace-fields")]
		protected IWebElement DestinationFieldElement { get; set; }

		protected SelectElement SelectDestinationFieldElement => new SelectElement(DestinationFieldElement);

		[FindsBy(How = How.Id, Using = "add-workspace-field")]
		protected IWebElement AddDestinationFieldElement { get; set; }

		[FindsBy(How = How.Id, Using = "add-all-workspace-fields")]
		protected IWebElement AddAllDestinationFieldsElement { get; set; }

		[FindsBy(How = How.Id, Using = "mapFieldsBtn")]
		protected IWebElement MapFieldsElement { get; set; }

		[FindsBy(How = How.Id, Using = "s2id_overwrite")]
		protected IWebElement OverwriteSelectWebElement { get; set; }

		protected SelectElement OverwriteSelectElement => new SelectElement(OverwriteSelectWebElement);

		[FindsBy(How = How.Id, Using = "s2id_overwrite")]
		protected IWebElement UniqueIdentifierSelectWebElement { get; set; }

		protected SelectElement UniqueIdentifierSelectElement => new SelectElement(UniqueIdentifierSelectWebElement);

		[FindsBy(How = How.ClassName, Using = "identifier")]
		protected IWebElement SettingsDivElement { get; set; }

		protected IWebElement CustodianManagerContainsLinkRowElement { get; set; }

		public ImportThirdPage(RemoteWebDriver driver) : base(driver)
		{
			WaitForPage();
			PageFactory.InitElements(driver, this);
			CustodianManagerContainsLinkRowElement = SettingsDivElement.FindElements(By.ClassName("field-row"))[7];
		}

		public void SelectSourceField(string fieldName)
		{
			SelectField(SelectSourceFieldsElement, AddSourceFieldElement, fieldName);
		}

		public void SelectAllSourceFields()
		{
			AddAllSourceFieldsElement.Click();
		}

		public void SelectDestinationField(string fieldName)
		{
			SelectField(SelectDestinationFieldElement, AddDestinationFieldElement, fieldName);
		}

		public void SelectAllDestinationFields()
		{
			AddAllDestinationFieldsElement.Click();
		}

		public void MapFields()
		{
			MapFieldsElement.Click();
		}

		public string Overwrite
		{
			get { return OverwriteSelectElement.SelectedOption.Text; }
			set
			{
				if (value != null)
				{
					OverwriteSelectElement.SelectByText(value);
				}
			}
		}

		public string UniqueIdentifier
		{
			get { return UniqueIdentifierSelectElement.SelectedOption.Text; }
			set
			{
				if (value != null)
				{
					UniqueIdentifierSelectElement.SelectByText(value);
				}
			}
		}

		public void SetCustodianManagerContainsLink(bool value)
		{
			int checkboxNumber = value ? 0 : 1;
			IWebElement input = CustodianManagerContainsLinkRowElement.FindElements(By.TagName("input"))[checkboxNumber];
			input.Click();
		}

		private void SelectField(SelectElement selectElement, IWebElement addFieldElement, string fieldName)
		{
			if (string.IsNullOrEmpty(fieldName))
			{
				return;
			}

			SelectOption(selectElement, fieldName);

			addFieldElement.Click();
		}

		private static void SelectOption(SelectElement selectElement, string textToSearchFor)
		{
			IWebElement option = selectElement.WrappedElement.FindElement(By.XPath($".//option[starts-with(normalize-space(.), \"{textToSearchFor}\")]"));
			if (!option.Selected)
			{
				option.Click();
			}
		}

	}
}
