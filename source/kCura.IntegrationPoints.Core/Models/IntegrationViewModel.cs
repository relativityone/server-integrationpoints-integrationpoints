using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.Relativity.Client;

namespace kCura.IntegrationPoints.Core.Models
{
	public class IntegrationViewModel
	{
		public string Name{ get; set; }
		public string Overwrite { get; set; }
		public string SourceProvider { get; set; }
		public string Destination { get; set; }
		public bool EnableScheduler { get; set; }
		public string Frequency { get; set; }
		public DateTime? StartDate{ get; set; }
		public DateTime? EndDate { get; set; }
		public DateTime? ScheduleTime{ get; set; }
		public string ConnectionPath { get; set; }
		public string FilterString { get; set; }
		public string Authentication { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
		public string NestedItems { get; set; }
		public DateTime? NextRun { get; set; }
		public DateTime? LastRun { get; set; }

	}
}
