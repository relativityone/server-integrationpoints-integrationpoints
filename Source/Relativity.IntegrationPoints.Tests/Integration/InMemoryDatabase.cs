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
		public List<AgentTest> Agents { get; set; } = new List<AgentTest>();

		public List<JobTest> JobsInQueue { get; set; } = new List<JobTest>();

		public List<WorkspaceTest> Workspaces { get; set; } = new List<WorkspaceTest>();

		public List<IntegrationPointTest> IntegrationPoints { get; set; } = new List<IntegrationPointTest>();

		public void Clear()
		{
			Agents.Clear();
			JobsInQueue.Clear();
			Workspaces.Clear();
			IntegrationPoints.Clear();
		}
	}
}
