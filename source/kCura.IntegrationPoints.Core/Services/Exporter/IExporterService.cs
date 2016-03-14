using System.Data;
using kCura.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.Core.Services.Exporter
{
	/// <summary>
	/// Provides public interface to RdoExporter
	/// </summary>
	public interface IExporterService
	{
		IDataReader GetDataReader();

		ArtifactDTO[] RetrieveData(int size);

		bool HasDataToRetrieve { get; }

		int TotalRecordsToImport { get; }

		int TotalRecordsFound { get; }
	}
}