using Atata;
using Relativity.Testing.Framework.Web.Triggers;
using Relativity.Testing.Framework.Web.Components;

namespace Relativity.IntegrationPoints.Tests.Functional.Web.Components
{
	using _ = ImportFromLoadFileConnectToSourcePage;

	[UseExternalFrame]
	[WaitForJQueryAjax(TriggerEvents.Init)]
	internal class ImportFromLoadFileConnectToSourcePage : WorkspacePage<_>
	{
	}
}
