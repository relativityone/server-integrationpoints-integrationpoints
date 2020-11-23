using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.UITests.Driver;
using OpenQA.Selenium;

namespace kCura.IntegrationPoints.UITests.Components
{
	public class PropertiesTable : Component
	{
		private readonly string _title;
		
		protected IWebElement TitleLink => Parent.FindElementEx(By.XPath("../..")).FindElementEx(By.LinkText(_title));

		public PropertiesTable(IWebElement parent, string title, IWebDriver driver) : base(parent, driver)
		{
			_title = title;
		}

		public PropertiesTable Select()
		{
			TitleLink.ClickEx(Driver);
			return this;
		}

		public Dictionary<string, string> Properties
		{
			get
			{
				var properties = new Dictionary<string, string>();
				List<IWebElement> names = Parent.FindElementsEx(By.ClassName("dynamicViewFieldName"))
					.Where(e => e.Displayed)
					.ToList();
				List<IWebElement> values = Parent.FindElementsEx(By.ClassName("dynamicViewFieldValue"))
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
