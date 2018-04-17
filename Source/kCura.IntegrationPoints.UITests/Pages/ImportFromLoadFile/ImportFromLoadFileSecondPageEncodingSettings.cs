using kCura.IntegrationPoint.Tests.Core.Models.ImportFromLoadFile;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.PageObjects;

namespace kCura.IntegrationPoints.UITests.Pages.ImportFromLoadFile
{
	public class ImportFromLoadFileSecondPageEncodingSettings : ImportFromLoadFileSecondPagePanel
	{
		[FindsBy(How = How.Id, Using = "dataFileEncodingSelector")]
		protected IWebElement FileEncodingSelectWebElement { get; set; }

		[FindsBy(How = How.Id, Using = "import-column")]
		protected IWebElement ColumnSelectWebElement { get; set; }

		[FindsBy(How = How.Id, Using = "import-quote")]
		protected IWebElement QuoteSelectWebElement { get; set; }

		[FindsBy(How = How.Id, Using = "import-newline")]
		protected IWebElement NewlineSelectWebElement { get; set; }

		[FindsBy(How = How.Id, Using = "import-multiValue")]
		protected IWebElement MultiValueSelectWebElement { get; set; }

		[FindsBy(How = How.Id, Using = "import-nestedValue")]
		protected IWebElement NestedValueSelectWebElement { get; set; }

		public ImportFromLoadFileSecondPageEncodingSettings(RemoteWebDriver driver) : base(driver)
		{
		}

		protected SelectElement FileEncodingSelectElement => new SelectElement(FileEncodingSelectWebElement);

		public string FileEncoding
		{
			get { return FileEncodingSelectElement.SelectedOption.Text; }
			set
			{
				if (value != null)
				{
					FileEncodingSelectElement.SelectByText(value);
				}
			}
		}

		protected SelectElement ColumnSelectElement => new SelectElement(ColumnSelectWebElement);

		public int Column
		{
			set
			{
				int index = value - 1;
				ColumnSelectElement.SelectByIndex(index);
			}
		}

		protected SelectElement QuoteSelectElement => new SelectElement(QuoteSelectWebElement);

		public int Quote
		{
			set
			{
				int index = value - 1;
				QuoteSelectElement.SelectByIndex(index);
			}
		}

		protected SelectElement NewlineSelectElement => new SelectElement(NewlineSelectWebElement);

		public int Newline
		{
			set
			{
				int index = value - 1;
				NewlineSelectElement.SelectByIndex(index);
			}
		}

		protected SelectElement MultiValueSelectElement => new SelectElement(MultiValueSelectWebElement);

		public int MultiValue
		{
			set
			{
				int index = value - 1;
				MultiValueSelectElement.SelectByIndex(index);
			}
		}

		protected SelectElement NestedValueSelectElement => new SelectElement(NestedValueSelectWebElement);

		public int NestedValue
		{
			set
			{
				int index = value - 1;
				NestedValueSelectElement.SelectByIndex(index);
			}
		}

		public override void SetupModel(ImportFromLoadFileModel model)
		{
			FileEncoding = model.FileEncoding.FileEncoding;
			Column = model.FileEncoding.Column;
			Quote = model.FileEncoding.Quote;
			Newline = model.FileEncoding.Newline;
			MultiValue = model.FileEncoding.MultiValue;
			NestedValue = model.FileEncoding.NestedValue;
		}
	}
}
