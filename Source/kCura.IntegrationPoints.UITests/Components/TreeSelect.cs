using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using kCura.IntegrationPoints.UITests.Driver;
using kCura.IntegrationPoints.UITests.Logging;
using OpenQA.Selenium;
using Serilog;

namespace kCura.IntegrationPoints.UITests.Components
{
	public class TreeSelect : Component
	{
		private readonly string _selectDivId;
		private readonly string _treeDivId;

		protected static readonly ILogger Log = LoggerFactory.CreateLogger(typeof(TreeSelect));

		public TreeSelect(IWebElement parent, string selectDivId, string treeDivId) : base(parent)
		{
			_selectDivId = selectDivId;
			_treeDivId = treeDivId;
		}

		public TreeSelect Expand()
		{
			IWebElement select = Parent.FindElement(By.XPath($@".//div[@id='{_selectDivId}']"));
			Thread.Sleep(TimeSpan.FromMilliseconds(1000));
			select.ClickEx();
			return this;
		}

		public TreeSelect ChooseRootElement()
		{
			Expand();

			IWebElement selectListPopup = Parent.FindElement(By.XPath($@".//div[@id='{_treeDivId}']"));
			Thread.Sleep(TimeSpan.FromMilliseconds(1000));
			IWebElement rootElement = selectListPopup.FindElementsEx(By.XPath(@".//a"))[0];
			Thread.Sleep(TimeSpan.FromMilliseconds(1000));
			rootElement.ClickEx();
			return this;
		}

		public TreeSelect ChooseFirstChildElement()
		{
			Expand();

			IWebElement selectListPopup = Parent.FindElement(By.XPath($@".//div[@id='{_treeDivId}']"));
			Thread.Sleep(TimeSpan.FromMilliseconds(1000));
			IWebElement rootElement = selectListPopup.FindElementsEx(By.XPath(@".//a"))[1];
			Thread.Sleep(TimeSpan.FromMilliseconds(1000));
			rootElement.ClickEx();
			return this;
		}

		public TreeSelect ChooseChildElement(string name)
		{
			Expand();

			IWebElement tree = Parent.FindElement(By.XPath($@".//div[@id='{_treeDivId}']"));
			OpenAllNodes(tree);

			Thread.Sleep(TimeSpan.FromMilliseconds(2000));

			IWebElement folderIcon = tree.FindElementsEx(By.CssSelector(".jstree-anchor")).First(el => el.Text == name);
			folderIcon.ScrollIntoView();
			Thread.Sleep(TimeSpan.FromSeconds(1));
			folderIcon.ClickEx();

			return this;
		}

		private void OpenAllNodes(IWebElement tree)
		{
			while (true)
			{
				Thread.Sleep(TimeSpan.FromMilliseconds(1000));
				ICollection<IWebElement> closedNodes = tree.FindElementsEx(By.CssSelector(".jstree-closed"));
				if (closedNodes.Count == 0)
				{
					break;
				}
				foreach (var closedNode in closedNodes)
				{
					IWebElement button = closedNode.FindElementEx(By.TagName("i"));
					button.ScrollIntoView();
					button.ClickEx();
				}
			}
		}

	}
}