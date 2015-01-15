﻿using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.Installer;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.ScheduleQueue.AgentBase;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Relativity.API;
using Component = Castle.MicroKernel.Registration.Component;

namespace kCura.IntegrationPoints.Agent.Tasks
{
	public interface ITaskFactory
	{
		ITask CreateTask(Job job, ScheduleQueueAgentBase agentBase);
		void Release(ITask task);
	}

	public class TaskFactory : ITaskFactory
	{
		private readonly IAgentHelper _helper;
		public TaskFactory(IAgentHelper helper)
		{
			_helper = helper;
		}

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

		public ITask CreateTask(Job job, ScheduleQueueAgentBase agentBase)
		{
			Container.Register(Component.For<IScheduleRuleFactory>().UsingFactoryMethod((k) => agentBase.ScheduleRuleFactory, managedExternally: true));
			Container.Register(Component.For<IHelper>().UsingFactoryMethod((k) => _helper, managedExternally: true));
			Container.Register(Component.For<IAgentHelper>().UsingFactoryMethod((k) => _helper, managedExternally: true));
			Container.Register(Component.For<IServiceContextHelper>().ImplementedBy<ServiceContextHelperForAgent>()
				.DependsOn(Dependency.OnValue<int>(job.WorkspaceID)));
			Container.Register(Component.For<ICaseServiceContext>().ImplementedBy<CaseServiceContext>());
			Container.Register(Component.For<IEddsServiceContext>().ImplementedBy<EddsServiceContext>());
			Container.Install(FromAssembly.InThisApplication());
			TaskType taskType = TaskType.None;
			TaskType.TryParse(job.TaskType, true, out taskType);
			switch (taskType)
			{
				case TaskType.SyncManager:
					return Container.Resolve<SyncManager>();
				case TaskType.SyncWorker:
					return Container.Resolve<SyncWorker>();
				default:
					return null;
			}
			return null;
		}

		public void Release(ITask task)
		{
			if (task != null)
			{
				Container.Release(task);
			}
			Container = null;
		}
	}
}
