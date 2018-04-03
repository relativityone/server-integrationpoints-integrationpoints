using kCura.IntegrationPoint.Tests.Core.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using SeleniumExtras.PageObjects;
using OpenQA.Selenium.Support.UI;

namespace kCura.IntegrationPoints.UITests.Pages
{
	public class ExportToFileThirdPageLoadFileOptions : ExportToFileThirdPagePanel
	{
		[FindsBy(How = How.Id, Using = "imageDataFileFormatSelector")]
		protected IWebElement ImageFileFormatSelectWebElement { get; set; }

		[FindsBy(How = How.Id, Using = "dataFileFormatSelector")]
		protected IWebElement DataFileFormatSelectWebElement { get; set; }

		[FindsBy(How = How.Id, Using = "dataFileEncodingSelector")]
		protected IWebElement DataFileEncodingSelectWebElement { get; set; }

		[FindsBy(How = How.Id, Using = "filePath_0")]
		protected IWebElement RelativeFilePathRadio { get; set; }

		[FindsBy(How = How.Id, Using = "filePath_1")]
		protected IWebElement AbsoluteFilePathRadio { get; set; }

		[FindsBy(How = How.Id, Using = "filePath_2")]
		protected IWebElement UserPrefixFilePathRadio { get; set; }

		[FindsBy(How = How.Id, Using = "filePathUserprefix_2")]
		protected IWebElement UserPrefixInput { get; set; }

		[FindsBy(How = How.Id, Using = "include-native-files-path-checkbox")]
		protected IWebElement IncludeNativeFilesPathCheckbox { get; set; }

		[FindsBy(How = How.Id, Using = "export-multiple-choice-fields-as-nested")]
		protected IWebElement ExportMultipleChoiceFieldsAsNestedCheckbox { get; set; }

		[FindsBy(How = How.Id, Using = "exportNativeWithFilenameFromTypeSelector")]
		protected IWebElement NameOutputFilesAfterSelectWebElement { get; set; }

		[FindsBy(How = How.Id, Using = "append-original-file-name-checkbox")]
		protected IWebElement AppendOriginalFileNameCheckbox { get; set; }

		public ExportToFileThirdPageLoadFileOptions(RemoteWebDriver driver) : base(driver)
		{
		}

		protected SelectElement ImageFileFormatSelectElement => new SelectElement(ImageFileFormatSelectWebElement);

		public string ImageFileFormat
		{
			get { return ImageFileFormatSelectElement.SelectedOption.Text; }
			set
			{
				if (value != null)
				{
					ImageFileFormatSelectElement.SelectByText(value);
				}
			}
		}

		protected SelectElement DataFileFormatSelectElement => new SelectElement(DataFileFormatSelectWebElement);

		public string DataFileFormat
		{
			get { return DataFileFormatSelectElement.SelectedOption.Text; }
			set
			{
				if (value != null)
				{
					DataFileFormatSelectElement.SelectByText(value);
				}
			}
		}

		protected SelectElement DataFileEncodingSelectElement => new SelectElement(DataFileEncodingSelectWebElement);

		public string DataFileEncoding
		{
			get { return DataFileEncodingSelectElement.SelectedOption.Text; }
			set
			{
				if (value != null)
				{
					DataFileEncodingSelectElement.SelectByText(value);
				}
			}
		}

		public void SelectFilePath(ExportToLoadFileProviderModel.FilePathTypeEnum filePath)
		{
			switch (filePath)
			{
				case ExportToLoadFileProviderModel.FilePathTypeEnum.Absolute:
					AbsoluteFilePathRadio.Click();
					break;
				case ExportToLoadFileProviderModel.FilePathTypeEnum.Relative:
					RelativeFilePathRadio.Click();
					break;
				case ExportToLoadFileProviderModel.FilePathTypeEnum.UserPrefix:
					UserPrefixFilePathRadio.Click();
					break;
			}
		}

		public string UserPrefix
		{
			get { return UserPrefixInput.Text; }
			set { SetInputText(UserPrefixInput, value); }
		}

		public void IncludeNativeFilesPath()
		{
			IncludeNativeFilesPathCheckbox.Click();
		}

		public void ExportMultipleChoiceFieldsAsNested()
		{
			ExportMultipleChoiceFieldsAsNestedCheckbox.Click();
		}

		protected SelectElement NameOutputFilesAfterSelectElement => new SelectElement(NameOutputFilesAfterSelectWebElement);

		public string NameOutputFilesAfter
		{
			get { return NameOutputFilesAfterSelectElement.SelectedOption.Text; }
			set
			{
				if (value != null)
				{
					NameOutputFilesAfterSelectElement.SelectByText(value);
				}
			}
		}

		public void AppendOriginalFileName()
		{
			AppendOriginalFileNameCheckbox.Click();
		}
	}
}
