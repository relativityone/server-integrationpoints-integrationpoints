﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;

namespace kCura.IntegrationPoints.UITests.Components
{
	public class TreeSelect : Component
	{
		public TreeSelect(IWebElement parent) : base(parent)
		{
		}

		public TreeSelect Expand()
		{
			IWebElement select = Parent.FindElement(By.XPath(@".//div[@id='location-select']"));
			Thread.Sleep(TimeSpan.FromMilliseconds(200));
			select.Click();
			return this;
		}

		public TreeSelect ChooseRootElement()
		{
			Expand();

			IWebElement selectListPopup = Parent.FindElement(By.XPath(@".//div[@id='jstree-holder-div']"));
			Thread.Sleep(TimeSpan.FromMilliseconds(1000));
			IWebElement rootElement = selectListPopup.FindElements(By.XPath(@".//a"))[0];
			Thread.Sleep(TimeSpan.FromMilliseconds(1000));
			rootElement.Click();
			return this;
		}

		public TreeSelect ChooseFirstChildElement()
		{
			Expand();

			IWebElement selectListPopup = Parent.FindElement(By.XPath(@".//div[@id='jstree-holder-div']"));
			Thread.Sleep(TimeSpan.FromMilliseconds(1000));
			IWebElement rootElement = selectListPopup.FindElements(By.XPath(@".//a"))[1];
			Thread.Sleep(TimeSpan.FromMilliseconds(1000));
			rootElement.Click();
			return this;
		}

		public TreeSelect ChooseChildElement(string name)
		{
			Expand();

			IWebElement tree = Parent.FindElement(By.XPath(@".//div[@id='jstree-holder-div']"));
			OpenAllNodes(tree);

			Thread.Sleep(TimeSpan.FromMilliseconds(1000));

			IWebElement folderIcon = tree.FindElements(By.ClassName("jstree-anchor")).First(el => el.Text == name);
			folderIcon.Click();

			return this;
		}

		private void OpenAllNodes(IWebElement tree)
		{
			while (true)
			{
				Thread.Sleep(TimeSpan.FromMilliseconds(1000));
				ICollection<IWebElement> closedNodes = tree.FindElements(By.ClassName("jstree-closed"));
				if (closedNodes.Count == 0)
				{
					break;
				}
				foreach (var closedNode in closedNodes)
				{
					IWebElement button = closedNode.FindElement(By.TagName("i"));
					button.Click();
				}
			}
		}
	}
}