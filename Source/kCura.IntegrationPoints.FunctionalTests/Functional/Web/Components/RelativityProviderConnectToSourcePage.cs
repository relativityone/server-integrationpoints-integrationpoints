using Atata;
using OpenQA.Selenium;
using System.Threading;
using Relativity.Testing.Framework.Web.Components;
using Relativity.Testing.Framework.Web.Triggers;
using Relativity.IntegrationPoints.Tests.Functional.Web.Models;
using Relativity.IntegrationPoints.Tests.Functional.Web.ControlSearch;
using Relativity.IntegrationPoints.Tests.Functional.Web.Controls;

namespace Relativity.IntegrationPoints.Tests.Functional.Web.Components
{
	using _ = RelativityProviderConnectToSourcePage;

	[UseExternalFrame]
	[WaitUntilOverlayMissing(TriggerEvents.BeforeAccess, AbsenceTimeout = 30, AppliesTo = TriggerScope.Children)]
	[WaitForJQueryAjax(TriggerEvents.Init)]

	internal class RelativityProviderConnectToSourcePage : WorkspacePage<_>
	{
		public Button<RelativityProviderMapFieldsPage, _> Next { get; private set; }

		[FindById("configurationFrame")]
		public Frame<_> ConfigurationFrame { get; private set; }

		[FindByPrecedingDivContent]
		[SwitchToFrame(nameof(ConfigurationFrame), TriggerEvents.BeforeAccess)]
		public Select2<RelativityProviderSources, _> Source { get; private set; }

		[FindByPrecedingDivContent]
		[SwitchToFrame(nameof(ConfigurationFrame), TriggerEvents.BeforeAccess)]
		public Select2<string, _> SavedSearch { get; private set; }

		[FindByPrecedingDivContent]
		[SwitchToFrame(nameof(ConfigurationFrame), TriggerEvents.BeforeAccess)]
		[WaitFor(Until.Visible, TriggerEvents.BeforeAccess, AbsenceTimeout = 20)]
		public Select2<string, _> ProductionSet { get; private set; }

		[FindByPrecedingDivContent]
		[SwitchToFrame(nameof(ConfigurationFrame), TriggerEvents.BeforeAccess)]
		public Select2<string, _> DestinationWorkspace { get; private set; }

		[FindById("location-span")]
		[SwitchToFrame(nameof(ConfigurationFrame), TriggerEvents.BeforeAccess)]
		public Control<_> SelectFolder { get; private set; }

		[FindByPrecedingDivContent]
		[SwitchToFrame(nameof(ConfigurationFrame), TriggerEvents.BeforeAccess)]
		public RadioButtonList<RelativityProviderDestinationLocations, _> Location { get; private set; }

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
	}
}
