using System.Collections.Generic;
using System.Linq;
using OpenQA.Selenium;

namespace kCura.IntegrationPoints.UITests.Components
{
	public class PropertiesTable : Component
	{
		private readonly string _title;
		
		protected IWebElement TitleLink => Parent.FindElement(By.XPath("../..")).FindElement(By.LinkText(_title));

		public PropertiesTable(IWebElement parent, string title) : base(parent)
		{
			_title = title;
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
				List<IWebElement> names = Parent.FindElements(By.ClassName("dynamicViewFieldName"))
					.Where(e => e.Displayed)
					.ToList();
				List<IWebElement> values = Parent.FindElements(By.ClassName("dynamicViewFieldValue"))
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
