using System.Collections.Generic;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Process
{
	public interface IProductionPrecedenceService
	{
		IEnumerable<ProductionPrecedenceDTO> GetProductionPrecedence(int workspaceArtifactID);
	}
}