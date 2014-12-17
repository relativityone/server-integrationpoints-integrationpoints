using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client;
using Newtonsoft.Json;

namespace kCura.IntegrationPoints.Core.Models
{
	public class Scheduler
	{
		public Scheduler()
		{
		}

		public Scheduler(IntegrationPoint ip)
		{
			this.EnableScheduler = ip.EnableScheduler.GetValueOrDefault(false);
			if(ip.EndDate.HasValue){
				this.EndDate = ip.EndDate.Value.ToString("MM/dd/yyyy");
			}
			if (ip.StartDate.HasValue)
			{
				this.StartDate = ip.StartDate.Value.ToString("MM/dd/yyyy");
			}
			this.SendOn = ip.SendOn;
			//this.StartDate = ip.StartDate.
			this.Reoccur = ip.Reoccur.GetValueOrDefault(0);
			if (ip.Frequency != null)
			{
				SelectedFrequency = ip.Frequency.Name;
			}
			this.ScheduledTime = ip.ScheduledTime ?? string.Empty;
		}

		public bool EnableScheduler { get; set; }
		public string EndDate { get; set; }
		public string StartDate { get; set; }
		public string SelectedFrequency { get; set; }
		public int Reoccur { get; set; }
		public string ScheduledTime { get; set; }
		public string SendOn { get; set; }

	}



	public class IntegrationModel
	{
		public int ArtifactID { get; set; }
		public string Name { get; set; }
		public string SelectedOverwrite { get; set; }
		public string SourceProvider { get; set; }
		public string Destination { get; set; }
		public Scheduler Scheduler { get; set; }
		public DateTime? NextRun { get; set; }
		public DateTime? LastRun { get; set; }
		public string SourceConfiguration { get; set; }

		public IntegrationModel()
		{
			this.SourceConfiguration = string.Empty;
		}

		public IntegrationPoint ToRdo()
		{
			var point = new IntegrationPoint();
			point.ArtifactId = this.ArtifactID;
			point.Name = this.Name;
			//point.OverwriteFields = new Choice(Guid.Parse(Data.OverwriteFieldsChoiceGuids.APPEND_AND_OVERLAY_GUID), Data.OverwriteFieldsChoices.IntegrationPointAppend.Name);
			point.SourceConfiguration = this.SourceConfiguration;
			point.SourceProvider = null;

			point.DestinationConfiguration = this.Destination;

			point.EnableScheduler = this.Scheduler.EnableScheduler;
			DateTime startDate;
			if (DateTime.TryParse(this.Scheduler.StartDate, out startDate))
			{
				point.StartDate = startDate;		
			}

			DateTime endDate;
			if (DateTime.TryParse(this.Scheduler.EndDate, out endDate))
			{
				point.EndDate = endDate;
			}
			//point.Frequency = new Choice(Guid.Parse(Data.FrequencyChoiceGuids.Daily), Data.FrequencyChoices.IntegrationPointDaily.Name); //TODO // this.Scheduler.SelectedFrequency;
			point.Reoccur = this.Scheduler.Reoccur;
			TimeSpan time;
			if (TimeSpan.TryParse(this.Scheduler.ScheduledTime, out time))
			{
				var localTime = DateTime.UtcNow.Date.Add(new DateTime(time.Ticks, DateTimeKind.Utc).TimeOfDay).ToLocalTime().TimeOfDay;
				point.ScheduledTime = localTime.Hours.ToString() + ":" + localTime.Minutes.ToString();
			}

			point.SendOn = this.Scheduler.SendOn;
			return point;
		}

		public IntegrationModel(IntegrationPoint ip)
		{
			this.ArtifactID = ip.ArtifactId;
			Name = ip.Name;
			SelectedOverwrite = string.Empty;
			if (ip.OverwriteFields != null)
			{
				SelectedOverwrite = ip.OverwriteFields.Name;
			}
			SourceProvider = ip.SourceConfiguration;
			Destination = ip.DestinationConfiguration;
			Scheduler = new Scheduler(ip);

			NextRun = ip.NextScheduledRuntime;
			LastRun = ip.LastRuntime;
			this.SourceConfiguration = ip.SourceConfiguration;

		}


	}
}
