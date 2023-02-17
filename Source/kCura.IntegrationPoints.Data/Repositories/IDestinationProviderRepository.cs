namespace kCura.IntegrationPoints.Data.Repositories
{
    /// <summary>
    /// Repository responsible for Destination Provider object
    /// </summary>
    public interface IDestinationProviderRepository : IRepository<DestinationProvider>
    {
        /// <summary>
        /// Gets the Destination Provider artifact id given a guid identifier
        /// </summary>
        /// <param name="destinationProviderGuidIdentifier">Guid identifier of Destination Provider type</param>
        /// <returns>Artifact id of the Destination Provider</returns>
        int GetArtifactIdFromDestinationProviderTypeGuidIdentifier(string destinationProviderGuidIdentifier);

        DestinationProvider ReadByProviderGuid(string providerGuid);
    }
}
