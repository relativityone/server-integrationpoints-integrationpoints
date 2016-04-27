﻿using System;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using Castle.Windsor.Installer;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Contexts;
using kCura.IntegrationPoints.Data.Queries;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Email;
using kCura.Relativity.Client;
using kCura.ScheduleQueue.AgentBase;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using kCura.ScheduleQueue.Core.Services;
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
			get { return _container ?? (_container = new WindsorContainer()); }
			set { _container = value; }
		}

		private void Install(Job job, ScheduleQueueAgentBase agentBase)
		{
			Container.Kernel.Resolver.AddSubResolver(new CollectionResolver(Container.Kernel));
			Container.Register(Component.For<IScheduleRuleFactory>().UsingFactoryMethod(k => agentBase.ScheduleRuleFactory, true));
			Container.Register(Component.For<IHelper>().UsingFactoryMethod(k => _helper, true));
			Container.Register(Component.For<IAgentHelper>().UsingFactoryMethod(k => _helper, true));
			Container.Register(Component.For<IServiceContextHelper>().ImplementedBy<ServiceContextHelperForAgent>()
				.DependsOn(Dependency.OnValue<int>(job.WorkspaceID)));
			Container.Register(Component.For<ICaseServiceContext>().ImplementedBy<CaseServiceContext>());
			Container.Register(Component.For<IEddsServiceContext>().ImplementedBy<EddsServiceContext>());
			Container.Register(Component.For<IWorkspaceDBContext>().UsingFactoryMethod(k => new WorkspaceContext(_helper.GetDBContext(job.WorkspaceID))));
			Container.Register(Component.For<Job>().UsingFactoryMethod(k => job));

			Container.Register(Component.For<GetApplicationBinaries>().ImplementedBy<GetApplicationBinaries>().DynamicParameters((k, d) => d["eddsDBcontext"] = _helper.GetDBContext(-1)).LifeStyle.Transient);
			Container.Install(FromAssembly.InThisApplication());
			Container.Register(Component.For<IRSAPIClient>().UsingFactoryMethod(k =>
				k.Resolve<RsapiClientFactory>().CreateClientForWorkspace(job.WorkspaceID, ExecutionIdentity.System)).LifestyleTransient());
			Container.Register(Component.For<ISendable>()
				.ImplementedBy<SMTP>()
				.DependsOn(Dependency.OnValue<EmailConfiguration>(new RelativityConfigurationFactory().GetConfiguration())));

			Container.Register(Component.For<IOnBehalfOfUserClaimsPrincipleFactory>()
					.ImplementedBy<OnBehalfOfUserClaimsPrincipleFactory>()
					.LifestyleTransient());
		}

		public ITask CreateTask(Job job, ScheduleQueueAgentBase agentBase)
		{
			Install(job, agentBase);
			try
			{
				TaskType taskType;
				Enum.TryParse(job.TaskType, true, out taskType);
				switch (taskType)
				{
					case TaskType.SyncManager:
						return Container.Resolve<SyncManager>();
					case TaskType.SyncWorker:
						return Container.Resolve<SyncWorker>();
					case TaskType.SyncCustodianManagerWorker:
						return Container.Resolve<SyncCustodianManagerWorker>();
					case TaskType.SendEmailManager:
						return Container.Resolve<SendEmailManager>();
					case TaskType.SendEmailWorker:
						return Container.Resolve<SendEmailWorker>();
					case TaskType.ExportService:
						return Container.Resolve<ExportServiceManager>();
					default:
						return null;
				}
			}
			catch (Exception e)
			{
				UpdateJobHistoryOnFailure(job, e);
				throw;
			}
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

		private void UpdateJobHistoryOnFailure(Job job, Exception e)
		{
			ISerializer serializer = Container.Resolve<ISerializer>();
			ICaseServiceContext caseServiceContext = Container.Resolve<ICaseServiceContext>();
			IRSAPIClient rsapiClient = Container.Resolve<IRSAPIClient>();
			IWorkspaceDBContext workspaceDbContext = Container.Resolve<IWorkspaceDBContext>();
			IEddsServiceContext eddsServiceContext = Container.Resolve<IEddsServiceContext>();
			IWorkspaceRepository workspaceRepository = Container.Resolve<IWorkspaceRepository>();
		
			ChoiceQuery choiceQuery = new ChoiceQuery(rsapiClient);
			JobResourceTracker jobResourceTracker = new JobResourceTracker(workspaceDbContext);
			JobTracker jobTracker = new JobTracker(jobResourceTracker);
			IAgentService agentService = new AgentService(_helper, new Guid(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID));

			IJobService jobService = new JobService(agentService, _helper);
			IJobManager jobManager = new AgentJobManager(eddsServiceContext, jobService, serializer, jobTracker);

			IntegrationPointService integrationPointService = new IntegrationPointService(caseServiceContext, serializer, choiceQuery, jobManager);
			IntegrationPoint integrationPoint = integrationPointService.GetRdo(job.RelatedObjectArtifactID);
			
			TaskParameters taskParameters = serializer.Deserialize<TaskParameters>(job.JobDetails);

			JobHistoryService jobHistoryService = new JobHistoryService(caseServiceContext, workspaceRepository);
			JobHistory jobHistory = jobHistoryService.GetRdo(taskParameters.BatchInstance);

			if (integrationPoint == null || jobHistory == null)
			{
				throw new NullReferenceException(
					$"Unable to retrieve the integration point or job history information for the following job batch: {taskParameters.BatchInstance}");
			}

			JobHistoryErrorService jobHistoryErrorService = new JobHistoryErrorService(caseServiceContext)
			{
				IntegrationPoint = integrationPoint,
				JobHistory = jobHistory
			};

			jobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, e);
			jobHistoryErrorService.CommitErrors();

			jobHistory.Status = JobStatusChoices.JobHistoryErrorJobFailed;
			jobHistoryService.UpdateRdo(jobHistory);

			// No updates to IP since the job history error service handles IP updates
		}
	}
}
