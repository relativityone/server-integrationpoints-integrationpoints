using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data.Transformer;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class IntegrationPointRepository : IIntegrationPointRepository
	{
		private readonly IGenericLibrary<IntegrationPoint> _integrationPointLibrary;

		public IntegrationPointRepository(IHelper helper, int workspaceArtifactId)
			: this(new RsapiClientLibrary<IntegrationPoint>(helper, workspaceArtifactId))
		{
		}

		internal IntegrationPointRepository(IGenericLibrary<IntegrationPoint> integrationPointLibrary)
		{
			_integrationPointLibrary = integrationPointLibrary;
		}

		public IntegrationPointDTO Read(int artifactId)
		{
			IntegrationPoint integrationPoint = _integrationPointLibrary.Read(artifactId);

			return integrationPoint.ToDto();
		}

		public List<IntegrationPointDTO> Read(IEnumerable<int> artifactIds)
		{
			List<IntegrationPoint> integrationPoints = _integrationPointLibrary.Read(artifactIds);

			return integrationPoints.ToDto();
		}


	}
}
