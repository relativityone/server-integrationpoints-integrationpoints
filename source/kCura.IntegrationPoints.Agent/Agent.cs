using System;
using System.Runtime.InteropServices;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.Installer;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client;
using kCura.ScheduleQueue.AgentBase;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Logging;
using kCura.ScheduleQueue.Core.TimeMachine;
using Relativity.API;
using ITaskFactory = kCura.IntegrationPoints.Agent.Tasks.ITaskFactory;
using kCura.Apps.Common.Data;

namespace kCura.IntegrationPoints.Agent
{
	[kCura.Agent.CustomAttributes.Name("Relativity Integration Points Agent")]
	[Guid(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID)]
	public class Agent : kCura.ScheduleQueue.AgentBase.ScheduleQueueAgentBase, IDisposable
	{
		//private IWindsorContainer _container;
		//private IWindsorContainer Container
		//{
		//	get
		//	{
		//		if (_container == null)
		//		{
		//			_container = new WindsorContainer();
		//			_container.Register(Component.For<IHelper>().UsingFactoryMethod((k) => base.Helper, managedExternally: true));
		//			_container.Install(FromAssembly.InThisApplication());
		//		}
		//		return _container;
		//	}
		//	set { _container = value; }
		//}

		private IRSAPIClient _eddsRsapiClient;
		public IRSAPIClient EddsRsapiClient
		{
			get
			{
				if (_eddsRsapiClient == null)
				{
					_eddsRsapiClient = new RsapiClientFactory(base.Helper).CreateClientForWorkspace(-1, ExecutionIdentity.System);
				}
				return _eddsRsapiClient;
			}
			set { _eddsRsapiClient = value; }
		}

		private ITaskFactory _taskFactory;
		ITaskFactory TaskFactory
		{
			get
			{
				if (_taskFactory == null) { _taskFactory = new TaskFactory(base.Helper); }
				return _taskFactory;
			}
		}

		public Agent()
			: base(Guid.Parse(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID))
		{
			base.RaiseException += new ExceptionEventHandler(RaiseException);
			base.RaiseJobLogEntry += new JobLoggingEventHandler(RaiseJobLog);
			kCura.Apps.Common.Config.Manager.Settings.Factory = new HelperConfigSqlServiceFactory(base.Helper);
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
			return TaskFactory.CreateTask(job, this);
		}

		protected override void ReleaseTask(ITask task)
		{
			TaskFactory.Release(task);
		}

		private CreateErrorRdo errorService;
		private void RaiseException(Job job, Exception exception)
		{
			if (errorService == null) errorService = new CreateErrorRdo(this.EddsRsapiClient);
			errorService.Execute(job, exception, "Relativity Integration Points Agent");
		}

		private void RaiseJobLog(Job job, JobLogState state, string details = null)
		{
			new JobLogService(base.Helper).Log(base.AgentService.AgentTypeInformation, job, state, details);
		}

		public void Dispose()
		{
			//if (errorService != null) Container.Release(errorService);
		}
	}
}
