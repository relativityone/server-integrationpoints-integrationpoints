using Atata;
using Relativity.Testing.Framework.Web.Triggers;
using Relativity.Testing.Framework.Web.Components;
using Relativity.IntegrationPoints.Tests.Functional.Web.ControlSearch;
using Relativity.IntegrationPoints.Tests.Functional.Web.Models;

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
		public Select2<string, _> WorkspaceDestinationFolder { get; private set; }

		[FindByPrecedingDivContent]
		[WaitFor]
		[SwitchToFrame(nameof(ConfigurationFrame), TriggerEvents.BeforeAccess)]
		public Select2<string, _> ImportSource { get; private set; }

		[FindById("s2id_dataFileEncodingSelector")]
		[WaitFor]
		public Select2<string, _> FileEncoding { get; set; }

		[FindById("s2id_import-column")]
		[WaitFor]
		public Select2<string, _> Column { get; set; }

		[FindById("ss2id_import-quote")]
		[WaitFor]
		public Select2<string, _> Quote { get; set; }
	}
}
