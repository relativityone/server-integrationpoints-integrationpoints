using System.Collections.Generic;
using System.Linq;
using System.Threading;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.UITests.Configuration;
using kCura.IntegrationPoints.UITests.Driver;
using kCura.IntegrationPoints.UITests.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using SeleniumExtras.PageObjects;
using OpenQA.Selenium.Support.UI;
using Serilog;

namespace kCura.IntegrationPoints.UITests.Pages
{
	public class PushToRelativityThirdPage : GeneralPage
	{
		private static readonly ILogger Log = LoggerFactory.CreateLogger(typeof(TestContext));
		protected IWebElement SaveButton => Driver.FindElementEx(By.Id("save"));

		protected IWebElement SelectCopyPhysicalFilesElement => Driver.FindElementEx(By.Id("native-file-mode-radio-0"));

		protected IWebElement SelectCopyLinksOnlyElement => Driver.FindElementEx(By.Id("native-file-mode-radio-1"));

		protected IWebElement SelectCopyNoFilesElement => Driver.FindElementEx(By.Id("native-file-mode-radio-2"));

		public IWebElement SelectCopyImagesYesElement => Driver.FindElementEx(By.Id("exportImages-radio-0"));

		protected IWebElement SelectCopyImagesNoElement => Driver.FindElementEx(By.Id("exportImages-radio-1"));

		protected IWebElement SelectMoveExitstingDocumentsYesElement => Driver.FindElementEx(By.Id("move-documents-radio-0"));

		protected IWebElement SelectMoveExitstingDocumentsNoElement => Driver.FindElementEx(By.Id("move-documents-radio-1"));

		protected IWebElement SelectCopyFilesToRepositoryYesElement => Driver.FindElementEx(By.Id("native-file-radio-0"));

		protected IWebElement SelectCopyFilesToRepositoryNoElement => Driver.FindElementEx(By.Id("native-file-radio-1"));

		protected IWebElement MapAllFieldsElement => Driver.FindElementEx(By.Id("mapFieldsBtn"));

		protected IWebElement MapFieldsFromSavedSearchButton => Driver.FindElementEx(By.Id("mapFieldsFromSavedSearchBtn"));

		protected IWebElement OverwriteElement => Driver.FindElementEx(By.Id("overwrite"));

		protected IWebElement MultiSelectFieldOverlayBehaviorElement => Driver.FindElementEx(By.Id("overlay-field-behavior"));

		protected IWebElement UseFolderPathElement => Driver.FindElementEx(By.Id("folderPathInformationSelect"));

		protected IWebElement ImagePrecedenceElement => Driver.FindElementEx(By.Id("image-production-precedence"));

		protected IWebElement ReadFromFieldElement => Driver.FindElementEx(By.Id("folderPath"));

		protected IWebElement SourceFieldsElement => Driver.FindElementEx(By.Id("source-fields"));

		protected IWebElement AddSourceFieldElement => Driver.FindElementEx(By.Id("add-source-field"));

		protected IWebElement DestinationFieldsElement => Driver.FindElementEx(By.Id("workspace-fields"));

		protected IWebElement AddWorkspaceFieldElement => Driver.FindElementEx(By.Id("add-workspace-field"));

		protected IWebElement ProductionPrecedenceElement => Driver.FindElementEx(By.Id("image-production-selection"));

		protected IWebElement ChooseProductionPrecedenceBtn => Driver.FindElementEx(By.Id("image-production-precedence-button"));

		protected IWebElement IncludeOriginalImagesIfNotProducedElement => Driver.FindElementEx(By.Id("image-include-original-images-checkbox"));

		protected IWebElement AvailableProductionsSelectElement => Driver.FindElementEx(By.Id("popup-list-available"));

		protected IWebElement AvailableProductionOkBtn => Driver.FindElementEx(By.Id("ok-button"));

		protected IWebElement CancelBtn => Driver.FindElementEx(By.Id("cancelBtn"));

		protected IWebElement PageInfoMessage => Driver.FindElementEx(By.Id("page-info-message"));

		public IWebElement InvalidMap0WebElement => GetElementById("invalidMap-0");
		public IWebElement InvalidMap1WebElement => GetElementById("invalidMap-1");
		public IWebElement InvalidMap2WebElement => GetElementById("invalidMap-2");
		public IWebElement InvalidReasons00WebElement => GetElementById("invalidReasons-0-0");
		public IWebElement InvalidReasons10WebElement => GetElementById("invalidReasons-1-0");
		public IWebElement InvalidReasons11WebElement => GetElementById("invalidReasons-1-1");
		public IWebElement InvalidReasons20WebElement => GetElementById("invalidReasons-2-0");

