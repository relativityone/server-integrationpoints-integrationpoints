using Atata;
using Relativity.Testing.Framework.Web.Components;
using Relativity.Testing.Framework.Web.Triggers;

namespace Relativity.IntegrationPoints.Tests.Functional.Web.Components
{
	using _ = ImportFromFTPConnectToSourcePage;

	[UseExternalFrame]
	[WaitUntilOverlayMissing(TriggerEvents.BeforeAccess, AbsenceTimeout = 30, AppliesTo = TriggerScope.Children)]
	[WaitForJQueryAjax(TriggerEvents.Init)]
	internal class ImportFromFTPConnectToSourcePage : WorkspacePage<_>
	{
		public Button<ImportFromLDAPMapFieldsPage, _> Next { get; private set; }

		[FindById("configurationFrame")]
		public Frame<_> ConfigurationFrame { get; private set; }
	}
}
