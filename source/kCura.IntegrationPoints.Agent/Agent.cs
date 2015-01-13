﻿using System;
using System.Runtime.InteropServices;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.Installer;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Data;
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
	public class Agent : kCura.ScheduleQueue.AgentBase.ScheduleQueueAgentBase, IDisposable
	{
		private AgentInformation agentInformation = null;

		private WindsorContainer _container;
		private WindsorContainer Container
		{
			get
			{
				if (_container == null) _container = new WindsorContainer();
				return _container;
			}
			set { _container = value; }
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
			return TaskFactory.CreateTask(job);
		}

		protected override void ReleaseTask(ITask task)
		{
			TaskFactory.Release(task);
		}

		private CreateErrorRDO errorService;
		private void RaiseException(Job job, Exception exception)
		{
			if (errorService == null) errorService = Container.Resolve<CreateErrorRDO>();
			errorService.Execute(job, exception, "Relativity Integration Points Agent");
		}

		private void RaiseJobLog(Job job, JobLogState state, string details = null)
		{
			new JobLogService(base.Helper).Log(base.AgentService.AgentInformation, job, state, details);
		}

		public void Dispose()
		{
			if (errorService != null) Container.Release(errorService);
		}
	}
}
