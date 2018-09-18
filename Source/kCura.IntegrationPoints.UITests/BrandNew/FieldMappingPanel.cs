using System.Collections.Generic;
using kCura.IntegrationPoints.UITests.Components;
using kCura.IntegrationPoints.UITests.Driver;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace kCura.IntegrationPoints.UITests.BrandNew
{
	public class FieldMappingPanel : Component
	{
		public SimpleSelectField SourceFields => new SimpleSelectField(Parent.FindElement(By.XPath(".//*[@id='source-fields']/..")));

		public IWebElement AddSourceFieldButton => Parent.FindElement(By.Id("add-source-field"));

		public IWebElement AddAllSourceFieldsButton => Parent.FindElement(By.Id("add-all-source-fields"));

		public SimpleSelectField DestinationFields => new SimpleSelectField(Parent.FindElement(By.XPath(".//*[@id='workspace-fields']/..")));

		public IWebElement AddDestinationFieldButton => Parent.FindElement(By.Id("add-workspace-field"));

		public IWebElement AddAllDestinationFieldsButton => Parent.FindElement(By.Id("add-all-workspace-fields"));

		public IWebElement MapFieldsButton => Parent.FindElement(By.Id("mapFieldsBtn"));
		
		public FieldMappingPanel(IWebElement parent) : base(parent)
		{
		}

		public void SelectSourceField(string fieldName)
		{
			SelectField(SourceFields.Select, AddSourceFieldButton, fieldName);
		}

		public void SelectAllSourceFields()
		{
			AddAllSourceFieldsButton.ClickEx();
		}

		public void SelectDestinationField(string fieldName)
		{
			SelectField(DestinationFields.Select, AddDestinationFieldButton, fieldName);
		}

		public void SelectAllDestinationFields()
		{
			AddAllDestinationFieldsButton.ClickEx();
		}

		public void MapFields()
		{
			MapFieldsButton.ClickEx();
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

			addFieldElement.ClickEx();
		}

		private static void SelectOption(SelectElement selectElement, string textToSearchFor)
		{
			IWebElement option = selectElement.WrappedElement.FindElement(
				By.XPath($".//option[starts-with(normalize-space(.), \"{textToSearchFor}\")]"));
			if (!option.Selected)
			{
				option.ClickEx();
			}
		}
	}
}
