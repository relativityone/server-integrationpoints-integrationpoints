using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using kCura.IntegrationPoints.UITests.Driver;
using kCura.IntegrationPoints.UITests.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.PageObjects;
using Serilog;
using ByChained = SeleniumExtras.PageObjects.ByChained;

namespace kCura.IntegrationPoints.UITests.Components
{
	public class TreeSelect : Component
	{
		private readonly string _selectDivId;
		private readonly string _treeDivId;

		protected static readonly ILogger Log = LoggerFactory.CreateLogger(typeof(TreeSelect));

		public TreeSelect(IWebElement parent, string selectDivId, string treeDivId, IWebDriver driver) : base(parent, driver)
		{
			_selectDivId = selectDivId;
			_treeDivId = treeDivId;
		}

		public TreeSelect Expand()
		{
			WebDriverWait wait = Driver.GetConfiguredWait();

			wait.Until(d =>
			{
				IWebElement select = Parent.FindElement(By.Id(_selectDivId));
				select.Click();

				IWebElement expandedDiv = d.FindElement(By.Id(_treeDivId));
				
				return expandedDiv.GetCssValue("display") != "none";
			});

			return this;
		}

		public TreeSelect ChooseRootElement()
		{
			Expand();

			SelectNthElement(0);

			return this;
		}

		private void SelectNthElement(int i)
		{
			Driver.GetConfiguredWait().Until(d =>
			{
				try
				{
					d.FindElements(By.XPath($@".//div[@id='{_treeDivId}']//a"))[i].Click();
					return true;
				}
				catch (ArgumentOutOfRangeException)
				{
					return false;
				}
			});
		}

		public TreeSelect ChooseFirstChildElement()
		{
			Expand();

			SelectNthElement(1);

			return this;
		}

		public TreeSelect ChooseChildElement(string name)
		{
			Expand();

			// for some reason this needs retry

			IWebElement folderIcon;
			do
			{
				folderIcon = GetNode(name);
			} while (folderIcon == null);

			folderIcon.ScrollIntoView(Driver);

			folderIcon.ClickEx(Driver);

			return this;
		}

		private IWebElement GetNode(string name)
		{
			By treeBy = By.Id(_treeDivId);
			By closedNodesBy = new ByChained(treeBy, By.CssSelector(".jstree-closed"), By.TagName("i"));


			IWebElement node = null;
			while (node == null)
			{
				try
				{
					bool needsToExpand = false;
					node = Driver.FindElementsEx(new ByChained(treeBy, By.CssSelector(".jstree-anchor")))
						.FirstOrDefault(el => el.Text == name);
					if (node == null)
					{
						ICollection<IWebElement> closedNodes = Driver.FindElementsEx(closedNodesBy);
						if (closedNodes.Count == 0)
						{
							break;
						}

						WebDriverWait wait = Driver.GetConfiguredWait();

						wait.Until(d =>
						{
							try
							{
								// d.FindElement(closedNodesBy).ScrollIntoView(d);
								d.FindElement(closedNodesBy).Click();

								return true;
							}
							catch (StaleElementReferenceException)
							{
								return false;
							}
							catch (WebDriverTimeoutException)
							{
								return false;
							}
							catch (ElementNotInteractableException)
							{
								needsToExpand = true;
								return true;
							}
						});
					}

					if (needsToExpand)
					{
						Expand();
					}
				}
				catch (StaleElementReferenceException)
				{
					continue;
				}
			};

			return node;
		}
	}
}