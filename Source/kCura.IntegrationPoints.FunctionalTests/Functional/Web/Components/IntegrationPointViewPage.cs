using Atata;
using Relativity.Testing.Framework.Web.Triggers;
using Relativity.Testing.Framework.Web.Components;
using OpenQA.Selenium;

namespace Relativity.IntegrationPoints.Tests.Functional.Web.Components
{
    using _ = IntegrationPointViewPage;

    [WaitUntilOverlayMissing(TriggerEvents.Init, PresenceTimeout = 10, AbsenceTimeout = 30, ThrowOnPresenceFailure = false, ThrowOnAbsenceFailure = false)]
	internal partial class IntegrationPointViewPage : WorkspacePage<_>
	{
		[WaitUntilEnabled]
        [WaitUntilOverlayMissing(TriggerEvents.BeforeAccess, PresenceTimeout = 10, AbsenceTimeout = 30, ThrowOnPresenceFailure = false, ThrowOnAbsenceFailure = false)]
		public Button<IntegrationPointRunPopup, _> Run { get; private set; }

		[FindByContent("Save as a Profile")]
		public Button<IntegrationPointSaveAsProfilePopup, _> SaveAsProfile { get; private set; }

		public JobHistoryStatusTab Status { get; private set; }

        public _ RunIntegrationPoint(string integrationPointName)
		{
			var integrationPointRunPopup = Run.WaitTo.Within(120).BeVisible()
                .Run.ClickAndGo();

			var integrationPointViewPage = integrationPointRunPopup.Ok.WaitTo.Within(120).BeVisible()
                .Ok.ClickAndGo();

			return integrationPointViewPage.WaitUntilJobCompleted(integrationPointName);
		}

		public int GetTransferredItemsCount(string integrationPointName)
        {
            return int.Parse(Status.Rows[y => y.Name == integrationPointName].ItemsTransferred.Content.Value);
        }

        public int GetTotalItemsCount(string integrationPointName)
        {
            return int.Parse(Status.Rows[y => y.Name == integrationPointName].TotalItems.Content.Value);
        }

        public string GetJobStatus(string integrationPointName)
        {
            return Status.Rows[y => y.Name == integrationPointName].JobStatus.Content;
        }

		private _ WaitUntilJobCompleted(string jobName = null)
		{
			return Status.Rows[y => y.Name == jobName].JobStatus.WaitTo.Within(600).Contain("Completed");
		}

		protected override void OnInitCompleted()
		{
			base.OnInitCompleted();

			IWebElement iframe = Driver.FindElement(By.Id("_externalPage"));
			Driver.SwitchTo().Frame(iframe);
		}
    }
}
