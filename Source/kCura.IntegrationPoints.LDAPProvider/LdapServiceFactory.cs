using System.Collections.Generic;
using Relativity.API;

namespace kCura.IntegrationPoints.LDAPProvider
{
    public class LdapServiceFactory : ILDAPServiceFactory
    {
        public ILDAPService Create(IAPILog logger, LDAPSettings settings, List<string> fieldsToLoad = null)
        {
            return new LDAPService(logger, settings, fieldsToLoad);
        }
    }
}