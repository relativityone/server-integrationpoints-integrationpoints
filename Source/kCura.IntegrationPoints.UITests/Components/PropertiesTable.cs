using System.Collections.Generic;
using System.Linq;
using OpenQA.Selenium;

namespace kCura.IntegrationPoints.UITests.Components
{
	public class PropertiesTable : Component
	{
		private readonly string _tableId;

		private readonly string _title;
		
		protected IWebElement TitleLink => Parent.FindElement(By.LinkText(_title));

		protected IWebElement Table => Parent.FindElement(By.Id(_tableId));

		public PropertiesTable(ISearchContext parent, string title, string tableId) : base(parent)
		{
			_title = title;
			_tableId = tableId;
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
