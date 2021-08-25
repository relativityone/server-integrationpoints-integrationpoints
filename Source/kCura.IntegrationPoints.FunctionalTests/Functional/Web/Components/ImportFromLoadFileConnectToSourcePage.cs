using Atata;
using Relativity.Testing.Framework.Web.Triggers;
using Relativity.Testing.Framework.Web.Components;
using Relativity.IntegrationPoints.Tests.Functional.Web.ControlSearch;
using Relativity.IntegrationPoints.Tests.Functional.Web.Models;
using OpenQA.Selenium;
using System.Threading;

namespace Relativity.IntegrationPoints.Tests.Functional.Web.Components
{
	using _ = ImportFromLoadFileConnectToSourcePage;

	[UseExternalFrame]
	[WaitUntilOverlayMissing(TriggerEvents.BeforeAccess, AbsenceTimeout = 30, AppliesTo = TriggerScope.Children)]
	[WaitForJQueryAjax(TriggerEvents.Init)]
	internal class ImportFromLoadFileConnectToSourcePage : WorkspacePage<_>
	{
		public Button<ImportFromLoadFileMapFieldsPage, _> Next { get; private set; }

		[FindById("configurationFrame")]
		public Frame<_> ConfigurationFrame { get; private set; }

		[FindByPrecedingDivContent]
		[WaitFor]
		[SwitchToFrame(nameof(ConfigurationFrame), TriggerEvents.BeforeAccess)]
		public Select2<IntegrationPointImportTypes, _> ImportType { get; private set; }

		[FindByPrecedingDivContent]
		[WaitFor]
		[SwitchToFrame(nameof(ConfigurationFrame), TriggerEvents.BeforeAccess)]
		public Control<_> WorkspaceDestinationFolder { get; set; }

		[FindByPrecedingDivContent]
		[WaitFor]
		[SwitchToFrame(nameof(ConfigurationFrame), TriggerEvents.BeforeAccess)]
		public Control<_> ImportSource { get; private set; }

		[FindByPrecedingDivContent]
		[WaitFor]
		[SwitchToFrame(nameof(ConfigurationFrame), TriggerEvents.BeforeAccess)]
		public Select2<string,_> Column { get; set; }

		[FindByPrecedingDivContent]
		[WaitFor]
		[SwitchToFrame(nameof(ConfigurationFrame), TriggerEvents.BeforeAccess)]
		public Select2<string, _> Quote { get; set; }


        [FindByXPath("ul[contains(@class,'jstree-container-ul')]", Visibility = Visibility.Visible)]
		[SwitchToFrame(nameof(ConfigurationFrame), TriggerEvents.BeforeAccess)]
		public UnorderedList<TreeItemControl<_>, _> TreeItems { get; private set; }

		public _ SetItem(params string[] itemNames)
		{
			var item = TreeItems[0].GetScope();
			string hierarhy = string.Empty;

			foreach (var itemName in itemNames)
			{
				string xpath = $"{hierarhy}//li[@role='treeitem']/a[.='{itemName}']";
				hierarhy = $"{xpath}/..";

				var textItem = item.FindElement(By.XPath(xpath));
				textItem.Click();
				Thread.Sleep(1000);
				item = Driver.FindElement(By.XPath(hierarhy));
			}

			return Owner;
		}

		[ControlDefinition("li[@role='treeitem']")]
		[WaitUntilOverlayMissing(TriggerEvents.BeforeClick, AppliesTo = TriggerScope.Children)]
		public class TreeItemControl<TPage> : Control<TPage>
			where TPage : PageObject<TPage>
		{
			[FindByXPath("a")]
			public Text<TPage> Text { get; private set; }

			public UnorderedList<TreeItemControl<TPage>, TPage> Children { get; private set; }
		}

	}
}
