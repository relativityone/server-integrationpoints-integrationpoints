using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data.Converters;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class KeplerFederatedInstanceRepository : IFederatedInstanceRepository
	{
		private readonly IAPILog _logger;
		private readonly int _federatedInstanceArtifactTypeId;
		private readonly IRelativityObjectManager _relativityObjectManager;

		public KeplerFederatedInstanceRepository(int federatedInstanceArtifactTypeId, IHelper helper, IRelativityObjectManager relativityObjectManager)
		{
			_federatedInstanceArtifactTypeId = federatedInstanceArtifactTypeId;
			_relativityObjectManager = relativityObjectManager;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<KeplerWorkspaceRepository>();
		}

		public FederatedInstanceDto RetrieveFederatedInstance(string name)
		{
			try
			{
				return RetrieveFederatedInstanceByCondition($"'Name' == '{name.EscapeSingleQuote()}'").FirstOrDefault();
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
				Fields = new List<FieldRef>()
				{
					new FieldRef() { Name = FederatedInstanceFieldsConstants.NAME_FIELD },
					new FieldRef() { Name = FederatedInstanceFieldsConstants.INSTANCE_URL_FIELD }
				}
			};

			IEnumerable<RelativityObject> artifactDtos = _relativityObjectManager.QueryAsync(query).GetAwaiter().GetResult();
			return artifactDtos.ToFederatedInstanceDTOs();
		}
	}
}