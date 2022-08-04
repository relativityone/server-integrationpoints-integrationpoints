using kCura.IntegrationPoints.Synchronizers.RDO.Entity;
using System.Linq;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Services
{
    class OpenLDAPEntityManagerLinksSanitizer : IEntityManagerLinksSanitizer
    {
        public string ManagerLinksFieldIdentifier => "cn";

        public string SanitizeManagerReferenceLink(string managerLink)
        {
            string sanitizedManagerLink = managerLink.Split(',')
                .Single(x => x.StartsWith(ManagerLinksFieldIdentifier))
                .Split('=').Last();

            return sanitizedManagerLink;
        }
    }
}
