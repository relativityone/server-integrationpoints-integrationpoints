using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.PageObjects;
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

		[FindsBy(How = How.Id, Using = "mapFieldsBtn")]
		protected IWebElement MapAllFieldsElement { get; set; }

		[FindsBy(How = How.Id, Using = "overwrite")]
		protected IWebElement OverwriteElement { get; set; }

		[FindsBy(How = How.Id, Using = "folderPathInformationSelect")]
		protected IWebElement UseFolderPathElement;

		[FindsBy(How = How.Id, Using = "folderPath")]
		protected IWebElement ReadFromFieldElement { get; set; }

		protected SelectElement SelectOverwriteElement => new SelectElement(OverwriteElement);

		protected SelectElement SelectUseFolderPathElement => new SelectElement(UseFolderPathElement);

		protected SelectElement SelectReadFromFieldElement => new SelectElement(ReadFromFieldElement);
		
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

		public IntegrationPointDetailsPage SaveIntegrationPoint()
		{
			SaveButton.Click();
			return new IntegrationPointDetailsPage(Driver);
		}

	}
}
