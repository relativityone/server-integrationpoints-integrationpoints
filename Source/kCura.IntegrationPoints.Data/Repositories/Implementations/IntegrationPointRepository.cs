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

		public IntegrationPoint Read(int integrationPointArtifactID)
		{
			IntegrationPoint integrationPoint = _objectManager.Read<IntegrationPoint>(integrationPointArtifactID);
			integrationPoint.FieldMappings = GetFieldMapJson(integrationPointArtifactID);
			return integrationPoint;
		}

		public string GetFieldMapJson(int integrationPointArtifactID)
		{
			var field = new FieldRef { Guid = Guid.Parse(IntegrationPointFieldGuids.FieldMappings) };
			Task<Stream> streamLongTextTask = _objectManager.StreamLongTextAsync(integrationPointArtifactID, field);
			Stream fieldMapStream = streamLongTextTask.GetAwaiter().GetResult();
			var fieldMapStreamReader = new StreamReader(fieldMapStream);
			return fieldMapStreamReader.ReadToEnd();
		}

		public string GetSecuredConfiguration(int integrationPointArtifactID)
		{
			return _objectManager.Read<IntegrationPoint>(integrationPointArtifactID).SecuredConfiguration;
		}

		public string GetName(int integrationPointArtifactID)
		{
			return _objectManager.Read<IntegrationPoint>(integrationPointArtifactID).Name;
		}
	}
}