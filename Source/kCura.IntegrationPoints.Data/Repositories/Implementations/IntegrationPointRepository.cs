using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class IntegrationPointRepository : IIntegrationPointRepository
	{
		private const string _ERROR_DESERIALIZING_FIELD_MAPPING = "Failed to deserialize field mapping for Integration Point";

		private readonly IRelativityObjectManager _objectManager;
		private readonly IIntegrationPointSerializer _serializer;
		private readonly IAPILog _logger;

		public IntegrationPointRepository(IRelativityObjectManager objectManager,
			IIntegrationPointSerializer serializer,
			IAPILog apiLog)
		{
			_objectManager = objectManager;
			_serializer = serializer;
			_logger = apiLog.ForContext<IntegrationPointRepository>();
		}

		public async Task<IntegrationPoint> ReadAsync(int integrationPointArtifactID)
		{
			IntegrationPoint integrationPoint = _objectManager.Read<IntegrationPoint>(integrationPointArtifactID);
			integrationPoint.FieldMappings = await GetFieldMappingJsonAsync(integrationPointArtifactID).ConfigureAwait(false);
			return integrationPoint;
		}

		public async Task<IEnumerable<FieldMap>> GetFieldMappingAsync(int integrationPointArtifactID)
		{
			IEnumerable<FieldMap> fieldMapping = new List<FieldMap>();

			if (integrationPointArtifactID <= 0)
			{
				return fieldMapping;
			}

			string fieldMappingJson = await GetFieldMappingJsonAsync(integrationPointArtifactID).ConfigureAwait(false);

			if (string.IsNullOrEmpty(fieldMappingJson))
			{
				return fieldMapping;
			}

			try
			{
				fieldMapping = _serializer.Deserialize<IEnumerable<FieldMap>>(fieldMappingJson);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, _ERROR_DESERIALIZING_FIELD_MAPPING);
				throw;
			}

			return fieldMapping;
		}

		public string GetSecuredConfiguration(int integrationPointArtifactID)
		{
			return _objectManager.Read<IntegrationPoint>(integrationPointArtifactID).SecuredConfiguration;
		}

		public string GetName(int integrationPointArtifactID)
		{
			return _objectManager.Read<IntegrationPoint>(integrationPointArtifactID).Name;
		}

		private async Task<string> GetFieldMappingJsonAsync(int integrationPointArtifactID)
		{
			var field = new FieldRef { Guid = Guid.Parse(IntegrationPointFieldGuids.FieldMappings) };
			Stream fieldMapStream = await _objectManager.StreamLongTextAsync(integrationPointArtifactID, field).ConfigureAwait(false);
			var fieldMapStreamReader = new StreamReader(fieldMapStream);
			return await fieldMapStreamReader.ReadToEndAsync().ConfigureAwait(false);
		}
	}
}