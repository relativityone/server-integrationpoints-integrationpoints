using System;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Readers;

namespace kCura.IntegrationPoints.Core.Services.Exporter
{
    /// <summary>
    /// Provides public interface to RdoExporter
    /// </summary>
    public interface IExporterService : IDisposable
    {
        /// <summary>
        /// Indicates whether exporter still has data to returned
        /// </summary>
        bool HasDataToRetrieve { get; }

        /// <summary>
        /// Indicates the number of record found within the exporter
        /// </summary>
        int TotalRecordsFound { get; }

        /// <summary>
        /// Gets the reader of the exported data.
        /// </summary>
        /// <returns>DataReader to read export results</returns>
        IDataTransferContext GetDataTransferContext(IExporterTransferConfiguration transferConfiguration);

        /// <summary>
        /// Retrieves data from exporter with a give size
        /// </summary>
        /// <param name="size">the size of data to be returned</param>
        /// <returns>An array of ArtifactDTO object represents the result of data</returns>
        ArtifactDTO[] RetrieveData(int size);
    }
}
