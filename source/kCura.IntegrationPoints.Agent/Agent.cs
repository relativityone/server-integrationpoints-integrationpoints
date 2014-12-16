using System;
using System.Runtime.InteropServices;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.Installer;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Core.Services;
using kCura.Relativity.Client;
using kCura.ScheduleQueueAgent;
using kCura.ScheduleQueueAgent.Logging;
using kCura.ScheduleQueueAgent.Services;
using kCura.ScheduleQueueAgent.TimeMachine;
using ITaskFactory = kCura.IntegrationPoints.Agent.Tasks.ITaskFactory;

namespace kCura.IntegrationPoints.Agent
{
	public static class GlobalConst
	{
		public const string AGENT_GUID = "08C0CE2D-8191-4E8F-B037-899CEAEE493D";
	}

	[kCura.Agent.CustomAttributes.Name("Relativity Integration Points Agent")]
	[Guid(GlobalConst.AGENT_GUID)]
	public class Agent : ScheduleQueueAgentBase
	{
		private IRSAPIClient rsapiClient;
		private AgentInformation agentInformation = null;

		private static IWindsorContainer _container;
		private IWindsorContainer Container
		{
			get
			{
				if (_container == null)
				{
					_container = new WindsorContainer();
					_container.Install(FromAssembly.InDirectory(new AssemblyFilter("bin")));
				}
				return _container;
			}
		}

		public Agent()
			: base(Guid.Parse(GlobalConst.AGENT_GUID))
		{
			base.RaiseException += new ExceptionEventHandler(RaiseException);
			base.RaiseJobLogEntry += new JobLoggingEventHandler(RaiseJobLog);
#if TIME_MACHINE
			AgentTimeMachineProvider.Current = new DefaultAgentTimeMachineProvider(Guid.Parse(GlobalConst.AGENT_GUID));
#endif
		}
		public override string Name
		{
			get { return "Relativity Integration Points"; }
		}

		public override ITask GetTask(Job job)
		{
			return Container.Resolve<ITaskFactory>().CreateTask(job);
		}

		protected override void ReleaseTask(ITask task)
		{
			Container.Resolve<ITaskFactory>().Release(task);
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
