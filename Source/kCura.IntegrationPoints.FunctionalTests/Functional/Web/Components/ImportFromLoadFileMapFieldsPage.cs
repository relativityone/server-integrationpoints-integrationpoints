using Atata;
using Relativity.Testing.Framework.Web.Models;
using Relativity.Testing.Framework.Web.Triggers;
using Relativity.Testing.Framework.Web.Components;
using Relativity.IntegrationPoints.Tests.Functional.Web.Models;
using Relativity.IntegrationPoints.Tests.Functional.Web.ControlSearch;

namespace Relativity.IntegrationPoints.Tests.Functional.Web.Components
{
	using _ = ImportFromLoadFileMapFieldsPage;

	[UseExternalFrame]
	[WaitUntilOverlayMissing(TriggerEvents.BeforeAccess, PresenceTimeout = 4, AbsenceTimeout = 30, AppliesTo = TriggerScope.Children)]
	[WaitForJQueryAjax(TriggerEvents.Init)]
	internal class ImportFromLoadFileMapFieldsPage : WorkspacePage<_>
	{
		public Button<IntegrationPointViewPage, _> Save { get; private set; }

	}
}