﻿using Atata;
using Relativity.Testing.Framework.Web.Models;
using Relativity.Testing.Framework.Web.Triggers;
using Relativity.Testing.Framework.Web.Components;
using Relativity.IntegrationPoints.Tests.Functional.Web.Models;
using Relativity.IntegrationPoints.Tests.Functional.Web.ControlSearch;

namespace Relativity.IntegrationPoints.Tests.Functional.Web.Components
{
	using _ = IntegrationPointEditPage;

	[UseExternalFrame]
	[WaitUntilOverlayMissing(TriggerEvents.Init, PresenceTimeout = 2)]
	internal class IntegrationPointEditPage : WorkspacePage<_>
	{
		[Term("Next")]
		public Button<RelativityProviderConnectToSourcePage, _> RelativityProviderNext { get; private set; }

		[Term("Next")]
		public Button<ImportFromLoadFileConnectToSourcePage, _> ImportFromLoadFileNext { get; private set; }

		[FindById("name")]
		[WaitForElement(WaitBy.Class, "loading", Until.MissingOrHidden, TriggerEvents.BeforeSet, AbsenceTimeout = 20)]
		public TextInput<_> Name { get; private set; }

		[FindByPrecedingDivContent]
		public RadioButtonList<IntegrationPointTypes, _> Type { get; private set; }

		[FindById("promoteEligible")]
		public RadioButtonList<YesNo, _> PromoteList { get; private set; }

		[FindByPrecedingDivContent]
		public Select2<IntegrationPointSources, _> Source { get; private set; }

		[FindByPrecedingDivContent]
		public Select2<IntegrationPointDestinations, _> Destination { get; private set; }

		[FindByPrecedingDivContent]
		public Select2<IntegrationPointTransferredObjects, _> TransferredObject { get; private set; }
	}
}