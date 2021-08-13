using Atata;
using OpenQA.Selenium;
using System.Threading;
using Relativity.Testing.Framework.Web.Components;
using Relativity.Testing.Framework.Web.Triggers;
using Relativity.IntegrationPoints.Tests.Functional.Web.Models;
using Relativity.IntegrationPoints.Tests.Functional.Web.ControlSearch;

namespace Relativity.IntegrationPoints.Tests.Functional.Web.Components
{
    using _ = ImportFromLDAPConnectToSourcePage;

    [UseExternalFrame]
    [WaitUntilOverlayMissing(TriggerEvents.BeforeAccess, AbsenceTimeout = 30, AppliesTo = TriggerScope.Children)]
    [WaitForJQueryAjax(TriggerEvents.Init)]
    internal class ImportFromLDAPConnectToSourcePage : WorkspacePage<_>
    {
		public Button<ImportFromLDAPMapFieldsPage, _> Next { get; private set; }

		[FindById("configurationFrame")]
		public Frame<_> ConfigurationFrame { get; private set; }

		[FindByPrecedingDivContent]
		[SwitchToFrame(nameof(ConfigurationFrame), TriggerEvents.BeforeAccess)]
		public TextInput<_> ConnectionPath { get; private set; }

		[FindByPrecedingDivContent]
		[SwitchToFrame(nameof(ConfigurationFrame), TriggerEvents.BeforeAccess)]
		public Select2<IntegrationPointAuthentication, _> Authentication { get; private set; }

		[FindByPrecedingDivContent]
		[SwitchToFrame(nameof(ConfigurationFrame), TriggerEvents.BeforeAccess)]
		public TextInput<_> Username { get; private set; }

		[FindByPrecedingDivContent]
		[SwitchToFrame(nameof(ConfigurationFrame), TriggerEvents.BeforeAccess)]
		public PasswordInput<_> Password { get; private set; }

		[FindByXPath("ul[contains(@class,'jstree-container-ul')]", Visibility = Visibility.Visible)]
		[SwitchToFrame(nameof(ConfigurationFrame), TriggerEvents.BeforeAccess)]
		public UnorderedList<TreeItemControl<_>, _> TreeItems { get; private set; }

		[FindById("select2-chosen-4")]
		[SwitchToFrame(nameof(ConfigurationFrame), TriggerEvents.BeforeAccess)]
		public Clickable<_> DestinationWorkspaceDropDown { get; private set; }

		public _ SetItem(params string[] itemNames)
		{
			var item = TreeItems[0].GetScope();
			string ierarhy = string.Empty;

			foreach (var itemName in itemNames)
			{
				string xpath = $"{ierarhy}//li[@role='treeitem']/a[.='{itemName}']";
				ierarhy = $"{xpath}/..";

				var textItem = item.FindElement(By.XPath(xpath));
				textItem.Click();
				Thread.Sleep(1000);
				item = Driver.FindElement(By.XPath(ierarhy));
			}

			return Owner;
		}

		[ControlDefinition("li[@role='treeitem']")]
		[WaitUntilOverlayMissing(TriggerEvents.BeforeClick, AppliesTo = TriggerScope.Children)]
		public class TreeItemControl<TPage> : Control<TPage>
			where TPage : PageObject<TPage>
		{
			[FindByClass("jstree-icon")]
			private Clickable<TPage> TreeIcon { get; set; }

			[FindByXPath("a")]
			public Text<TPage> Text { get; private set; }

			public UnorderedList<TreeItemControl<TPage>, TPage> Children { get; private set; }
		}
	}
}
