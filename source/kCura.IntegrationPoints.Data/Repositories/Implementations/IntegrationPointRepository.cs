using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data.Transformer;
using kCura.Relativity.Client;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class IntegrationPointRepository : IIntegrationPointRepository
	{
		private readonly IGenericLibrary<IntegrationPoint> _integrationPointLibrary;

		public IntegrationPointRepository(IRSAPIClient rsapiClient)
			: this(new RsapiClientLibrary<IntegrationPoint>(rsapiClient))
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
