using System.Security;

namespace kCura.IntegrationPoint.Tests.Core.Models.Import.Ldap
{
    public class ImportFromLdapSourceConnectionModel
    {
        public string ConnectionPath { get; set; }

        public string ObjectFilterString { get; set; }

        public LdapAuthenticationType Authentication { get; set; }

        public SecureString Username { get; set; }

        public SecureString Password { get; set; }

        public bool ImportNestedItems { get; set; } = false;

    }
}
