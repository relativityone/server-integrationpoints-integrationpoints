﻿using System.Collections.Generic;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Services
{
	public interface IProductionService
	{
		IEnumerable<ProductionDTO> GetProductions(int workspaceArtifactID);
	}
}