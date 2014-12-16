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
			this.EndDate = ip.EndDate;
			this.StartDate = ip.StartDate;
			this.SendOn = ip.SendOn;
			//this.StartDate = ip.StartDate.
		}

		public bool EnableScheduler { get; set; }
		public DateTime? EndDate { get; set; }
		public DateTime? StartDate { get; set; }
		public string SelectedFrequency { get; set; }
		public int Reoccur { get; set; }
		public TimeSpan ScheduledTime { get; set; }
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
		public string SelectedFrequency { get; set; }
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
			point.StartDate = this.Scheduler.StartDate;
			point.EndDate = this.Scheduler.EndDate;
			point.Frequency = new Choice(Guid.Parse(Data.FrequencyChoiceGuids.Daily), Data.FrequencyChoices.IntegrationPointDaily.Name); //TODO // this.Scheduler.SelectedFrequency;
			point.Reoccur = this.Scheduler.Reoccur;
			//point.ScheduledTime = this.Scheduler.ScheduledTime;
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
			if (ip.Frequency != null)
			{
				SelectedFrequency = ip.Frequency.Name;
			}
			if (ip.Frequency != null)
			{
				SelectedFrequency = ip.Frequency.Name;
			}
			NextRun = ip.NextScheduledRuntime;
			LastRun = ip.LastRuntime;
			this.SourceConfiguration = ip.SourceConfiguration;

		}


	}
}
