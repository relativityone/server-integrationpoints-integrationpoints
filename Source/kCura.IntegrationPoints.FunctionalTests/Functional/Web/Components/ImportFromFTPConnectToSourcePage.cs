using Atata;
using Relativity.IntegrationPoints.Tests.Functional.Web.Models;
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

		[FindById("host")]
		[WaitForElement(WaitBy.Id, "host", Until.Visible)]
		[SwitchToFrame(nameof(ConfigurationFrame), TriggerEvents.BeforeAccess)]
		public TextInput<_> Host { get; private set; }

		[FindById("s2id_protocol")]
		[WaitForElement(WaitBy.Id, "s2id_protocol", Until.Visible)]
		[SwitchToFrame(nameof(ConfigurationFrame), TriggerEvents.BeforeAccess)]
		public Select2<FTPProtocol, _> Protocol { get; private set; }

		[FindById("port")]
		[WaitForElement(WaitBy.Id, "port", Until.Visible)]
		[SwitchToFrame(nameof(ConfigurationFrame), TriggerEvents.BeforeAccess)]
		public NumberInput<_> Port { get; private set; }

		[FindById("username")]
		[WaitForElement(WaitBy.Id, "username", Until.Visible)]
		[SwitchToFrame(nameof(ConfigurationFrame), TriggerEvents.BeforeAccess)]
		public TextInput<_> Username { get; private set; }

		[FindById("password")]
		[WaitForElement(WaitBy.Id, "password", Until.Visible)]
		[SwitchToFrame(nameof(ConfigurationFrame), TriggerEvents.BeforeAccess)]
		public PasswordInput<_> Password { get; private set; }

		[FindById("filename_prefix")]
		[WaitForElement(WaitBy.Id, "filename_prefix", Until.Visible)]
		[SwitchToFrame(nameof(ConfigurationFrame), TriggerEvents.BeforeAccess)]
		public TextInput<_> CSVFilePath { get; private set; }
	}
}
