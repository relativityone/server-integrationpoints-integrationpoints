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
    public class SavedSearchDialog : Page // TODO logging
    {
        public SavedSearchDialog(IWebDriver driver) : base(driver)
        {
            Thread.Sleep(1000);
        }

        public void ChooseSavedSearch(string name)
        {
            IWebElement element = FindSavedSearchElement(name);
            if (element == null)
            {
                throw new PageException("Cannot find Saved Search element named " + name);
            }
            element.Click();
            IWebElement ok = Driver.FindElement(By.Id("saved-search-picker-ok-button"));
            ok.Click();
        }

        public IWebElement FindSavedSearchElement(string name)
        {
            IWebElement tree = Driver.FindElement(By.Id("saved-search-picker-browser-tree"));

            var nodes = new Stack<IWebElement>();

            AddChildrenToStack(tree, nodes);

            while (nodes.Any())
            {
                Console.WriteLine("Taking another node");
                IWebElement node = nodes.Pop();
                if (IsLeaf(node))
                {
                    Console.WriteLine("Leaf found");
                    if (Text(node).Equals(name))
                    {
                        Console.WriteLine("Node found");
                        IWebElement a = node.FindElement(By.TagName("a"));
                        return a;
                    }
                }
                else
                {
                    Console.WriteLine("Node found");
                    if (IsClosed(node))
                    {
                        Console.WriteLine("Opening node");
                        ToggleNode(node);
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

        public void ToggleNode(IWebElement node)
        {
            IWebElement icon = node.FindElement(By.CssSelector("i"));
            icon.Click();
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