		public IWebElement ObjectIdentifierWarning => GetElementById("objectIdentifierWarning");
		public IWebElement MappedFieldsWarning => GetElementById("mappedFieldsWarning");
		public IWebElement ClearAndProceedBtn => GetElementById("clearAndProceedBtn");
		
		public IWebElement ObjectIdentifierWarningOrNull => GetElementByIdOrNull("objectIdentifierWarning");
		public IWebElement MappedFieldsWarningOrNull => GetElementByIdOrNull("mappedFieldsWarning");
		public IWebElement ClearAndProceedBtnOrNull => GetElementByIdOrNull("clearAndProceedBtn");


		public IWebElement ProceedBtn => GetElementById("proceedBtn");
		public IWebElement PopupMessageDiv => GetElementById("msgDiv");

		protected SelectElement SelectOverwriteElement => new SelectElement(OverwriteElement);

		protected SelectElement SelectMultiSelectFieldOverlayBehaviorElement => new SelectElement(MultiSelectFieldOverlayBehaviorElement);

		protected SelectElement SelectUseFolderPathElement => new SelectElement(UseFolderPathElement);

		protected SelectElement SelectReadFromFieldElement => new SelectElement(ReadFromFieldElement);

		protected SelectElement SelectImagePrecedenceElement => new SelectElement(ImagePrecedenceElement);

		protected SelectElement SelectSourceFieldsElement => new SelectElement(SourceFieldsElement);

		protected SelectElement SelectDestinationFieldsElement => new SelectElement(DestinationFieldsElement);

		protected SelectElement SelectAvailableProductions => new SelectElement(AvailableProductionsSelectElement);

		public PushToRelativityThirdPage(RemoteWebDriver driver) : base(driver)
		{
			WaitForPage();
			PageFactory.InitElements(driver, this);
		}

		public void SelectCopyNativeFiles(RelativityProviderModel.CopyNativeFilesEnum? mode)
		{
			if (mode == RelativityProviderModel.CopyNativeFilesEnum.PhysicalFiles)
			{
				SelectCopyPhysicalFilesElement.ClickEx(Driver);
			}
			else if (mode == RelativityProviderModel.CopyNativeFilesEnum.LinksOnly)
			{
				SelectCopyLinksOnlyElement.ClickEx(Driver);
			}
			else if (mode == RelativityProviderModel.CopyNativeFilesEnum.No)
			{
				SelectCopyNoFilesElement.ClickEx(Driver);
			}
		}

		public void SelectCopyImages(bool? mode)
		{
			if (!mode.HasValue)
			{
				return;
			}

			if (mode.Value)
			{
				SelectCopyImagesYesElement.ClickEx(Driver);
			}
			else
			{
				SelectCopyImagesNoElement.ClickEx(Driver);
			}
			WaitForPage();
		}

		public PushToRelativityThirdPage MapAllFields()
		{
			MapAllFieldsElement.ClickEx(Driver);
			WaitForPage();
			return this;
		}

		public PushToRelativityThirdPage MapFieldsFromSavedSearch()
		{
			MapFieldsFromSavedSearchButton.ClickEx(Driver);
			WaitForPage();
			return this;
		}

		public string SelectOverwrite
		{
			get { return SelectOverwriteElement.SelectedOption.Text; }
			set { SelectOverwriteElement.SelectByTextEx(value, Driver); }
		}

		public string SelectMultiSelectFieldOverlayBehavior
		{
			get { return SelectMultiSelectFieldOverlayBehaviorElement.SelectedOption.Text; }
			set { SelectMultiSelectFieldOverlayBehaviorElement.SelectByTextEx(value, Driver); }
		}

		public string SelectFolderPathInfo
		{
			get { return SelectUseFolderPathElement.SelectedOption.Text; }
			set { SelectUseFolderPathElement.SelectByTextEx(value, Driver); }
		}

		public string SelectReadFromField
		{
			get { return SelectReadFromFieldElement.SelectedOption.Text; }
			set { SelectReadFromFieldElement.SelectByTextEx(value, Driver); }
		}

		public string SelectImagePrecedence
		{
			get { return SelectImagePrecedenceElement.SelectedOption.Text; }
			set { SelectImagePrecedenceElement.SelectByTextEx(value, Driver); }
		}

		public string ProductionPrecedenceText => ProductionPrecedenceElement.Text;

		public string PageInfoMessageText => PageInfoMessage.Text;

