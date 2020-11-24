using System.Linq;
using kCura.IntegrationPoint.Tests.Core.Models.Import;
using kCura.IntegrationPoint.Tests.Core.Models.Shared;
using kCura.IntegrationPoints.UITests.Driver;
using kCura.Utility;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using SeleniumExtras.PageObjects;
using OpenQA.Selenium.Support.UI;

namespace kCura.IntegrationPoints.UITests.Pages
{
	public abstract class ImportThirdPage<TModel> : GeneralPage
	{
		protected IWebElement SaveButton => Driver.FindElementEx(By.Id("save"));

		protected IWebElement SourceFieldsElement => Driver.FindElementEx(By.Id("source-fields"));

		protected IWebElement AddSourceFieldElement => Driver.FindElementEx(By.Id("add-source-field"));

		protected IWebElement AddAllSourceFieldsElement => Driver.FindElementEx(By.Id("add-all-source-fields"));

		protected IWebElement DestinationFieldElement => Driver.FindElementEx(By.Id("workspace-fields"));

		protected IWebElement AddDestinationFieldElement => Driver.FindElementEx(By.Id("add-workspace-field"));

		protected IWebElement AddAllDestinationFieldsElement => Driver.FindElementEx(By.Id("add-all-workspace-fields"));

		protected IWebElement MapFieldsElement => Driver.FindElementEx(By.Id("mapFieldsBtn"));

		protected IWebElement OverwriteSelectWebElement => Driver.FindElementEx(By.Id("overwrite"));

		protected IWebElement UniqueIdentifierSelectWebElement => Driver.FindElementEx(By.Id("overlay-identifier"));

		protected IWebElement SettingsDivElement => Driver.FindElementEx(By.ClassName("identifier"));

		protected IWebElement EntityManagerContainsLinkRowElement { get; set; }

		protected IWebElement CopyNativeFilesPhysicalFilesElement => Driver.FindElementEx(By.Id("native-file-mode-radio-0"));

		protected IWebElement CopyNativeFilesLinksOnlyElement => Driver.FindElementEx(By.Id("native-file-mode-radio-1"));

		protected IWebElement CopyNativeFilesNoElement => Driver.FindElementEx(By.Id("native-file-mode-radio-2"));

		protected IWebElement NativeFilePathSelectWebElement => Driver.FindElementEx(By.Id("native-filePath"));

		protected IWebElement UseFolderPathInfoYesElement => Driver.FindElementEx(By.Id("folder-path-radio-0"));

		protected IWebElement UseFolderPathInfoNoElement => Driver.FindElementEx(By.Id("folder-path-radio-1"));

		protected IWebElement FolderPathInfoSelectWebElement => Driver.FindElementEx(By.Id("folderPath"));

		protected IWebElement CellContainsFileLocationYesElement => Driver.FindElementEx(By.Id("extracted-text-radio-0"));

		protected IWebElement CellContainsFileLocationNoElement => Driver.FindElementEx(By.Id("extracted-text-radio-1"));

		protected IWebElement FileLocationCellSelectDivElement => Driver.FindElementEx(By.ClassName("margin-top-8px"));

		protected ImportThirdPage(RemoteWebDriver driver) : base(driver)
		{
			WaitForPage();
			PageFactory.InitElements(driver, this);
			EntityManagerContainsLinkRowElement = SettingsDivElement.FindElementsEx(By.ClassName("field-row"))[11];
		}

		protected SelectElement SelectSourceFieldsElement => new SelectElement(SourceFieldsElement);

		public void SelectSourceField(string fieldName)
		{
			SelectField(SelectSourceFieldsElement, AddSourceFieldElement, fieldName);
		}

		public void SelectAllSourceFields()
		{
			AddAllSourceFieldsElement.ClickEx(Driver);
		}

		protected SelectElement SelectDestinationFieldElement => new SelectElement(DestinationFieldElement);

		public void SelectDestinationField(string fieldName)
		{
			SelectField(SelectDestinationFieldElement, AddDestinationFieldElement, fieldName);
		}

		public void SelectAllDestinationFields()
		{
			AddAllDestinationFieldsElement.ClickEx(Driver);
		}

		public void MapFields()
		{
			MapFieldsElement.ClickEx(Driver);
		}

		protected SelectElement OverwriteSelectElement => new SelectElement(OverwriteSelectWebElement);

		public string Overwrite
		{
			get { return OverwriteSelectElement.SelectedOption.Text; }
			set
			{
				if (value != null)
				{
					OverwriteSelectElement.SelectByTextEx(value, Driver);
				}
			}
		}

		protected SelectElement UniqueIdentifierSelectElement => new SelectElement(UniqueIdentifierSelectWebElement);

