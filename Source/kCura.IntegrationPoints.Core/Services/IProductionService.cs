using System.Collections.Generic;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Services
{
	public interface IProductionService
	{
		IEnumerable<ProductionDTO> GetProductionsForExport(int workspaceArtifactID);
		IEnumerable<ProductionDTO> GetProductionsForImport(int workspaceArtifactId);
	}
}