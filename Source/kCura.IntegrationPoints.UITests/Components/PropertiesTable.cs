using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using IntegrationPointsUITests.Common;
using IntegrationPointsUITests.Pages;
using OpenQA.Selenium;

namespace IntegrationPointsUITests.Components
{
    public class PropertiesTable : Page
    {
        protected IWebElement TitleLink => _parent != null ? _parent.FindElement(By.LinkText(_title)) : Driver.FindElement(By.LinkText(_title));
        protected IWebElement Table => _parent != null ? _parent.FindElement(By.Id(_tableId)) : Driver.FindElement(By.Id(_tableId));

        private readonly string _title;

        private readonly string _tableId;

        private readonly IWebElement _parent;

        public PropertiesTable(IWebDriver driver, string title, string tableId, IWebElement parent = null) : base(driver)
        {
            _title = title;
            _tableId = tableId;
            _parent = parent;
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
