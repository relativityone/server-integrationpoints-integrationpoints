using Atata;

namespace Relativity.Testing.Framework.Web.Components
{
    public class RwcCustomModalLayout<TNavigateTo, TOwner> : ExternalFramedPopup<TOwner>
        where TNavigateTo : PageObject<TNavigateTo>
        where TOwner : RwcCustomModalLayout<TNavigateTo, TOwner>
    {

        [FindByXPath(new[] { "rwc-modal-layout" }, As = FindAs.ShadowHost)]
        public Button<TNavigateTo, TOwner> Ok { get; }

        [FindByXPath(new[] { "rwc-modal-layout" }, As = FindAs.ShadowHost)]
        public Button<TNavigateTo, TOwner> Cancel { get; }
    }
}