using Atata;
using Relativity.Testing.Framework.Web.Components;

namespace Relativity.IntegrationPoints.Tests.Functional.Web.Components
{
    using _ = IntegrationPointRunPopup;

    [FindByXPath("rwc-confirmation-modal-layout", As = FindAs.ShadowHost, TargetNames = new[] { nameof(Ok), nameof(Cancel) })]
    internal class IntegrationPointRunPopup : RwcConfirmationModalLayout<IntegrationPointViewPage, _>
    {
        public Button<IntegrationPointViewPage, _> Ok { get; private set; }

        public Button<IntegrationPointViewPage, _> Cancel { get; private set; }

        protected override void OnInit()
        {
            base.OnInit();

            Driver.SwitchTo().DefaultContent();
        }
    }
}
