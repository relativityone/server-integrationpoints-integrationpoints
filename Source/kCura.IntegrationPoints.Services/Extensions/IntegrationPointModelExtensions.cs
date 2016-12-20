using Newtonsoft.Json;

namespace kCura.IntegrationPoints.Services.Extensions
{
	public static class IntegrationPointModelExtensions
	{
		public static Core.Models.IntegrationPointModel ToCoreModel(this IntegrationPointModel model, string overwriteFieldsName)
		{
			return new Core.Models.IntegrationPointModel
			{
				ArtifactID = model.ArtifactId,
				DestinationProvider = model.DestinationProvider,
				SourceProvider = model.SourceProvider,
				Name = model.Name,
				SourceConfiguration = JsonConvert.SerializeObject(model.SourceConfiguration),
				Destination = JsonConvert.SerializeObject(model.DestinationConfiguration),
				LogErrors = model.LogErrors,
				Map = JsonConvert.SerializeObject(model.FieldMappings),
				NotificationEmails = model.EmailNotificationRecipients,
				Scheduler = model.ScheduleRule.ToScheduler(),
				Type = model.Type,
				SelectedOverwrite = overwriteFieldsName
			};
		}
	}
}