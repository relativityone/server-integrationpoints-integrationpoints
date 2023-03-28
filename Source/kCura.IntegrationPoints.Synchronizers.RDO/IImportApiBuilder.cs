using IImportAPI = kCura.Relativity.ImportAPI.IImportAPI;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
    /// <summary>
    /// Wraps ExtendedImportAPI static methods to enable mocking these methods in unit tests.
    /// </summary>
    public interface IImportApiBuilder
    {
        /// <summary>
        /// Returns an instance of <see cref="ImportAPI"/> class. It is a wrapper for ExtendedImportAPI.CreateByTokenProvider() static method.
        /// </summary>
        /// <param name="webServiceUrl">The web service url.</param>
        /// <param name="importBatchSize">The import batch size parameter.</param>
        IImportAPI CreateImportAPI(string webServiceUrl, int importBatchSize);
    }
}
