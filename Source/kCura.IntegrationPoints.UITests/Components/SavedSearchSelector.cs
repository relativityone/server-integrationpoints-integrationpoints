using System;
using System.Threading;
using kCura.IntegrationPoints.UITests.Driver;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;

namespace kCura.IntegrationPoints.UITests.Components
{
	public class SavedSearchSelector
	{
		private IWebElement _savedSearch;
		private readonly RemoteWebDriver _driver;
		
		private IWebElement SavedSearch
		{
			get
			{
				if (_savedSearch == null)
				{
					_savedSearch = _driver.FindElementById("s2id_savedSearchSelector");
				}
				return _savedSearch;
			}
		}

		public SavedSearchSelector(RemoteWebDriver driver)
		{
			_driver = driver;
		}

		public void SelectSavedSearch(string savedSearch)
		{
			SavedSearch.ClickWhenClickable(TimeSpan.FromSeconds(1));
			string generatedSelect2Id = SavedSearch.FindElement(By.TagName("input")).GetAttribute("id");
			string inputId = $"{generatedSelect2Id}_search";
			IWebElement input = _driver.FindElementById(inputId);

			input.SendKeys(savedSearch);
			Thread.Sleep(TimeSpan.FromMilliseconds(750));
			input.SendKeys(Keys.Enter);
			Thread.Sleep(TimeSpan.FromMilliseconds(250));
		}

		public string GetSelectedSavedSearch()
		{
			return SavedSearch.Text;
		}
	}
}
