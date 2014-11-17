using System.Runtime.InteropServices;
using kCura.Agent.ScheduleQueueAgent;

namespace kCura.IntegrationPoints.Agent
{
	[kCura.Agent.CustomAttributes.Name("Relativity Integration Points Agent")]
	[Guid("08C0CE2D-8191-4E8F-B037-899CEAEE493D")]
	public class Agent : ScheduleQueueAgentBase
	{
		public override string Name
		{
			get { return "Relativity Integration Points"; }
		}
	}
}
