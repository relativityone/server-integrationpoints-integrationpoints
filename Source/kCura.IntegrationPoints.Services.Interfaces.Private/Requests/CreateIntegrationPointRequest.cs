using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Domain.Models;
using Newtonsoft.Json;

namespace kCura.IntegrationPoints.Services
{
	public class CreateIntegrationPointRequest
	{
		public int WorkspaceArtifactId { get; set; }
		public string Name { get; set; }
		public List<FieldMap> FieldMappings { get; set; }
		public object SourceConfiguration { get; set; }
		public object DestinationConfiguration { get; set; }
		public int SourceProvider { get; set; }
		public Scheduler ScheduleRule { get; set; }
		public int DestinationProvider { get; set; }
		public bool LogErrors { get; set; }
		public string EmailNotificationRecipients { get; set; }
		public int Type { get; set; }
		//TODO we could think about replacing string with int (we would need new Kepler Service for retrieving choices)
		public string SelectedOverwrite { get; set; }

		public virtual Core.Models.IntegrationPointModel ToModel()
		{
			return new Core.Models.IntegrationPointModel
			{
				ArtifactID = 0,
				DestinationProvider = DestinationProvider,
				SourceProvider = SourceProvider,
				Name = Name,
				SourceConfiguration = JsonConvert.SerializeObject(SourceConfiguration),
				Destination = JsonConvert.SerializeObject(DestinationConfiguration),
				LogErrors = LogErrors,
				Map = JsonConvert.SerializeObject(FieldMappings),
				NotificationEmails = EmailNotificationRecipients,
				Scheduler = ScheduleRule,
				Type = Type,
				SelectedOverwrite = SelectedOverwrite
			};
		}
	}
}