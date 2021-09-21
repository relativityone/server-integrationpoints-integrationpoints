using Atata;
using Relativity.IntegrationPoints.Tests.Functional.Web.Attributes;
using Relativity.Testing.Framework.Web.Models;
using Relativity.Testing.Framework.Web.Triggers;
using Relativity.Testing.Framework.Web.Components;
using Relativity.IntegrationPoints.Tests.Functional.Web.Models;

namespace Relativity.IntegrationPoints.Tests.Functional.Web.Components
{
	using _ = IntegrationPointEditPage;

	[UseExternalFrame]
	[WaitUntilOverlayMissing(TriggerEvents.Init, PresenceTimeout = 2)]
	internal class IntegrationPointEditPage : WorkspacePage<_>
	{
		[FindById("name")]
		public TextInput<_> Name { get; private set; }

		[FindById("isExportType")]
		[WaitForElement(WaitBy.Id, "isExportType", Until.Visible)]
		[WaitOnTrigger(1, TriggerEvents.AfterClickOrSet)]
		public RadioButtonList<IntegrationPointTypes, _> Type { get; private set; }

		[FindById("promoteEligible")]
		public RadioButtonList<YesNo, _> PromoteList { get; private set; }

		[FindById("s2id_sourceProvider")]
		public Select2<IntegrationPointSources, _> Source { get; private set; }

		[FindById("s2id_destinationProviderType")]
		public Select2<IntegrationPointDestinations, _> Destination { get; private set; }

		[FindById("s2id_destinationRdo")]
		public Select2<IntegrationPointTransferredObjects, _> TransferredObject { get; private set; }

		[FindById("notificationEmails")]
		public TextArea<_> EmailRecipients { get; private set; }

		#region Next Button

		[Term("Next")]
		public Button<RelativityProviderConnectToSourcePage, _> RelativityProviderNext { get; private set; }

		[Term("Next")]
		public Button<ImportFromLoadFileConnectToSourcePage, _> ImportFromLoadFileNext { get; private set; }

		[Term("Next")]
		public Button<ImportFromLDAPConnectToSourcePage, _> ImportFromLDAPNext { get; private set; }

		[Term("Next")]
		public Button<ExportToLoadFileConnectToSourcePage, _> ExportToLoadFileNext { get; private set; }

		[Term("Next")]
		public Button<ImportFromFTPConnectToSourcePage, _> ImportFromFTPNext { get; private set; }

		#endregion
	}
}
