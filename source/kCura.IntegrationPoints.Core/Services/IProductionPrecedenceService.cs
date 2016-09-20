using System.Collections.Generic;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Services
{
	public interface IProductionPrecedenceService
	{
		IEnumerable<ProductionPrecedenceDTO> GetProductionPrecedence(int workspaceArtifactID);
	}
}