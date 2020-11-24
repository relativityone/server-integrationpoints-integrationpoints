using System.Collections.Generic;
using kCura.IntegrationPoints.UITests.Components;
using kCura.IntegrationPoints.UITests.Driver;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace kCura.IntegrationPoints.UITests.BrandNew
{
	public class FieldMappingPanel : Component
	{
		public SimpleSelectField SourceFields => new SimpleSelectField(Parent.FindElementEx(By.XPath(".//*[@id='source-fields']/..")), Driver);

		public IWebElement AddSourceFieldButton => Parent.FindElementEx(By.Id("add-source-field"));

		public IWebElement AddAllSourceFieldsButton => Parent.FindElementEx(By.Id("add-all-source-fields"));

		public SimpleSelectField DestinationFields => new SimpleSelectField(Parent.FindElementEx(By.XPath(".//*[@id='workspace-fields']/..")), Driver);

		public IWebElement AddDestinationFieldButton => Parent.FindElementEx(By.Id("add-workspace-field"));

		public IWebElement AddAllDestinationFieldsButton => Parent.FindElementEx(By.Id("add-all-workspace-fields"));

		public IWebElement MapFieldsButton => Parent.FindElementEx(By.Id("mapFieldsBtn"));
		
		public FieldMappingPanel(IWebElement parent, IWebDriver driver) : base(parent, driver)
		{
		}

		public void SelectSourceField(string fieldName)
		{
			SelectField(SourceFields.Select, AddSourceFieldButton, fieldName);
		}

		public void SelectAllSourceFields()
		{
			AddAllSourceFieldsButton.ClickEx(Driver);
		}

		public void SelectDestinationField(string fieldName)
		{
			SelectField(DestinationFields.Select, AddDestinationFieldButton, fieldName);
		}

		public void SelectAllDestinationFields()
		{
			AddAllDestinationFieldsButton.ClickEx(Driver);
		}

		public void MapFields()
		{
			MapFieldsButton.ClickEx(Driver);
		}

		public FieldMappingPanel MapFields(IDictionary<string, string> mappings)
		{
			foreach (var mapping in mappings)
			{
				SelectSourceField(mapping.Key);
				SelectDestinationField(mapping.Value);
			}

			return this;
		}

		private void SelectField(SelectElement selectElement, IWebElement addFieldElement, string fieldName)
		{
			if (string.IsNullOrEmpty(fieldName))
			{
				return;
			}

			SelectOption(selectElement, fieldName);

			addFieldElement.ClickEx(Driver);
		}

		private void SelectOption(SelectElement selectElement, string textToSearchFor)
		{
			IWebElement option = selectElement.WrappedElement.FindElementEx(
				By.XPath($".//option[starts-with(normalize-space(.), \"{textToSearchFor}\")]"));
			if (!option.Selected)
			{
				option.ClickEx(Driver);
			}
		}
	}
}
