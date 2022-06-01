using Atata;
using Relativity.Testing.Framework.Web.Triggers;
using Relativity.Testing.Framework.Web.Components;

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
			return Run.WaitTo.Within(60).BeVisible()
				.Run.ClickAndGo()
				.OK.ClickAndGo()
				.WaitUntilJobCompleted(integrationPointName);
		}

        public int GetTransferredItemsCount(string integrationPointName)
        {
            return int.Parse(Status.Table.Rows[y => y.Name == integrationPointName].ItemsTransferred.Content.Value);
        }

        public int GetTotalItemsCount(string integrationPointName)
        {
            return int.Parse(Status.Table.Rows[y => y.Name == integrationPointName].TotalItems.Content.Value);
        }

        public string GetJobStatus(string integrationPointName)
        {
            return Status.Table.Rows[y => y.Name == integrationPointName].JobStatus.Content;
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

        public class SummaryPage : EditSection<_>
        {
            public ItemTable<Row, _> Table { get; private set; }

            public class Row : ItemTableRow<_>
            {
                public Text<_> Name { get; private set; }

                public Text<_> Type { get; private set; }

                public Text<_> LastRuntimeUTC { get; private set; }

                public Text<_> ItemsTransferred { get; private set; }

                public Text<_> TotalItems { get; private set; }
            }
		}
	}
}
