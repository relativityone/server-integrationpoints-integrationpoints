using System.Linq;
using System.Threading;
using kCura.IntegrationPoint.Tests.Core.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using SeleniumExtras.PageObjects;
using OpenQA.Selenium.Support.UI;

namespace kCura.IntegrationPoints.UITests.Pages
{
	public class PushToRelativityThirdPage : GeneralPage
	{
		[FindsBy(How = How.Id, Using = "save")]
		protected IWebElement SaveButton { get; set; }

		[FindsBy(How = How.Id, Using = "native-file-mode-radio-0")]
		protected IWebElement SelectCopyPhysicalFilesElement { get; set; }

		[FindsBy(How = How.Id, Using = "native-file-mode-radio-1")]
		protected IWebElement SelectCopyLinksOnlyElement { get; set; }

		[FindsBy(How = How.Id, Using = "native-file-mode-radio-2")]
		protected IWebElement SelectCopyNoFilesElement { get; set; }

		[FindsBy(How = How.Id, Using = "exportImages-radio-0")]
		protected IWebElement SelectCopyImagesYesElement { get; set; }

		[FindsBy(How = How.Id, Using = "exportImages-radio-1")]
		protected IWebElement SelectCopyImagesNoElement { get; set; }

		[FindsBy(How = How.Id, Using = "move-documents-radio-0")]
		protected IWebElement SelectMoveExitstingDocumentsYesElement { get; set; }

		[FindsBy(How = How.Id, Using = "move-documents-radio-1")]
		protected IWebElement SelectMoveExitstingDocumentsNoElement { get; set; }

		[FindsBy(How = How.Id, Using = "native-file-radio-0")]
		protected IWebElement SelectCopyFilesToRepositoryYesElement { get; set; }

		[FindsBy(How = How.Id, Using = "native-file-radio-1")]
		protected IWebElement SelectCopyFilesToRepositoryNoElement { get; set; }

		[FindsBy(How = How.Id, Using = "mapFieldsBtn")]
		protected IWebElement MapAllFieldsElement { get; set; }

		[FindsBy(How = How.Id, Using = "overwrite")]
		protected IWebElement OverwriteElement { get; set; }

		[FindsBy(How = How.Id, Using = "overlay-field-behavior")]
		protected IWebElement MultiSelectFieldOverlayBehaviorElement { get; set; }

		[FindsBy(How = How.Id, Using = "folderPathInformationSelect")]
		protected IWebElement UseFolderPathElement;

		[FindsBy(How = How.Id, Using = "image-production-precedence")]
		protected IWebElement ImagePrecedenceElement { get; set; }

		[FindsBy(How = How.Id, Using = "folderPath")]
		protected IWebElement ReadFromFieldElement { get; set; }

		[FindsBy(How = How.Id, Using = "source-fields")]
		protected IWebElement SourceFieldsElement { get; set; }

		[FindsBy(How = How.Id, Using = "add-source-field")]
		protected IWebElement AddSourceFieldElement { get; set; }

		[FindsBy(How = How.Id, Using = "workspace-fields")]
		protected IWebElement DestinationFieldsElement { get; set; }

		[FindsBy(How = How.Id, Using = "add-workspace-field")]
		protected IWebElement AddWorkspaceFieldElement { get; set; }

		[FindsBy(How = How.Id, Using = "image-production-precedence-button")]
		protected IWebElement ChooseProductionPrecedenceBtn { get; set; }

		[FindsBy(How = How.Id, Using = "image-include-original-images-checkbox")]
		protected IWebElement IncludeOriginalImagesIfNotProducedElement { get; set; }

		[FindsBy(How = How.Id, Using = "popup-list-available")]
		protected IWebElement AvailableProductionsSelectElement { get; set; }

		[FindsBy(How = How.Id, Using = "ok-button")]
		protected IWebElement AvailableProductionOkBtn { get; set; }

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
				SelectCopyPhysicalFilesElement.Click();
			}
			else if (mode == RelativityProviderModel.CopyNativeFilesEnum.LinksOnly)
			{
				SelectCopyLinksOnlyElement.Click();
			}
			else if (mode == RelativityProviderModel.CopyNativeFilesEnum.No)
			{
				SelectCopyNoFilesElement.Click();
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
				SelectCopyImagesYesElement.Click();
			}
			else
			{
				SelectCopyImagesNoElement.Click();
			}

			Thread.Sleep(200);
		}

		public PushToRelativityThirdPage MapAllFields()
		{
			MapAllFieldsElement.Click();
			return this;
		}

		public string SelectOverwrite
		{
			get { return SelectOverwriteElement.SelectedOption.Text; }
			set { SelectOverwriteElement.SelectByText(value); }
		}

		public string SelectMultiSelectFieldOverlayBehavior
		{
			get { return SelectMultiSelectFieldOverlayBehaviorElement.SelectedOption.Text; }
			set { SelectMultiSelectFieldOverlayBehaviorElement.SelectByText(value); }
		}

		public string SelectFolderPathInfo
		{
			get { return SelectUseFolderPathElement.SelectedOption.Text; }
			set { SelectUseFolderPathElement.SelectByText(value); }
		}

		public string SelectReadFromField
		{
			get { return SelectReadFromFieldElement.SelectedOption.Text; }
			set { SelectReadFromFieldElement.SelectByText(value); }
		}

		public string SelectImagePrecedence
		{
			get { return SelectImagePrecedenceElement.SelectedOption.Text; }
			set { SelectImagePrecedenceElement.SelectByText(value); }
		}

		public void SelectMoveExitstingDocuments(bool? mode)
		{
			if (!mode.HasValue)
			{
				return;
			}

			if (mode.Value)
			{
				SelectMoveExitstingDocumentsYesElement.Click();
			}
			else
			{
				SelectMoveExitstingDocumentsNoElement.Click();
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
				SelectCopyFilesToRepositoryYesElement.Click();
			}
			else
			{
				SelectCopyFilesToRepositoryNoElement.Click();
			}
		}

		public void SelectProductionPrecedence(string productionName)
		{
			ChooseProductionPrecedenceBtn.Click();
			IWebElement productionOption = SelectAvailableProductions.Options.Single(x => x.Text.Equals(productionName));
			var action = new OpenQA.Selenium.Interactions.Actions(Driver);
			action.DoubleClick(productionOption).Perform();
			AvailableProductionOkBtn.Click();
		}

		public void SelectIncludeOriginalImagesIfNotProduced(bool? mode)
		{
			if (!mode.HasValue)
			{
				return;
			}

			if (mode.Value)
			{
				IncludeOriginalImagesIfNotProducedElement.Click();
			}
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

		public IntegrationPointDetailsPage SaveIntegrationPoint()
		{
			SaveButton.Click();
			return new IntegrationPointDetailsPage(Driver);
		}

	}
}