		public void SelectMoveExistingDocuments(bool? mode)
		{
			if (!mode.HasValue)
			{
				return;
			}

			if (mode.Value)
			{
				SelectMoveExitstingDocumentsYesElement.ClickEx(Driver);
			}
			else
			{
				SelectMoveExitstingDocumentsNoElement.ClickEx(Driver);
			}
		}

		public void SelectCopyFilesToRepository(bool? mode)
		{
			if (!mode.HasValue)
			{
				return;
			}

			if (mode.Value)
			{
				SelectCopyFilesToRepositoryYesElement.ClickEx(Driver);
			}
			else
			{
				SelectCopyFilesToRepositoryNoElement.ClickEx(Driver);
			}
		}

		public void SelectProductionPrecedence(string productionName)
		{
			ChooseProductionPrecedenceBtn.ClickEx(Driver);
			Thread.Sleep(1000);
			IWebElement productionOption = SelectAvailableProductions.Options.Single(x => x.Text.Equals(productionName));
			var action = new OpenQA.Selenium.Interactions.Actions(Driver);
			action.DoubleClick(productionOption).Perform();
			AvailableProductionOkBtn.ClickEx(Driver);
		}

		public void SelectIncludeOriginalImagesIfNotProduced(bool? mode)
		{
			if (!mode.HasValue)
			{
				return;
			}

			if (mode.Value)
			{
				IncludeOriginalImagesIfNotProducedElement.ClickEx(Driver);
			}
		}

		private List<string> GetFieldsFromListBox(string boxId)
		{
			IWebElement elem = Driver.FindElementEx(By.Id(boxId));

			SelectElement selectList = new SelectElement(elem);
			IList<IWebElement> options = selectList.Options;
			return options.Select(option => option.GetAttribute("title")).ToList();
		}

		public List<string> GetFieldsFromSourceWorkspaceListBox()
		{
			string sourceWorkspaceListBoxId = "source-fields";
			return GetFieldsFromListBox(sourceWorkspaceListBoxId);
		}
		public List<string> GetFieldsFromSelectedSourceWorkspaceListBox()
		{
			string sourceWorkspaceListBoxId = "selected-source-fields";
			return GetFieldsFromListBox(sourceWorkspaceListBoxId);
		}
		public List<string> GetFieldsFromDestinationWorkspaceListBox()
		{
			string sourceWorkspaceListBoxId = "workspace-fields";
			return GetFieldsFromListBox(sourceWorkspaceListBoxId);
		}
		public List<string> GetFieldsFromSelectedDestinationWorkspaceListBox()
		{
			string sourceWorkspaceListBoxId = "selected-workspace-fields";
			return GetFieldsFromListBox(sourceWorkspaceListBoxId);
		}

		public void SelectSourceField(string fieldName)
		{
			SelectField(SelectSourceFieldsElement, AddSourceFieldElement, fieldName);
		}

		public void SelectWorkspaceField(string fieldName)
		{
			SelectField(SelectDestinationFieldsElement, AddWorkspaceFieldElement, fieldName);
		}

		protected void SelectField(SelectElement selectElement, IWebElement addFieldElement, string fieldName)
		{
			if (string.IsNullOrEmpty(fieldName))
			{
				return;
			}

			string textToSearchFor = fieldName + " [";  //We are adding [ bracket to make sure whole filed name is taken into account

			SelectOption(selectElement, textToSearchFor);

			addFieldElement.ClickEx(Driver);
		}

		public IWebElement GetElementById(string id)
		{
			return Driver.FindElementEx(By.Id(id));
		}

		public IWebElement GetElementByIdOrNull(string id)
		{
			return Driver.FindElementsEx(By.Id(id)).FirstOrDefault();
		}

		private void SelectOption(SelectElement selectElement, string textToSearchFor)
		{
			IWebElement option = selectElement.WrappedElement.FindElementEx(By.XPath($".//option[starts-with(normalize-space(.), \"{textToSearchFor}\")]"));

			if (!option.Selected)
			{
				option.ClickEx(Driver);
			}
		}

		public IntegrationPointDetailsPage SaveIntegrationPoint()
		{
			WaitForPage();
			SaveButton.ClickEx(Driver);
			Log.Information("SaveIntegrationPoint");
			return new IntegrationPointDetailsPage(Driver);
		}
		public PushToRelativityThirdPage ClickSaveButtonExpectPopup()
		{
			SaveButton.ClickEx(Driver);

			// wait for popup to show
			IWebElement _ = CancelBtn;

			return this;
		}
		public IntegrationPointDetailsPage ClearAndProceedOnInvalidMapping()
		{
			ClearAndProceedBtn.ClickEx(Driver);
			return new IntegrationPointDetailsPage(Driver);
		}
	}
}
