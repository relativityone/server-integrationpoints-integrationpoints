using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.UITests.Logging;
using kCura.IntegrationPoints.UITests.Pages;
using OpenQA.Selenium;
using Serilog;

namespace kCura.IntegrationPoints.UITests.Components
{
	public class PropertiesTable : Page
	{
		
		private readonly ISearchContext _parent;

		private readonly string _tableId;

		private readonly string _title;
		
		protected IWebElement TitleLink => _parent.FindElement(By.LinkText(_title));

		protected IWebElement Table => _parent.FindElement(By.Id(_tableId));

		public PropertiesTable(IWebDriver driver, string title, string tableId, ISearchContext parent = null) : base(driver)
		{
			_title = title;
			_tableId = tableId;
			_parent = parent ?? driver;
		}

		public PropertiesTable Select()
		{
			TitleLink.Click();
			return this;
		}

		public Dictionary<string, string> Properties
		{
			get
			{
				var properties = new Dictionary<string, string>();
				List<IWebElement> names = Table.FindElements(By.ClassName("dynamicViewFieldName"))
					.Where(e => e.Displayed)
					.ToList();
				List<IWebElement> values = Table.FindElements(By.ClassName("dynamicViewFieldValue"))
					.Where(e => e.Displayed)
					.ToList();
				for (var i = 0; i < names.Count; ++i)
				{
					properties.Add(names[i].Text, values[i].Text);
				}
				return properties;
			}
		}
	}
}
