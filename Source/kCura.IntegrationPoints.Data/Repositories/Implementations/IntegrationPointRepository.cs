using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Transformers;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class IntegrationPointRepository : IIntegrationPointRepository
	{
		private readonly IGenericLibrary<IntegrationPoint> _integrationPointLibrary;
	    private readonly IDtoTransformer<IntegrationPointDTO, IntegrationPoint> _dtoTransformer;

        public IntegrationPointRepository(IHelper helper, int workspaceArtifactId)
			: this(new RsapiClientLibrary<IntegrationPoint>(helper, workspaceArtifactId), 
                  new IntegrationPointTransformer(helper, workspaceArtifactId))
		{
		}

		/// <summary>
		/// To be used externally by unit tests only
		/// </summary>
		internal IntegrationPointRepository(IGenericLibrary<IntegrationPoint> integrationPointLibrary, IDtoTransformer<IntegrationPointDTO, IntegrationPoint> dtoTransformer)
		{
			_integrationPointLibrary = integrationPointLibrary;
		    _dtoTransformer = dtoTransformer;
		}

		public IntegrationPointDTO Read(int artifactId)
		{
			IntegrationPoint integrationPoint = _integrationPointLibrary.Read(artifactId);
            return _dtoTransformer.ConvertToDto(integrationPoint);

		}

		public List<IntegrationPointDTO> Read(IEnumerable<int> artifactIds)
		{
			List<IntegrationPoint> integrationPoints = _integrationPointLibrary.Read(artifactIds);
            return _dtoTransformer.ConvertToDto(integrationPoints);
		}
	}
}
