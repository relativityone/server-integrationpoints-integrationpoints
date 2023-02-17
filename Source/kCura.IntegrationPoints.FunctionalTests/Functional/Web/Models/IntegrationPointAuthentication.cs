using Atata;

namespace Relativity.IntegrationPoints.Tests.Functional.Web.Models
{
    public enum IntegrationPointAuthentication
    {
        [Term("Select...")]
        Select,
        [Term("Anonymous")]
        Anonymous,
        [Term("FastBind")]
        FastBind,
        [Term("Secure Socket Layer")]
        SecureSocketLayer
    }
}
