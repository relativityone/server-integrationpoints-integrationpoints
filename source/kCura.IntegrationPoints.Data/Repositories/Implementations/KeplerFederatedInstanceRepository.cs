using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Contracts.RDO;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Domain.Models;
using Relativity;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class KeplerFederatedInstanceRepository : KeplerServiceBase, IFederatedInstanceRepository
	{
		public KeplerFederatedInstanceRepository(IObjectQueryManagerAdaptor objectQueryManagerAdaptor) 
			: base(objectQueryManagerAdaptor)
		{
		}
		public FederatedInstanceDto RetrieveFederatedInstance(int artifactId)
		{
			var query = new global::Relativity.Services.ObjectQuery.Query()
			{
				Condition = $"'Artifact ID' == {artifactId}",
				Fields = new [] {"Name", "Instance URL" },
				IncludeIdWindow = false,
				SampleParameters = null,
				RelationalField = null,
				SearchProviderCondition = null,
				TruncateTextFields = false
			};

			ArtifactDTO[] artifactDtos = null;

			try
			{
				artifactDtos = this.RetrieveAllArtifactsAsync(query).GetResultsWithoutContextSync();
			}
			catch (Exception e)
			{
				throw new Exception("Unable to retrieve federated instance", e);
			}

			return Convert(artifactDtos).FirstOrDefault();
		}

		public IEnumerable<FederatedInstanceDto> RetrieveAll()
		{
			var query = new global::Relativity.Services.ObjectQuery.Query()
			{
				Fields = new[] { "Name", "Instance URL" },
			};

			ArtifactDTO[] artifactDtos = null;
			try
			{
				artifactDtos = this.RetrieveAllArtifactsAsync(query).GetResultsWithoutContextSync();
			}
			catch (Exception e)
			{
				throw new Exception("Unable to retrieve federated instances", e);
			}

			return Convert(artifactDtos);
		}

		private IEnumerable<FederatedInstanceDto> Convert(IEnumerable<ArtifactDTO> artifactDtos)
		{
			var federatedInstances = new List<FederatedInstanceDto>();

			foreach (ArtifactDTO artifactDto in artifactDtos)
			{
				federatedInstances.Add(new FederatedInstanceDto
				{
					ArtifactId = artifactDto.ArtifactId,
					Name = artifactDto.Fields[0].Value as string,
					InstanceUrl = artifactDto.Fields[1].Value as string
				});
			}

			return federatedInstances;
		}
	}
}