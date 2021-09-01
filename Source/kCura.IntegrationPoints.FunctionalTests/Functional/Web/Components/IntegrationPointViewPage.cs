using Atata;
using Relativity.Testing.Framework.Web.Triggers;
using Relativity.Testing.Framework.Web.Components;
using System;

namespace Relativity.IntegrationPoints.Tests.Functional.Web.Components
{
	using _ = IntegrationPointViewPage;

	[WaitUntilOverlayMissing(TriggerEvents.Init, PresenceTimeout = 10, AbsenceTimeout = 30, ThrowOnPresenceFailure = false, ThrowOnAbsenceFailure = false)]
	internal class IntegrationPointViewPage : WorkspacePage<_>
	{
		[WaitUntilEnabled]
		[WaitUntilOverlayMissing(TriggerEvents.BeforeAccess, PresenceTimeout = 10, AbsenceTimeout = 30, ThrowOnPresenceFailure = false, ThrowOnAbsenceFailure = false)]
		public Link<IntegrationPointRunPopup, _> Run { get; private set; }

		[FindByTitle("Save as a Profile")]
		public Link<IntegrationPointSaveAsProfilePopup, _> SaveAsProfile { get; private set; }

		public StatusTab Status { get; private set; }
		
		public _ RunIntegrationPoint(string integrationPointName)
		{
			return this.Run.WaitTo.Within(60).BeVisible()
				.Run.ClickAndGo()
				.OK.ClickAndGo()
				.WaitUntilJobCompleted(integrationPointName);
		}

		public int GetTransferredItemsCount(string integrationPointName)
		{
			return Int32.Parse(this.Status.Table.Rows[y => y.Name == integrationPointName].ItemsTransferred.Content.Value);
		}

		private _ WaitUntilJobCompleted(string jobName = null)
		{
			return Status.Table.Rows[y => y.Name == jobName].JobStatus.WaitTo.Within(600).Contain("Completed");
		}

		public class StatusTab : EditSection<_>
		{
			public ItemTable<Row, _> Table { get; private set; }

			public class Row : ItemTableRow<_>
			{
				public Text<_> Name { get; private set; }

				public Text<_> JobType { get; private set; }

				public Text<_> JobStatus { get; private set; }

				public Text<_> ItemsTransferred { get; private set; }

				public Text<_> TotalItems { get; private set; }
			}
		}
	}
}
