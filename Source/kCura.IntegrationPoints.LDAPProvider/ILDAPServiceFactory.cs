using System.Collections.Generic;
using Relativity.API;

namespace kCura.IntegrationPoints.LDAPProvider
{
    public interface ILDAPServiceFactory
    {
        ILDAPService Create(IAPILog logger, LDAPSettings settings, List<string> fieldsToLoad = null);
    }
}