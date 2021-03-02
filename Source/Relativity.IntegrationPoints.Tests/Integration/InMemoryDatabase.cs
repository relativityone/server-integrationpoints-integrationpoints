using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration
{
	public class InMemoryDatabase
	{
		public List<Agent> Agents { get; set; } = new List<Agent>();

		public List<Job> JobsInQueue { get; set; } = new List<Job>();

		public List<Workspace> Workspaces { get; set; } = new List<Workspace>();

		public List<IntegrationPoint> IntegrationPoints { get; set; } = new List<IntegrationPoint>();

		public void Clear()
		{
			Agents.Clear();
			JobsInQueue.Clear();
			Workspaces.Clear();
			IntegrationPoints.Clear();
		}
	}
}
