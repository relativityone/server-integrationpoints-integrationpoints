using System;
using System.Runtime.InteropServices;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.Installer;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client;
using kCura.ScheduleQueue.AgentBase;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Logging;
using kCura.ScheduleQueue.Core.TimeMachine;
using Relativity.API;
using ITaskFactory = kCura.IntegrationPoints.Agent.Tasks.ITaskFactory;

namespace kCura.IntegrationPoints.Agent
{
	[kCura.Agent.CustomAttributes.Name("Relativity Integration Points Agent")]
	[Guid(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID)]
	public class Agent : kCura.ScheduleQueue.AgentBase.ScheduleQueueAgentBase
	{
		private AgentInformation agentInformation = null;

		private static IWindsorContainer _container;
		private IWindsorContainer Container
		{
			get
			{
				if (_container == null)
				{
					_container = new WindsorContainer();
					_container.Register(Component.For<IHelper>().UsingFactoryMethod((k) => base.Helper).LifeStyle.Transient.LifeStyle.Transient);
					_container.Register(Component.For<RsapiClientFactory>().ImplementedBy<RsapiClientFactory>().LifeStyle.Transient);
					_container.Register(Component.For<IRSAPIClient>().UsingFactoryMethod((k) =>
				k.Resolve<RsapiClientFactory>().CreateClientForWorkspace(-1, ExecutionIdentity.System))
				.LifestyleTransient());
					_container.Install(FromAssembly.InDirectory(new AssemblyFilter("bin")));
				}
				return _container;
			}
		}

		public Agent()
			: base(Guid.Parse(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID))
		{
			base.RaiseException += new ExceptionEventHandler(RaiseException);
			base.RaiseJobLogEntry += new JobLoggingEventHandler(RaiseJobLog);
#if TIME_MACHINE
			AgentTimeMachineProvider.Current = new DefaultAgentTimeMachineProvider(Guid.Parse(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID));
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
			var errorService = Container.Resolve<CreateErrorRDO>();
			errorService.Execute(job, exception, "Relativity Integration Points Agent");
		}

		private void RaiseJobLog(Job job, JobLogState state, string details = null)
		{
			new JobLogService(base.Helper).Log(base.AgentService.AgentInformation, job, state, details);
		}
	}
}
