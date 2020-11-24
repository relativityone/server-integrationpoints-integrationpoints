using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.UITests.Driver;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using SeleniumExtras.PageObjects;
using OpenQA.Selenium.Support.UI;

namespace kCura.IntegrationPoints.UITests.Pages
{
	public class ExportToFileThirdPageImageNativeTextOptions : ExportToFileThirdPagePanel
	{
		protected IWebElement ImageSubdirectoryPrefixInput => Driver.FindElementEx(By.Id("subdirectory-image-prefix-input"));

		protected IWebElement NativeSubdirectoryPrefixInput => Driver.FindElementEx(By.Id("subdirectory-native-prefix-input"));

		protected IWebElement TextFileEncodingSelectWebElement => Driver.FindElementEx(By.Id("textFileEncodingSelector"));

		protected IWebElement TextPrecedenceButton => Driver.FindElementEx(By.Id("text-precedence-button"));

		protected IWebElement TextPrecedencePickerElement => Driver.FindElementEx(By.Id("textPrecedencePicker"));

		protected IWebElement TextSubdirectoryPrefixInput => Driver.FindElementEx(By.Id("subdirectory-text-prefix-input"));

		public ExportToFileThirdPageImageNativeTextOptions(RemoteWebDriver driver) : base(driver)
		{
		}

		public string FileType
		{
			get { return GetSelectedOption(By.Id("imageFileTypesSelector")); }
			set
			{
				if (value != null)
				{
					SelectOptionByText(value, By.Id("imageFileTypesSelector"));
				}
			}
		}

		private void SelectOptionByText(string value, By id)
		{
			Driver.GetConfiguredWait().Until(d =>
			{
				new SelectElement(Driver.FindElement(id))
					.SelectByText(value);
				return true;
			});
		}

		private string GetSelectedOption(By @by)
		{
			return new SelectElement(Driver.FindElementEx(@by)).SelectedOption.Text;
		}

		protected By ImagePrecedenceSelectElementSelector => By.Id("image-production-precedence");

		public string ImagePrecedence
		{
			get { return GetSelectedOption(ImagePrecedenceSelectElementSelector); }
			set
			{
				if (value != null)
				{
					SelectOptionByText(value, ImagePrecedenceSelectElementSelector);
				}
			}
		}

		public string ImageSubdirectoryPrefix
		{
			get { return ImageSubdirectoryPrefixInput.Text; }
			set { SetInputText(ImageSubdirectoryPrefixInput, value); }
		}

		public string NativeSubdirectoryPrefix
		{
			get { return NativeSubdirectoryPrefixInput.Text; }
			set { SetInputText(NativeSubdirectoryPrefixInput, value); }
		}

		protected SelectElement TextFileEncodingSelectElement => new SelectElement(TextFileEncodingSelectWebElement);

		public string TextFileEncoding
		{
			get { return TextFileEncodingSelectElement.SelectedOption.Text; }
			set
			{
				if (value != null && value != TextFileEncoding)
				{
					TextFileEncodingSelectElement.SelectByTextEx(value, Driver);
				}
			}
		}

		public void SelectTextPrecedenceField(string fieldName)
		{
			do
			{
				TextPrecedenceButton.ClickEx(Driver);
			}
			while (!IsAnyElementVisible(TextPrecedencePickerElement, By.Id("ok-button")));

			SelectOption(TextPrecedencePickerElement, fieldName ?? "Extracted Text");
			ClickButton(TextPrecedencePickerElement, "select-single-item");
			ClickButton(TextPrecedencePickerElement, "ok-button");
		}

		private void SelectOption(IWebElement parentElement, string textToSearchFor)
		{
			List<IWebElement> options = parentElement.FindElementsEx(By.TagName("option")).ToList();
			IWebElement optionToSelect = options.First(option => option.Text == textToSearchFor);

			if (!optionToSelect.Selected)
			{
				optionToSelect.ClickEx(Driver);
			}
		}

		private void ClickButton(IWebElement parentElement, string id)
		{
			IWebElement okButton = parentElement.FindElementEx(By.Id(id));
			okButton.ClickEx(Driver);
		}

		public string TextSubdirectoryPrefix
		{
			get { return TextSubdirectoryPrefixInput.Text; }
			set { SetInputText(TextSubdirectoryPrefixInput, value); }
		}
	}
}
