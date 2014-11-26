using System;
using System.Runtime.InteropServices;
using kCura.Relativity.Client;
using kCura.ScheduleQueueAgent;
using kCura.ScheduleQueueAgent.Logging;

namespace kCura.IntegrationPoints.Agent
{
	[kCura.Agent.CustomAttributes.Name("Relativity Integration Points Agent")]
	[Guid("08C0CE2D-8191-4E8F-B037-899CEAEE493D")]
	public class Agent : ScheduleQueueAgentBase
	{
		private IRSAPIClient rsapiClient;
		private AgentInformation agentInformation = null;
		public Agent()
		{
			base.RaiseException += new ExceptionEventHandler(RaiseException);
			base.RaiseJobLogEntry += new JobLoggingEventHandler(RaiseJobLog);
		}
		public override string Name
		{
			get { return "Relativity Integration Points"; }
		}

		private void RaiseException(Job job, Exception exception)
		{
			new CreateErrorRDO(rsapiClient).Execute(job, exception, "Relativity Integration Points Agent");
		}

		private void RaiseJobLog(Job job, JobLogState state, string details = null)
		{
			new JobLogService(base.Helper).Log(base.AgentService.AgentInformation, job, state, details);
		}
	}
}
