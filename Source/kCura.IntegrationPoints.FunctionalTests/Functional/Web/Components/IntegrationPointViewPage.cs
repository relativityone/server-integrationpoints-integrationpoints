using Atata;
using Relativity.Testing.Framework.Web.Triggers;
using Relativity.Testing.Framework.Web.Components;

namespace Relativity.IntegrationPoints.Tests.Functional.Web.Components
{
	using _ = IntegrationPointViewPage;

	[UseExternalFrame]
	[WaitUntilOverlayMissing(TriggerEvents.Init, PresenceTimeout = 10, AbsenceTimeout = 30, ThrowOnPresenceFailure = false, ThrowOnAbsenceFailure = false)]
	internal class IntegrationPointViewPage : ExternalFramedPage<_>
	{
		[FindById("runId")]
		[WaitUntilOverlayMissing(TriggerEvents.BeforeAccess, PresenceTimeout = 10, AbsenceTimeout = 30, ThrowOnPresenceFailure = false, ThrowOnAbsenceFailure = false)]
		public ConsoleButton<IntegrationPointRunPopup, _> Run { get; private set; }

		[FindByTitle("Save as a Profile")]
		public Link<IntegrationPointSaveAsProfilePopup, _> SaveAsProfile { get; private set; }

		public StatusTab Status { get; private set; }
		
		public _ RunIntegrationPoint(string integrationPointName)
		{
			var runResult = Run.WaitTo.Within(60).BeVisible();
				
			var runResult1 = runResult.Run.ClickAndGo();
				
			return runResult1.OK.ClickAndGo()
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
	}
}