		public string UniqueIdentifier
		{
			get { return UniqueIdentifierSelectElement.SelectedOption.Text; }
			set
			{
				if (value != null)
				{
					UniqueIdentifierSelectElement.SelectByTextEx(value, Driver);
				}
			}
		}

		public void SetEntityManagerContainsLink(bool value)
		{
			int checkboxNumber = value ? 0 : 1;
			IWebElement input = EntityManagerContainsLinkRowElement.FindElementsEx(By.TagName("input"))[checkboxNumber];
			input.ClickEx(Driver);
		}

		public CopyNativeFiles CopyNativeFiles
		{
			set
			{
				if (value == CopyNativeFiles.PhysicalFiles)
				{
					CopyNativeFilesPhysicalFilesElement.ClickEx(Driver);
				}
				else if (value == CopyNativeFiles.LinksOnly)
				{
					CopyNativeFilesLinksOnlyElement.ClickEx(Driver);
				}
				else if (value == CopyNativeFiles.No)
				{
					CopyNativeFilesNoElement.ClickEx(Driver);
				}
			}
		}

		protected SelectElement NativeFilePathSelectElement => new SelectElement(NativeFilePathSelectWebElement);

		public string NativeFilePath
		{
			set
			{
				NativeFilePathSelectElement.SelectByTextEx(value, Driver);
			}
		}

		public bool UseFolderPathInformation
		{
			set
			{
				IWebElement element = value ? UseFolderPathInfoYesElement : UseFolderPathInfoNoElement;
				element.ClickEx(Driver);
			}
		}

		protected SelectElement FolderPathInfoSelectElement => new SelectElement(FolderPathInfoSelectWebElement);

		public string FolderPathInformation
		{
			set
			{
				FolderPathInfoSelectElement.SelectByTextEx(value, Driver);
			}
		}

		public bool CellContainsFileLocation
		{
			set
			{
				IWebElement element = value ? CellContainsFileLocationYesElement : CellContainsFileLocationNoElement;
				element.ClickEx(Driver);
			}
		}

		protected SelectElement FileLocationCellSelectElement =>
			new SelectElement(FileLocationCellSelectDivElement.FindElementsEx(By.TagName("select")).First());

		public string FileLocationCell
		{
			set
			{
				FileLocationCellSelectElement.ScrollIntoView(Driver);
				FileLocationCellSelectElement.SelectByTextEx(value, Driver);
			}
		}

		protected SelectElement EncodingForUndetectableFilesSelectElement =>
			new SelectElement(FileLocationCellSelectDivElement.FindElementsEx(By.TagName("select")).ToList()[1]);

		public string EncodingForUndetectableFiles
		{
			set
			{
				EncodingForUndetectableFilesSelectElement.SelectByTextEx(value, Driver);
			}
		}

		public IntegrationPointDetailsPage SaveIntegrationPoint()
		{
			SaveButton.ScrollIntoView(Driver);
			SaveButton.ClickEx(Driver);
			return new IntegrationPointDetailsPage(Driver);
		}

		protected void SetUpSharedSettingsModel(ImportSettingsModel model)
		{
			foreach (var tuple in model.FieldMapping)
			{
				string sourceField = tuple.Item1;
				string destinationField = tuple.Item2;
				SelectSourceField(sourceField);
				SelectDestinationField(destinationField);
			}

			if (model.MapFieldsAutomatically)
			{
				MapFields();
			}

			Overwrite = model.Overwrite.GetDescription();
		}

		protected void SetUpEntitySettingsModel(ImportEntitySettingsModel model)
		{
			UniqueIdentifier = model.UniqueIdentifier;
			SetEntityManagerContainsLink(model.EntityManagerContainsLink);
		}

		protected void SetUpDocumentSettingsModel(ImportDocumentSettingsModel model)
		{
			CopyNativeFiles = model.CopyNativeFiles;
			if (model.CopyNativeFiles != CopyNativeFiles.No)
			{
				NativeFilePath = model.NativeFilePath;
			}
			UseFolderPathInformation = model.UseFolderPathInformation;
			if (model.UseFolderPathInformation)
			{
				FolderPathInformation = model.FolderPathInformation;
			}
			CellContainsFileLocation = model.CellContainsFileLocation;
			if (model.CellContainsFileLocation)
			{
				FileLocationCell = model.FileLocationCell;
				EncodingForUndetectableFiles = model.EncodingForUndetectableFiles;
			}
		}

		public abstract void SetupModel(TModel model);

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
			IWebElement option = selectElement.WrappedElement.FindElementEx(By.XPath($".//option[starts-with(normalize-space(.), \"{textToSearchFor}\")]"));
			if (!option.Selected)
			{
				option.ClickEx(Driver);
			}
		}
	}
}
