namespace kCura.IntegrationPoints.Synchronizers.RDO.Entity
{
    public class EntityManagerLinksSanitizer : IEntityManagerLinksSanitizer
    {
        public string ManagerLinksFieldIdentifier => "distinguishedname";

        public string SanitizeManagerReferenceLink(string managerLink)
        {
            return managerLink;
        }
    }
}
