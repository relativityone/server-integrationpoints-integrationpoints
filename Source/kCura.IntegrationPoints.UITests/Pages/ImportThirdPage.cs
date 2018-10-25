﻿using System.Linq;
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
		[FindsBy(How = How.Id, Using = "save")]
		protected IWebElement SaveButton { get; set; }

		[FindsBy(How = How.Id, Using = "source-fields")]
		protected IWebElement SourceFieldsElement { get; set; }

		[FindsBy(How = How.Id, Using = "add-source-field")]
		protected IWebElement AddSourceFieldElement { get; set; }

		[FindsBy(How = How.Id, Using = "add-all-source-fields")]
		protected IWebElement AddAllSourceFieldsElement { get; set; }

		[FindsBy(How = How.Id, Using = "workspace-fields")]
		protected IWebElement DestinationFieldElement { get; set; }

		[FindsBy(How = How.Id, Using = "add-workspace-field")]
		protected IWebElement AddDestinationFieldElement { get; set; }

		[FindsBy(How = How.Id, Using = "add-all-workspace-fields")]
		protected IWebElement AddAllDestinationFieldsElement { get; set; }

		[FindsBy(How = How.Id, Using = "mapFieldsBtn")]
		protected IWebElement MapFieldsElement { get; set; }

		[FindsBy(How = How.Id, Using = "overwrite")]
		protected IWebElement OverwriteSelectWebElement { get; set; }

		[FindsBy(How = How.Id, Using = "overlay-identifier")]
		protected IWebElement UniqueIdentifierSelectWebElement { get; set; }

		[FindsBy(How = How.ClassName, Using = "identifier")]
		protected IWebElement SettingsDivElement { get; set; }

		protected IWebElement EntityManagerContainsLinkRowElement { get; set; }

		[FindsBy(How = How.Id, Using = "native-file-mode-radio-0")]
		protected IWebElement CopyNativeFilesPhysicalFilesElement { get; set; }

		[FindsBy(How = How.Id, Using = "native-file-mode-radio-1")]
		protected IWebElement CopyNativeFilesLinksOnlyElement { get; set; }

		[FindsBy(How = How.Id, Using = "native-file-mode-radio-2")]
		protected IWebElement CopyNativeFilesNoElement { get; set; }

		[FindsBy(How = How.Id, Using = "native-filePath")]
		protected IWebElement NativeFilePathSelectWebElement { get; set; }

		[FindsBy(How = How.Id, Using = "folder-path-radio-0")]
		protected IWebElement UseFolderPathInfoYesElement { get; set; }

		[FindsBy(How = How.Id, Using = "folder-path-radio-1")]
		protected IWebElement UseFolderPathInfoNoElement { get; set; }

		[FindsBy(How = How.Id, Using = "folderPath")]
		protected IWebElement FolderPathInfoSelectWebElement { get; set; }

		[FindsBy(How = How.Id, Using = "extracted-text-radio-0")]
		protected IWebElement CellContainsFileLocationYesElement { get; set; }

		[FindsBy(How = How.Id, Using = "extracted-text-radio-1")]
		protected IWebElement CellContainsFileLocationNoElement { get; set; }

		[FindsBy(How = How.ClassName, Using = "margin-top-8px")]
		protected IWebElement FileLocationCellSelectDivElement { get; set; }

		protected ImportThirdPage(RemoteWebDriver driver) : base(driver)
		{
			WaitForPage();
			PageFactory.InitElements(driver, this);
			EntityManagerContainsLinkRowElement = SettingsDivElement.FindElements(By.ClassName("field-row"))[11];
		}

		protected SelectElement SelectSourceFieldsElement => new SelectElement(SourceFieldsElement);

		public void SelectSourceField(string fieldName)
		{
			SelectField(SelectSourceFieldsElement, AddSourceFieldElement, fieldName);
		}

		public void SelectAllSourceFields()
		{
			AddAllSourceFieldsElement.ClickEx();
		}

		protected SelectElement SelectDestinationFieldElement => new SelectElement(DestinationFieldElement);

		public void SelectDestinationField(string fieldName)
		{
			SelectField(SelectDestinationFieldElement, AddDestinationFieldElement, fieldName);
		}

		public void SelectAllDestinationFields()
		{
			AddAllDestinationFieldsElement.ClickEx();
		}

		public void MapFields()
		{
			MapFieldsElement.ClickEx();
		}

		protected SelectElement OverwriteSelectElement => new SelectElement(OverwriteSelectWebElement);

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

		protected SelectElement UniqueIdentifierSelectElement => new SelectElement(UniqueIdentifierSelectWebElement);

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

		public void SetEntityManagerContainsLink(bool value)
		{
			int checkboxNumber = value ? 0 : 1;
			IWebElement input = EntityManagerContainsLinkRowElement.FindElements(By.TagName("input"))[checkboxNumber];
			input.ClickEx();
		}

		public CopyNativeFiles CopyNativeFiles
		{
			set
			{
				if (value == CopyNativeFiles.PhysicalFiles)
				{
					CopyNativeFilesPhysicalFilesElement.ClickEx();
				}
				else if (value == CopyNativeFiles.LinksOnly)
				{
					CopyNativeFilesLinksOnlyElement.ClickEx();
				}
				else if (value == CopyNativeFiles.No)
				{
					CopyNativeFilesNoElement.ClickEx();
				}
			}
		}

		protected SelectElement NativeFilePathSelectElement => new SelectElement(NativeFilePathSelectWebElement);

		public string NativeFilePath
		{
			set
			{
				NativeFilePathSelectElement.SelectByText(value);
			}
		}

		public bool UseFolderPathInformation
		{
			set
			{
				IWebElement element = value ? UseFolderPathInfoYesElement : UseFolderPathInfoNoElement;
				element.ClickEx();
			}
		}

		protected SelectElement FolderPathInfoSelectElement => new SelectElement(FolderPathInfoSelectWebElement);

		public string FolderPathInformation
		{
			set
			{
				FolderPathInfoSelectElement.SelectByText(value);
			}
		}

		public bool CellContainsFileLocation
		{
			set
			{
				IWebElement element = value ? CellContainsFileLocationYesElement : CellContainsFileLocationNoElement;
				element.ClickEx();
			}
		}

		protected SelectElement FileLocationCellSelectElement => 
			new SelectElement(FileLocationCellSelectDivElement.FindElements(By.TagName("select")).First());

		public string FileLocationCell
		{
			set
			{
				FileLocationCellSelectElement.ScrollIntoView();
				FileLocationCellSelectElement.SelectByText(value);
			}
		}

		protected SelectElement EncodingForUndetectableFilesSelectElement => 
			new SelectElement(FileLocationCellSelectDivElement.FindElements(By.TagName("select")).ToList()[1]);

		public string EncodingForUndetectableFiles
		{
			set
			{
				EncodingForUndetectableFilesSelectElement.SelectByText(value);
			}
		}

		public IntegrationPointDetailsPage SaveIntegrationPoint()
		{
			SaveButton.ScrollIntoView(Driver);
			SaveButton.ClickEx();
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

			addFieldElement.ClickEx();
		}

		private static void SelectOption(SelectElement selectElement, string textToSearchFor)
		{
			IWebElement option = selectElement.WrappedElement.FindElement(By.XPath($".//option[starts-with(normalize-space(.), \"{textToSearchFor}\")]"));
			if (!option.Selected)
			{
				option.ClickEx();
			}
		}
	}
}
