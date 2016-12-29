using AutoMapper;
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
			result.Credentials = JsonConvert.SerializeObject(model.Credentials);
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
			Mapper.Map(model, modelBase);
			modelBase.SourceConfiguration = JsonConvert.SerializeObject(model.SourceConfiguration);
			modelBase.Destination = JsonConvert.SerializeObject(model.DestinationConfiguration);
			modelBase.Map = JsonConvert.SerializeObject(model.FieldMappings);
			modelBase.NotificationEmails = model.EmailNotificationRecipients;
			modelBase.Scheduler = Mapper.Map<Scheduler>(model.ScheduleRule);
			modelBase.SelectedOverwrite = overwriteFieldsName;
		}
	}
}