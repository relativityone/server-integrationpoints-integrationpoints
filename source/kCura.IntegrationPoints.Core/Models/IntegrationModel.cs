using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client;

namespace kCura.IntegrationPoints.Core.Models
{
	public class IntegrationModel
	{
		public string Name{ get; set; }
		public string Overwrite { get; set; }
		public string SourceProvider { get; set; }
		public string Destination { get; set; }
		public bool? EnableScheduler { get; set; }
		public Choice Frequency { get; set; }
		public DateTime? StartDate{ get; set; }
		public DateTime? EndDate { get; set; }
		public string ScheduleTime{ get; set; }
		public string ConnectionPath { get; set; }
		public string FilterString { get; set; }
		public string Authentication { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
		public string NestedItems { get; set; }
		public DateTime? NextRun { get; set; }
		public DateTime? LastRun { get; set; }

		
		public IntegrationModel(IntegrationPoint ip)
		{
			Name = ip.Name;
			Overwrite = ip.OverwriteFields.ToString();
			SourceProvider = ip.SourceConfiguration;
			Destination = ip.DestinationConfiguration;
			EnableScheduler = ip.EnableScheduler;
			Frequency = ip.Frequency;
			StartDate = ip.StartDate;
			EndDate = ip.EndDate;
			ScheduleTime = ip.ScheduledTime;
			NextRun = ip.NextScheduledRuntime;
			LastRun = ip.LastRuntime;
			ConnectionPath = "NOT HERE";
			FilterString = "NOT HERE";
			Authentication = "NOT HERE";
			Username = "NOT HERE";
			Password = "NOT HERE";
			NestedItems = "NOT HERE";

		}
	}
}
