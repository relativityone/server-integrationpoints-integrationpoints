using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.UITests.Pages;
using OpenQA.Selenium;

namespace kCura.IntegrationPoints.UITests.Components
{
    public class StatusTable : Page
    {
        private readonly ISearchContext _parent;

        protected IWebElement Table => _parent.FindElement(By.ClassName("iFrameItemList"));

        protected IWebElement HeaderRow => Table.FindElement(By.ClassName("itemListHead"));

        public List<string> Headers => HeaderRow.FindElements(By.TagName("th"))
            .Select(element => element.Text)
            .ToList();

        public IWebElement this[int row] => Table.FindElements(By.CssSelector(".itemTable > tbody tr"))[row];

        public string this[int row, string column] => this[row]
            .FindElements(By.TagName("td"))
            .Select(element => element.Text)
            .ToList()[Headers.IndexOf(column)];

        public StatusTable(IWebDriver driver, ISearchContext parent = null) : base(driver)
        {
            _parent = parent ?? driver;
        }

    }
}
