using kCura.IntegrationPoints.Core.Models;
using Newtonsoft.Json;

namespace kCura.IntegrationPoints.Services.Extensions
{
	public static class IntegrationPointModelExtensions
	{
		public static Core.Models.IntegrationPointModel ToCoreModel(this IntegrationPointModel model, string overwriteFieldsName)
		{
			var result = new Core.Models.IntegrationPointModel();
			result.SetProperties(model, overwriteFieldsName);
			return result;
		}

		public static IntegrationPointProfileModel ToCoreProfileModel(this IntegrationPointModel model, string overwriteFieldsName)
		{
			var result = new IntegrationPointProfileModel();
			result.SetProperties(model, overwriteFieldsName);
			return result;
		}

		private static void SetProperties(this IntegrationPointModelBase modelBase, IntegrationPointModel model, string overwriteFieldsName)
		{
			modelBase.ArtifactID = model.ArtifactId;
			modelBase.DestinationProvider = model.DestinationProvider;
			modelBase.SourceProvider = model.SourceProvider;
			modelBase.Name = model.Name;
			modelBase.SourceConfiguration = JsonConvert.SerializeObject(model.SourceConfiguration);
			modelBase.Destination = JsonConvert.SerializeObject(model.DestinationConfiguration);
			modelBase.LogErrors = model.LogErrors;
			modelBase.Map = JsonConvert.SerializeObject(model.FieldMappings);
			modelBase.NotificationEmails = model.EmailNotificationRecipients;
			modelBase.Scheduler = model.ScheduleRule.ToScheduler();
			modelBase.Type = model.Type;
			modelBase.SelectedOverwrite = overwriteFieldsName;
		}
	}
}