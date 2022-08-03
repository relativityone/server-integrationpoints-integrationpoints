namespace kCura.IntegrationPoints.Synchronizers.RDO.Entity
{
    public interface IEntityManagerLinksSanitizer
    {
        string ManagerLinksFieldIdentifier { get; }

        string SanitizeManagerReferenceLink(string managerLink);
    }
}
