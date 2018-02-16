using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Domain.Models;
using Relativity;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class KeplerFederatedInstanceRepository : KeplerServiceBase, IFederatedInstanceRepository
	{
		private readonly IAPILog _logger;
		private readonly int _federatedInstanceArtifactTypeId;

		public KeplerFederatedInstanceRepository(int federatedInstanceArtifactTypeId, IHelper helper, IRelativityObjectManager relativityObjectManager)
			: base(relativityObjectManager)
		{
			_federatedInstanceArtifactTypeId = federatedInstanceArtifactTypeId;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<KeplerWorkspaceRepository>();
		}

		public FederatedInstanceDto RetrieveFederatedInstance(string name)
		{
			try
			{
				return RetrieveFederatedInstanceByCondition($"'Name' == '{EscapeSingleQuote(name)}'").FirstOrDefault();
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Unable to retrieve federated instance {Name}", name);
				throw;
			}
		}

		public FederatedInstanceDto RetrieveFederatedInstance(int artifactId)
		{
			try
			{
				return RetrieveFederatedInstanceByCondition($"'Artifact ID' == {artifactId}").FirstOrDefault();
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Unable to retrieve federated instance {ArtifactId}", artifactId);
				throw;
			}
		}

		public IEnumerable<FederatedInstanceDto> RetrieveAll()
		{
			try
			{
				return RetrieveFederatedInstanceByCondition(string.Empty);
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Unable to retrieve federated instances");
				throw;
			}
		}

		private IEnumerable<FederatedInstanceDto> RetrieveFederatedInstanceByCondition(string condition)
		{
			var query = new QueryRequest()
			{
				ObjectType = new ObjectTypeRef { ArtifactTypeID = _federatedInstanceArtifactTypeId },
				Condition = condition,
				Fields = new List<FieldRef>() { new FieldRef() { Name = "Name" }, new FieldRef() { Name = "Instance URL" } }
			};

			ArtifactDTO[] artifactDtos = null;

			artifactDtos = this.RetrieveAllArtifactsAsync(query).GetResultsWithoutContextSync();

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