using System.Collections.Generic;
using kCura.Apps.Common.Utils.Serializers;
using Relativity.API;

namespace kCura.IntegrationPoints.LDAPProvider
{
    public interface ILDAPServiceFactory
    {
        ILDAPService Create(IAPILog logger, ISerializer serializer, LDAPSettings settings, LDAPSecuredConfiguration securedConfiguration, List<string> fieldsToLoad = null);
    }
}
