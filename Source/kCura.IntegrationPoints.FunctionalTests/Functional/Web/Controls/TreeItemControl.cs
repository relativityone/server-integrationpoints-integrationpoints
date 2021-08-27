using Atata;
using Relativity.Testing.Framework.Web.Triggers;

namespace Relativity.IntegrationPoints.Tests.Functional.Web.Controls
{
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
