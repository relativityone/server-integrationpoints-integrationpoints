using Atata;
using Relativity.Testing.Framework.Web.Triggers;

namespace Relativity.IntegrationPoints.Tests.Functional.Web.Controls
{
    [FindByAttribute("label", TermCase.Title, OuterXPath = ".//rwc-radio-button-group-field/self::", As = FindAs.ShadowHost)]
    [FindByXPath("div[@class = 'rwa-base-field view cell']")]
    internal class RwcRadioButtonGroupField<TOwner> : Content<string, TOwner> where TOwner : PageObject<TOwner>
    {
    }
}
