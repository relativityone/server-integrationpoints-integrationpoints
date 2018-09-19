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
		[FindsBy(How = How.Id, Using = "imageFileTypesSelector")]
		protected IWebElement FileTypeSelectWebElement { get; set; }

		[FindsBy(How = How.Id, Using = "image-production-precedence")]
		protected IWebElement ImagePrecedenceSelectWebElement { get; set; }

		[FindsBy(How = How.Id, Using = "subdirectory-image-prefix-input")]
		protected IWebElement ImageSubdirectoryPrefixInput { get; set; }

		[FindsBy(How = How.Id, Using = "subdirectory-native-prefix-input")]
		protected IWebElement NativeSubdirectoryPrefixInput { get; set; }

		[FindsBy(How = How.Id, Using = "textFileEncodingSelector")]
		protected IWebElement TextFileEncodingSelectWebElement { get; set; }

		[FindsBy(How = How.Id, Using = "text-precedence-button")]
		protected IWebElement TextPrecedenceButton { get; set; }

		[FindsBy(How = How.Id, Using = "textPrecedencePicker")]
		protected IWebElement TextPrecedencePickerElement { get; set; }

		[FindsBy(How = How.Id, Using = "subdirectory-text-prefix-input")]
		protected IWebElement TextSubdirectoryPrefixInput { get; set; }

		public ExportToFileThirdPageImageNativeTextOptions(RemoteWebDriver driver) : base(driver)
		{
		}

		protected SelectElement FileTypeSelectElement => new SelectElement(FileTypeSelectWebElement);

		public string FileType
		{
			get { return FileTypeSelectElement.SelectedOption.Text; }
			set
			{
				if (value != null)
				{
					FileTypeSelectElement.SelectByText(value);
				}
			}
		}

		protected SelectElement ImagePrecedenceSelectElement => new SelectElement(ImagePrecedenceSelectWebElement);

		public string ImagePrecedence
		{
			get { return ImagePrecedenceSelectElement.SelectedOption.Text; }
			set
			{
				if (value != null)
				{
					ImagePrecedenceSelectElement.SelectByText(value);
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
					TextFileEncodingSelectElement.SelectByText(value);
				}
			}
		}

		public void SelectTextPrecedenceField(string fieldName)
		{
			do
			{
				TextPrecedenceButton.ClickEx();
				Sleep(500);
			}
			while (!IsAnyElementVisible(TextPrecedencePickerElement, By.Id("ok-button")));
			SelectOption(TextPrecedencePickerElement, fieldName ?? "Extracted Text");
			ClickButton(TextPrecedencePickerElement, "select-single-item");
			ClickButton(TextPrecedencePickerElement, "ok-button");
		}

		private void SelectOption(IWebElement parentElement, string textToSearchFor)
		{
			List<IWebElement> options = parentElement.FindElements(By.TagName("option")).ToList();
		    IWebElement optionToSelect = options.First(option => option.Text == textToSearchFor);

			if (!optionToSelect.Selected)
			{
				optionToSelect.ClickEx();
			}
		}

		private void ClickButton(IWebElement parentElement, string id)
		{
			IWebElement okButton = parentElement.FindElement(By.Id(id));
			okButton.ClickEx();
		}

		public string TextSubdirectoryPrefix
		{
			get { return TextSubdirectoryPrefixInput.Text; }
			set { SetInputText(TextSubdirectoryPrefixInput, value); }
		}
	}
}
