using System.Linq;

namespace kCura.IntegrationPoints.Synchronizers.RDO.Entity
{
	public class EntityManagerLinksSanitizer : IEntityManagerLinksSanitizer
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
