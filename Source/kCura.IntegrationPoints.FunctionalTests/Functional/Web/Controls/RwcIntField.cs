using Atata;
using Relativity.Testing.Framework.Web.Components;

namespace Relativity.IntegrationPoints.Tests.Functional.Web.Controls
{
    public class RwcIntField<TOwner> : RwcTextField<int, TOwner> where TOwner : PageObject<TOwner>
    {
    }
}
