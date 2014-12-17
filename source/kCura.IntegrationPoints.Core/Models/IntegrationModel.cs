﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client;
using Newtonsoft.Json;

namespace kCura.IntegrationPoints.Core.Models
{
	public class IntegrationModel
	{
		public int ArtifactID { get; set; }
		public string Name{ get; set; }
		public Choice Overwrite { get; set; }
		public string SourceProvider { get; set; }
		public string Destination { get; set; }
		public bool? EnableScheduler { get; set; }
		public Choice Frequency { get; set; }
		public DateTime? StartDate{ get; set; }
		public DateTime? EndDate { get; set; }
		public string ScheduleTime{ get; set; }
		public DateTime? NextRun { get; set; }
		public DateTime? LastRun { get; set; }
		public string SourceConfiguration { get; set; }

		public IntegrationModel()
		{
			this.SourceConfiguration = string.Empty;
		}

		public IntegrationModel(IntegrationPoint ip)
		{
			//this.ArtifactID = ip.ArtifactId;
			//Name = ip.Name;
			//Overwrite = ip.OverwriteFields;
			//SourceProvider = ip.SourceConfiguration;
			//Destination = ip.DestinationConfiguration;
			//EnableScheduler = ip.EnableScheduler;
			//Frequency = ip.Frequency;
			//StartDate = ip.StartDate;
			//EndDate = ip.EndDate;
			//ScheduleTime = ip.ScheduledTime;
			//NextRun = ip.NextScheduledRuntime;
			//LastRun = ip.LastRuntime;
			//this.SourceConfiguration = ip.SourceConfiguration;
		}
	}
}
