using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using IntegrationPointsUITests.Pages;
using OpenQA.Selenium;

namespace IntegrationPointsUITests.Components
{
    public class StatusTable : Page
    {
        private readonly IWebElement _parent;

        protected IWebElement Table => _parent != null
            ? _parent.FindElement(By.ClassName("iFrameItemList"))
            : Driver.FindElement(By.ClassName("iFrameItemList"));

        protected IWebElement HeaderRow => Table.FindElement(By.ClassName("itemListHead"));

        public List<string> Headers => HeaderRow.FindElements(By.TagName("th"))
            .Select(element => element.Text)
            .ToList();

        public IWebElement this[int row] => Table.FindElements(By.CssSelector(".itemTable > tbody tr"))[row];

        public string this[int row, string column] => this[row]
            .FindElements(By.TagName("td"))
            .Select(element => element.Text)
            .ToList()[Headers.IndexOf(column)];

        public StatusTable(IWebDriver driver, IWebElement parent = null) : base(driver)
        {
            _parent = parent;
        }

    }
}
