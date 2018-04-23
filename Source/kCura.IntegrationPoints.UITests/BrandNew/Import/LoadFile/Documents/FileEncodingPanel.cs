using kCura.IntegrationPoints.UITests.Components;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace kCura.IntegrationPoints.UITests.BrandNew.Import.LoadFile.Documents
{
	public class FileEncodingPanel : Component
	{
		protected SelectElement FileEncodingSelect =>
			new SelectElement(Parent.FindElement(By.Id("dataFileEncodingSelector")));

		protected SelectElement ColumnSelect => new SelectElement(Parent.FindElement(By.Id("import-column")));

		protected SelectElement QuoteSelect => new SelectElement(Parent.FindElement(By.Id("import-quote")));

		protected SelectElement NewlineSelect => new SelectElement(Parent.FindElement(By.Id("import-newline")));

		protected SelectElement MultiValueSelect => new SelectElement(Parent.FindElement(By.Id("import-multiValue")));

		protected SelectElement NestedValueSelect => new SelectElement(Parent.FindElement(By.Id("import-nestedValue")));


		public FileEncodingPanel(IWebElement parent) : base(parent)
		{
		}

		public string FileEncoding
		{
			get => FileEncodingSelect.SelectedOption.Text;
			set
			{
				if (value != null)
				{
					FileEncodingSelect.SelectByText(value);
				}
			}
		}

		public int Column
		{
			set => ColumnSelect.SelectByIndex(value - 1);
		}

		public int Quote
		{
			set => QuoteSelect.SelectByIndex(value - 1);
		}

		public int Newline
		{
			set => NewlineSelect.SelectByIndex(value - 1);
		}

		public int MultiValue
		{
			set => MultiValueSelect.SelectByIndex(value - 1);
		}

		public int NestedValue
		{
			set => NestedValueSelect.SelectByIndex(value - 1);
		}
	}
}