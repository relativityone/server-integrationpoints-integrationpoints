using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.Core.Services.Exporter
{
	/// <summary>
	/// Provides public interface to RdoExporter
	/// </summary>
	public interface IExporterService
	{
		ArtifactDTO[] RetrieveData(int size);

		bool HasDataToRetrieve { get; }

		int TotalRecordsToImport { get; }

		int TotalRecordsFound { get; }
	}
}
