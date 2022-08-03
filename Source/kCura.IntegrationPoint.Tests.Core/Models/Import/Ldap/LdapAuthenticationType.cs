using System.ComponentModel;

namespace kCura.IntegrationPoint.Tests.Core.Models.Import.Ldap
{
    public enum LdapAuthenticationType
    {
        [Description("Anonymous")]
        Anonymous,

        [Description("FastBind")]
        FastBind,

        [Description("Secure Socket Layer")]
        SecureSocketLayer
    }
}