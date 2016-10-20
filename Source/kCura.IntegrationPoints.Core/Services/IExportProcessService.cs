using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Services
{
	public interface IExportInitProcessService
	{
		int CalculateDocumentCountToTransfer(ExportUsingSavedSearchSettings exportSettings);
	}
}
