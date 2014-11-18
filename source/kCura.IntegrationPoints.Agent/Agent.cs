using System.Runtime.InteropServices;
using kCura.Agent.CustomAttributes;
using kCura.Agent.ScheduleQueueAgent;
using kCura.Agent.ScheduleQueueAgent.CustomAttributes;

namespace kCura.IntegrationPoints.Agent
{
	[kCura.Agent.CustomAttributes.Name("Relativity Integration Points Agent")]
	[Guid("08C0CE2D-8191-4E8F-B037-899CEAEE493D")]
	[QueueTable(Name = "IntegrationPointsQueue")]
	public class Agent : ScheduleQueueAgentBase
	{
		public override string Name
		{
			get { return "Relativity Integration Points"; }
		}
	}
}
