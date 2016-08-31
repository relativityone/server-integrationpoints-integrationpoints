using System.Collections.Generic;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.EventHandlers.MassOperations
{
	public class IntegrationPointMassCopy
	{
		private readonly IRSAPIService _service;

		public IntegrationPointMassCopy(IRSAPIService service)
		{
			_service = service;
		}

		public void Copy(IEnumerable<int> integrationPointArtifactIds)
		{
			var selectedIntegrationPoints = _service.IntegrationPointLibrary.Read(integrationPointArtifactIds);
			foreach (var integrationPoint in selectedIntegrationPoints)
			{
				var newIntegrationPoint = BuildIntegrationPointModel(integrationPoint);
				_service.IntegrationPointLibrary.Create(newIntegrationPoint);
			}
		}

		private Data.IntegrationPoint BuildIntegrationPointModel(Data.IntegrationPoint row)
		{
			var ip = new Data.IntegrationPoint
			{
				ArtifactId = 0,
				EnableScheduler = false,
				HasErrors = false,
				LogErrors = row.LogErrors,
				Name = row.Name + "_copy",
				DestinationProvider = row.DestinationProvider,
				DestinationConfiguration = row.DestinationConfiguration,
				SourceConfiguration = row.SourceConfiguration,
				FieldMappings = row.FieldMappings,
				EmailNotificationRecipients = row.EmailNotificationRecipients,
				SourceProvider = row.SourceProvider,
				OverwriteFields = row.OverwriteFields
			};
			return ip;
		}
	}
}