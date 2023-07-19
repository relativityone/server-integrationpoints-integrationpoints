using Atata;
using Relativity.Testing.Framework.Web.Components;
using Relativity.Testing.Framework.Web.Triggers;

namespace Relativity.IntegrationPoints.Tests.Functional.Web.Components
{
    using _ = IntegrationPointSaveAsProfilePopup;

    [PageObjectDefinition("rwc-modal-layout", ComponentTypeName = "dialog", IgnoreNameEndings = "PopupWindow,Window,Popup,Modal,Dialog")]
    internal class IntegrationPointSaveAsProfilePopup : RwcCustomModalLayout<IntegrationPointViewPage, _>
    {
        [Term("Integration Point Profile Name")]
        public RwcTextInputField<string, _> IntegrationPointProfileName { get; private set; }

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
