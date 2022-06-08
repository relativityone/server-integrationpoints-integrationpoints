using Atata;
using Relativity.Testing.Framework.Web.Components;
using Relativity.Testing.Framework.Web.Triggers;

namespace Relativity.IntegrationPoints.Tests.Functional.Web.Components
{
    using _ = IntegrationPointSaveAsProfilePopup;

    [FindByXPath("rwc-confirmation-modal-layout", As = FindAs.ShadowHost, TargetNames = new[] { nameof(SaveAsProfile), nameof(Cancel) })]
	internal class IntegrationPointSaveAsProfilePopup : RwcConfirmationModalLayout<IntegrationPointViewPage, _>
	{
		[FindByClass("rwa-input")]
		[WaitUntilOverlayMissing(TriggerEvents.BeforeAccess, PresenceTimeout = 10, AbsenceTimeout = 30, ThrowOnPresenceFailure = false, ThrowOnAbsenceFailure = false)]
		public TextInput<_> ProfileName { get; private set; }

		public Button<IntegrationPointViewPage, _> SaveAsProfile { get; private set; }

		public Button<IntegrationPointViewPage, _> Cancel { get; private set; }

		protected override void OnInit()
		{
			base.OnInit();

			Driver.SwitchTo().DefaultContent();
		}
	}
}
