using System;
using System.IO;
using System.Threading.Tasks;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class IntegrationPointRepository : IIntegrationPointRepository
	{
		private readonly IRelativityObjectManager _objectManager;

		public IntegrationPointRepository(IRelativityObjectManager objectManager)
		{
			_objectManager = objectManager;
		}

		public async Task<IntegrationPoint> ReadAsync(int integrationPointArtifactID)
		{
			IntegrationPoint integrationPoint = _objectManager.Read<IntegrationPoint>(integrationPointArtifactID);
			integrationPoint.FieldMappings = await GetFieldMappingJsonAsync(integrationPointArtifactID).ConfigureAwait(false);
			return integrationPoint;
		}

		public string GetSecuredConfiguration(int integrationPointArtifactID)
		{
			return _objectManager.Read<IntegrationPoint>(integrationPointArtifactID).SecuredConfiguration;
		}

		public string GetName(int integrationPointArtifactID)
		{
			return _objectManager.Read<IntegrationPoint>(integrationPointArtifactID).Name;
		}

		public async Task<string> GetFieldMappingJsonAsync(int integrationPointArtifactID)
		{
			var field = new FieldRef { Guid = Guid.Parse(IntegrationPointFieldGuids.FieldMappings) };
			Stream fieldMapStream = await _objectManager.StreamLongTextAsync(integrationPointArtifactID, field).ConfigureAwait(false);
			var fieldMapStreamReader = new StreamReader(fieldMapStream);
			return await fieldMapStreamReader.ReadToEndAsync().ConfigureAwait(false);
		}
	}
}