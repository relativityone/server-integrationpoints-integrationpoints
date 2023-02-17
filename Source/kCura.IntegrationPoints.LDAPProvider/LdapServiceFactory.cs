using System.Collections.Generic;
using kCura.Apps.Common.Utils.Serializers;
using Relativity.API;

namespace kCura.IntegrationPoints.LDAPProvider
{
    public class LdapServiceFactory : ILDAPServiceFactory
    {
        public ILDAPService Create(IAPILog logger, ISerializer serializer, LDAPSettings settings, LDAPSecuredConfiguration securedConfiguration, List<string> fieldsToLoad = null)
        {
            return new LDAPService(logger, serializer, settings, securedConfiguration, fieldsToLoad);
        }
    }
}
