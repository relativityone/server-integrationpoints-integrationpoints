using System;
using System.Linq;
using kCura.IntegrationPoints.UITests.Driver;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;

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

		public void SelectSavedSearch(string savedSearchName)
		{
			SavedSearch.ClickWhenClickable(TimeSpan.FromSeconds(1));

			string selectFullId = ReadSelectFullId();
			IWebElement input = GetSelectInputElement(selectFullId);

			input.SendKeys(savedSearchName);
			WaitUntilSavedSearchIsHighlighted(selectFullId);
			input.SendKeys(Keys.Enter);
			WaitUntilSavedSearchDropdownIsClosed(input);
		}

		private string ReadSelectFullId()
		{
			return SavedSearch.FindElement(By.TagName("input")).GetAttribute("id");
		}

		private IWebElement GetSelectInputElement(string selectFullId)
		{
			string inputId = $"{selectFullId}_search";
			IWebElement input = _driver.FindElementById(inputId);
			return input;
		}

		private void WaitUntilSavedSearchIsHighlighted(string selectFullId)
		{
			// wait until class 'select2-highlighted' is present
			string numericId = GetSelectNumericId(selectFullId);

			string resultsContainerId = $"select2-results-{numericId}";
			IWebElement resultsContainer = _driver.FindElementById(resultsContainerId);
			
			var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
			wait.Until(IsSavedSearchHighlighted(resultsContainer));
		}

		private static string GetSelectNumericId(string selectFullId)
		{
			// selectFullId format: s2id_autogen{ID}
			int lengthOfPrefixInSelectFullId = "s2id_autogen".Length;
			return selectFullId.Substring(lengthOfPrefixInSelectFullId);
		}

		private void WaitUntilSavedSearchDropdownIsClosed(IWebElement inputElement)
		{
			// wait until li class 'select2-focused' is not present
			var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
			wait.Until(IsSavedSearchDropdownClosed(inputElement));
		}

		public string GetSelectedSavedSearch()
		{
			return SavedSearch.Text;
		}

		private Func<IWebDriver, bool> IsSavedSearchHighlighted(IWebElement resultContainer)
		{
			return driver => resultContainer.FindElements(By.ClassName("select2-highlighted")).Any();
		}

		private Func<IWebDriver, bool> IsSavedSearchDropdownClosed(IWebElement inputElement)
		{
			return driver => !inputElement.GetAttribute("class").Contains("select2-focused");
		}
	}
}
