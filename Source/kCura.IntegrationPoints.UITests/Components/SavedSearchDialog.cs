using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using kCura.IntegrationPoints.UITests.Common;
using kCura.IntegrationPoints.UITests.Driver;
using kCura.IntegrationPoints.UITests.Logging;
using OpenQA.Selenium;
using Serilog;

namespace kCura.IntegrationPoints.UITests.Components
{
	public class SavedSearchDialog : Component
	{

		private static readonly ILogger Log = LoggerFactory.CreateLogger(typeof(SavedSearchDialog));

		public SavedSearchDialog(IWebElement parent) : base(parent)
		{
			Thread.Sleep(1000);
		}

		public void ChooseSavedSearch(string name)
		{
			IWebElement element = FindSavedSearchElement(name);
			if (element == null)
			{
				throw new PageException($"Cannot find Saved Search element named {name}.");
			}
			element.ClickEx();
			IWebElement ok = Parent.FindElement(By.Id("saved-search-picker-ok-button"));
			ok.ClickEx();
		}

		public IWebElement FindSavedSearchElement(string name)
		{
			IWebElement tree = Parent.FindElement(By.Id("saved-search-picker-browser-tree"));

			var nodes = new Stack<IWebElement>();

			AddChildrenToStack(tree, nodes);

			while (nodes.Any())
			{
				Log.Verbose("Taking another node");
				IWebElement node = nodes.Pop();
				if (IsLeaf(node))
				{
					Log.Verbose("Leaf found");
					if (Text(node).Equals(name))
					{
						Log.Verbose("Node found");
						IWebElement a = node.FindElement(By.TagName("a"));
						return a;
					}
				}
				else
				{
					Log.Verbose("Node found");
					if (IsClosed(node))
					{
						Log.Verbose("Opening node");
						node = ToggleNode(node);
					}
					AddChildrenToStack(node, nodes);
				}
			}
			return null;
		}

		private void AddChildrenToStack(IWebElement parent, Stack<IWebElement> nodes)
		{
			foreach (IWebElement webElement in GetChildren(parent))
			{
				nodes.Push(webElement);
			}
		}

		public IWebElement ToggleNode(IWebElement node)
		{
			string nodeId = node.GetAttribute("id");

			IWebElement icon = node.FindElement(By.CssSelector("i"));
			icon.ClickEx();

			return SelectNode(nodeId);
		}

		private IWebElement SelectNode(string nodeId)
		{
			IWebElement newNode;
			int i = 0;
			int numberOfRetries = 10;
			do
			{
				i++;
				Thread.Sleep(TimeSpan.FromMilliseconds(250));
				newNode = Parent.FindElement(By.Id(nodeId));
			}
			while (newNode == null && i < numberOfRetries);
			return newNode;
		}

		public string Text(IWebElement li)
		{
			IWebElement a = li.FindElement(By.CssSelector("a"));
			return a.Text;
		}

		public ReadOnlyCollection<IWebElement> GetChildren(IWebElement li)
		{
			return li.FindElements(By.CssSelector("ul > li"));
		}

		public bool IsLeaf(IWebElement element)
		{
			return element.GetAttribute("class").Contains("jstree-leaf");
		}

		public bool IsClosed(IWebElement element)
		{
			return element.GetAttribute("class").Contains("jstree-closed");
		}

	}
}