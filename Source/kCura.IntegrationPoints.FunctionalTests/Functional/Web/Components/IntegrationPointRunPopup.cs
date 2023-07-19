using Atata;
using Relativity.Testing.Framework.Web.Components;

namespace Relativity.IntegrationPoints.Tests.Functional.Web.Components
{
    using _ = IntegrationPointRunPopup;

    [PageObjectDefinition("rwc-modal-layout", ComponentTypeName = "dialog", IgnoreNameEndings = "PopupWindow,Window,Popup,Modal,Dialog")]
    internal class IntegrationPointRunPopup : RwcModalLayout<_>
    {
        [Term("Ok")]
        public Button<IntegrationPointViewPage, _> Ok { get; private set; }

        [Term("Cancel")]
        public Button<IntegrationPointViewPage, _> Cancel { get; private set; }

        protected override void OnInit()
        {
            base.OnInit();

            Driver.SwitchTo().DefaultContent();
        }
    }
}
