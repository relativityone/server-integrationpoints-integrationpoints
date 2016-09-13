using System.Collections.Generic;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.EventHandlers.MassOperations
{
	public class IntegrationPointMassCopy
	{
		private readonly IIntegrationPointNameHelper _integrationPointNameHelper;
		private readonly IRSAPIService _service;

		public IntegrationPointMassCopy(IRSAPIService service, IIntegrationPointNameHelper integrationPointNameHelper)
		{
			_service = service;
			_integrationPointNameHelper = integrationPointNameHelper;
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

		private Data.IntegrationPoint BuildIntegrationPointModel(Data.IntegrationPoint integrationPoint)
		{
			var newName = _integrationPointNameHelper.CreateNameForCopy(integrationPoint);
			var ip = new Data.IntegrationPoint
			{
				ArtifactId = 0,
				EnableScheduler = false,
				HasErrors = false,
				LogErrors = integrationPoint.LogErrors,
				Name = newName,
				DestinationProvider = integrationPoint.DestinationProvider,
				DestinationConfiguration = integrationPoint.DestinationConfiguration,
				SourceConfiguration = integrationPoint.SourceConfiguration,
				FieldMappings = integrationPoint.FieldMappings,
				EmailNotificationRecipients = integrationPoint.EmailNotificationRecipients,
				SourceProvider = integrationPoint.SourceProvider,
				OverwriteFields = integrationPoint.OverwriteFields
			};
			return ip;
		}
	}
}