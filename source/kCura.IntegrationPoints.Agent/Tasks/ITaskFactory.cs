using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.Installer;
using kCura.Agent;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data.Queries;
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

		private void Install(Job job, ScheduleQueueAgentBase agentBase)
		{
			Container.Register(Component.For<IScheduleRuleFactory>().UsingFactoryMethod((k) => agentBase.ScheduleRuleFactory, managedExternally: true));
			Container.Register(Component.For<IHelper>().UsingFactoryMethod((k) => _helper, managedExternally: true));
			Container.Register(Component.For<IAgentHelper>().UsingFactoryMethod((k) => _helper, managedExternally: true));
			Container.Register(Component.For<IServiceContextHelper>().ImplementedBy<ServiceContextHelperForAgent>()
				.DependsOn(Dependency.OnValue<int>(job.WorkspaceID)));
			Container.Register(Component.For<ICaseServiceContext>().ImplementedBy<CaseServiceContext>());
			Container.Register(Component.For<IEddsServiceContext>().ImplementedBy<EddsServiceContext>());
			Container.Register(Component.For<GetApplicationBinaries>().ImplementedBy<GetApplicationBinaries>().DynamicParameters((k, d) => d["eddsDBcontext"] = _helper.GetDBContext(-1)).LifeStyle.Transient);
			Container.Install(FromAssembly.InThisApplication());
		}

		public ITask CreateTask(Job job, ScheduleQueueAgentBase agentBase)
		{
			Install(job, agentBase);
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
			try
			{
				if (task != null)
				{
					Container.Release(task);
				}
			}
			finally
			{
				Container = null;
			}
		}
	}
}
