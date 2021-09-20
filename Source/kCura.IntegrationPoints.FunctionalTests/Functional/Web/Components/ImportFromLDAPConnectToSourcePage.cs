using Atata;
using Relativity.Testing.Framework.Web.Components;
using Relativity.Testing.Framework.Web.Triggers;
using Relativity.IntegrationPoints.Tests.Functional.Web.Models;

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

		[FindById("connectionPath")]
		[WaitForElement(WaitBy.Id, "connectionPath", Until.Visible)]
		[SwitchToFrame(nameof(ConfigurationFrame), TriggerEvents.BeforeAccess)]
		public TextInput<_> ConnectionPath { get; private set; }

		[FindById("s2id_authentication")]
		[WaitForElement(WaitBy.Id, "s2id_authentication", Until.Visible)]
		[SwitchToFrame(nameof(ConfigurationFrame), TriggerEvents.BeforeAccess)]
		public Select2<IntegrationPointAuthentication, _> Authentication { get; private set; }

		[FindById("connectionUsername")]
		[WaitForElement(WaitBy.Id, "connectionUsername", Until.Visible)]
		[SwitchToFrame(nameof(ConfigurationFrame), TriggerEvents.BeforeAccess)]
		public TextInput<_> Username { get; private set; }

		[FindById("connectionPassword")]
		[WaitForElement(WaitBy.Id, "connectionPassword", Until.Visible)]
		[SwitchToFrame(nameof(ConfigurationFrame), TriggerEvents.BeforeAccess)]
		public PasswordInput<_> Password { get; private set; }
	}
}
