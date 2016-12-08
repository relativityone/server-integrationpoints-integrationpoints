using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client.DTOs;
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
		public int OverwriteFieldsChoiceId { get; set; }
		
		public virtual Core.Models.IntegrationPointModel ToModel(IList<Choice> choices)
		{
			//TODO remove this hack when IntegrationPointModel will start using ChoiceId instead of ChoiceName
			var choice = choices.First(x => x.ArtifactID == OverwriteFieldsChoiceId);
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
				SelectedOverwrite = choice.Name
			};
		}
	}
}