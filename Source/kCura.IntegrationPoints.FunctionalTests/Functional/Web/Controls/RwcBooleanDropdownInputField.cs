using Atata;
using Relativity.Testing.Framework.Web.Triggers;

namespace Relativity.IntegrationPoints.Tests.Functional.Web.Controls
{
    [FindByAttribute("label", TermCase.Title, OuterXPath = ".//rwc-boolean-dropdown-input-field/self::", As = FindAs.ShadowHost)]
    [FindByXPath("div[@class = 'rwa-base-field view cell']")]
    internal class RwcBooleanDropdownInputField<TOwner> : Content<string, TOwner> where TOwner : PageObject<TOwner>
    {
    }
}
