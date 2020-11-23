using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.UITests.Driver;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using SeleniumExtras.PageObjects;
using OpenQA.Selenium.Support.UI;

namespace kCura.IntegrationPoints.UITests.Pages
{
	public class ExportToFileThirdPageLoadFileOptions : ExportToFileThirdPagePanel
	{
		protected IWebElement ImageFileFormatSelectWebElement => Driver.FindElementEx(By.Id("imageDataFileFormatSelector"));

		protected IWebElement DataFileFormatSelectWebElement => Driver.FindElementEx(By.Id("dataFileFormatSelector"));

		protected IWebElement DataFileEncodingSelectWebElement => Driver.FindElementEx(By.Id("dataFileEncodingSelector"));

		protected IWebElement RelativeFilePathRadio => Driver.FindElementEx(By.Id("filePath_0"));

		protected IWebElement AbsoluteFilePathRadio => Driver.FindElementEx(By.Id("filePath_1"));

		protected IWebElement UserPrefixFilePathRadio => Driver.FindElementEx(By.Id("filePath_2"));

		protected IWebElement UserPrefixInput => Driver.FindElementEx(By.Id("filePathUserprefix_2"));

		protected IWebElement IncludeNativeFilesPathCheckbox => Driver.FindElementEx(By.Id("include-native-files-path-checkbox"));

		protected IWebElement ExportMultipleChoiceFieldsAsNestedCheckbox => Driver.FindElementEx(By.Id("export-multiple-choice-fields-as-nested"));

		protected IWebElement NameOutputFilesAfterSelectWebElement => Driver.FindElementEx(By.Id("exportNativeWithFilenameFromTypeSelector"));

		protected IWebElement AppendOriginalFileNameCheckbox => Driver.FindElementEx(By.Id("append-original-file-name-checkbox"));

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
					ImageFileFormatSelectElement.SelectByTextEx(value, Driver);
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
					DataFileFormatSelectElement.SelectByTextEx(value, Driver);
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
					DataFileEncodingSelectElement.SelectByTextEx(value, Driver);
				}
			}
		}

		public void SelectFilePath(ExportToLoadFileProviderModel.FilePathTypeEnum filePath)
		{
			switch (filePath)
			{
				case ExportToLoadFileProviderModel.FilePathTypeEnum.Absolute:
					AbsoluteFilePathRadio.ClickEx(Driver);
					break;
				case ExportToLoadFileProviderModel.FilePathTypeEnum.Relative:
					RelativeFilePathRadio.ClickEx(Driver);
					break;
				case ExportToLoadFileProviderModel.FilePathTypeEnum.UserPrefix:
					UserPrefixFilePathRadio.ClickEx(Driver);
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
			IncludeNativeFilesPathCheckbox.ClickEx(Driver);
		}

		public void ExportMultipleChoiceFieldsAsNested()
		{
			ExportMultipleChoiceFieldsAsNestedCheckbox.ClickEx(Driver);
		}

		protected SelectElement NameOutputFilesAfterSelectElement => new SelectElement(NameOutputFilesAfterSelectWebElement);

		public string NameOutputFilesAfter
		{
			get { return NameOutputFilesAfterSelectElement.SelectedOption.Text; }
			set
			{
				if (value != null)
				{
					NameOutputFilesAfterSelectElement.SelectByTextEx(value, Driver);
				}
			}
		}

		public void AppendOriginalFileName()
		{
			AppendOriginalFileNameCheckbox.ClickEx(Driver);
		}
	}
}
