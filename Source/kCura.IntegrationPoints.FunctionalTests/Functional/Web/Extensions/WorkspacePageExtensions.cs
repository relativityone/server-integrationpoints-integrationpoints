using System.Threading;
using OpenQA.Selenium;
using Relativity.IntegrationPoints.Tests.Functional.Web.Interfaces;
using Relativity.Testing.Framework.Web.Components;

namespace Relativity.IntegrationPoints.Tests.Functional.Web.Extensions
{
    public static class WorkspacePageExtensions
    {
        public static T SetTreeItem<T>(this T page, params string[] itemNames)
            where T: WorkspacePage<T>, IHasTreeItems<T>
        {
            var item = page.GetScope();
            string hierarchy = string.Empty;

            foreach (var itemName in itemNames)
            {
                string xpath = $"{hierarchy}//a[@role='treeitem'][.='{itemName}']";
                hierarchy = $"{xpath}/..";

                var textItem = item.FindElement(By.XPath(xpath));
                textItem.Click();
                Thread.Sleep(1000);
                item = item.FindElement(By.XPath(hierarchy));
            }

            return page.Owner;
        }
    }
}
