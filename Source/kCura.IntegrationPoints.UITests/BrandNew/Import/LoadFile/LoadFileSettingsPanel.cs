using kCura.IntegrationPoints.UITests.Components;
using kCura.IntegrationPoints.UITests.Driver;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace kCura.IntegrationPoints.UITests.BrandNew.Import.LoadFile
{
	public class LoadFileSettingsPanel : Component
	{
		protected IWebElement ImportTypeSelect => Parent.FindElement(By.Id("import-importType"));

		protected IWebElement WorkspaceDestinationTreeParent =>
			Parent.FindElement(By.XPath("//*[@id='destination-location-select']/.."));

		protected TreeSelect WorkspaceDestinationTree => new TreeSelect(WorkspaceDestinationTreeParent,
			"destination-location-select", "destination-jstree-holder-div");

		protected IWebElement ImportSourceTreeParent => Parent.FindElement(By.XPath("//*[@id='location-select']/.."));

		protected TreeSelect ImportSourceTree =>
			new TreeSelect(ImportSourceTreeParent, "location-select", "jstree-holder-div");

		protected IWebElement StartLineInput => Parent.FindElement(By.Id("import-columnname-numbers"));

		public LoadFileSettingsPanel(IWebElement parent) : base(parent)
		{
		}

		public string ImportType
		{
			get { return new SelectElement(ImportTypeSelect).SelectedOption.Text; }
			set
			{
				if (value != null)
				{
					new SelectElement(ImportTypeSelect).SelectByText(value);
				}
			}
		}

		public string WorkspaceDestination
		{
			set { WorkspaceDestinationTree.ChooseChildElement(value); }
		}

		public string ImportSource
		{
			set { ImportSourceTree.ChooseChildElement(value); }
		}

		public int StartLine
		{
			get { return int.Parse(StartLineInput.Text); }
			set { StartLineInput.SetText(value.ToString()); }
		}
	}
}