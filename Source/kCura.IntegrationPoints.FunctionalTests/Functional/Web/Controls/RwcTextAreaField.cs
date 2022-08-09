using Atata;

namespace Relativity.IntegrationPoints.Tests.Functional.Web.Controls
{
    [FindByAttribute("label", TermCase.Title, OuterXPath = ".//rwc-text-area-field/self::", As = FindAs.ShadowHost)]
    [FindByXPath("div[@class = 'rwa-base-field view cell']")]
    internal class RwcTextAreaField<TOwner> : Content<string, TOwner> where TOwner : PageObject<TOwner>
    {
    }
}
