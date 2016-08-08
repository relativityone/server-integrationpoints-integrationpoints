using System;
using System.Collections.Generic;
using System.Linq;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using Castle.Windsor.Installer;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Agent.Attributes;
using kCura.IntegrationPoints.Agent.Exceptions;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Factories.Implementations;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Contexts;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Queries;
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
	using global::kCura.IntegrationPoints.Agent.kCura.IntegrationPoints.Agent;

	public interface ITaskFactory
    {
        ITask CreateTask(Job job, ScheduleQueueAgentBase agentBase);

        void Release(ITask task);
    }

    public class TaskFactory : ITaskFactory
    {
        private readonly IAgentHelper _helper;
		private ISerializer _serializer;
	    private IContextContainerFactory _contextContainerFactory;
		private ICaseServiceContext _caseServiceContext;
		private IRSAPIClient _rsapiClient;
		private IWorkspaceDBContext _workspaceDbContext;
		private IEddsServiceContext _eddsServiceContext;
		private IRepositoryFactory _repositoryFactory;
	    private IJobHistoryService _jobHistoryService;
	    private IAgentService _agentService;
	    private IJobService _jobService;
	    private IManagerFactory _managerFactory;

		public TaskFactory(IAgentHelper helper)
        {
            _helper = helper;
        }

		public TaskFactory(IAgentHelper helper, IWindsorContainer container) : this(helper)
		{
			this.Container = container;
		}

		/// <summary>
		/// For unit tests only
		/// </summary>
	    internal TaskFactory(IAgentHelper helper, ISerializer serializer, IContextContainerFactory contextContainerFactory, ICaseServiceContext caseServiceContext, IRSAPIClient rsapiClient, IWorkspaceDBContext workspaceDbContext, IEddsServiceContext eddsServiceContext, 
			IRepositoryFactory repositoryFactory, IJobHistoryService jobHistoryService, IAgentService agentService, IJobService jobService, IManagerFactory managerFactory)
		{
			_helper = helper;
			_serializer = serializer;
			_contextContainerFactory = contextContainerFactory;
			_caseServiceContext = caseServiceContext;
			_rsapiClient = rsapiClient;
			_workspaceDbContext = workspaceDbContext;
			_eddsServiceContext = eddsServiceContext;
			_repositoryFactory = repositoryFactory;
			_jobHistoryService = jobHistoryService;
			_agentService = agentService;
			_jobService = jobService;
			_managerFactory = managerFactory;
		}

		private IWindsorContainer _container;

        private IWindsorContainer Container
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
            Container.Register(Component.For<IRSAPIService>().Instance(new RSAPIService(Container.Resolve<IHelper>(), job.WorkspaceID)).LifestyleTransient());
            Container.Register(Component.For<ISendable>()
              .ImplementedBy<SMTP>()
              .DependsOn(Dependency.OnValue<EmailConfiguration>(new RelativityConfigurationFactory().GetConfiguration())));

            Container.Register(Component.For<IOnBehalfOfUserClaimsPrincipalFactory>()
                .ImplementedBy<OnBehalfOfUserClaimsPrincipalFactory>()
                .LifestyleTransient());
        }

        public ITask CreateTask(Job job, ScheduleQueueAgentBase agentBase)
        {
            Install(job, agentBase);
			ResolveDependencies();
	        IntegrationPoint integrationPointDto = GetIntegrationPoint(job);
	        try
	        {
		        TaskType taskType;
		        Enum.TryParse(job.TaskType, true, out taskType);

		        //kCura.Method.Injection.InjectionManager.Instance.Evaluate("0b42a5bb-84e9-4fe8-8a75-1c6fbc0d4195");
		        switch (taskType)
		        {
			        case TaskType.SyncManager:
				        CheckForSynchronization(typeof(SyncManager), job, integrationPointDto, agentBase);
				        return Container.Resolve<SyncManager>();

			        case TaskType.SyncWorker:
						CheckForSynchronization(typeof(SyncWorker), job, integrationPointDto, agentBase);
						return Container.Resolve<SyncWorker>();

			        case TaskType.SyncCustodianManagerWorker:
						CheckForSynchronization(typeof(SyncCustodianManagerWorker), job, integrationPointDto, agentBase);
						return Container.Resolve<SyncCustodianManagerWorker>();

			        case TaskType.SendEmailManager:
						CheckForSynchronization(typeof(SendEmailManager), job, integrationPointDto, agentBase);
						return Container.Resolve<SendEmailManager>();

			        case TaskType.SendEmailWorker:
						CheckForSynchronization(typeof(SendEmailWorker), job, integrationPointDto, agentBase);
						return Container.Resolve<SendEmailWorker>();

			        case TaskType.ExportService:
						CheckForSynchronization(typeof(ExportServiceManager), job, integrationPointDto, agentBase);
						return Container.Resolve<ExportServiceManager>();

			        case TaskType.ExportManager:
						CheckForSynchronization(typeof(ExportManager), job, integrationPointDto, agentBase);
						return Container.Resolve<ExportManager>();

			        case TaskType.ExportWorker:
						CheckForSynchronization(typeof(ExportWorker), job, integrationPointDto, agentBase);
						return Container.Resolve<ExportWorker>();

			        default:
				        return null;
		        }
	        }
			catch (AgentDropJobException)
			{
				throw;
			}
			catch (Exception e)
	        {
		        UpdateJobHistoryOnFailure(job, integrationPointDto, e);
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

	    private void ResolveDependencies()
	    {
			_serializer = Container.Resolve<ISerializer>();
			_caseServiceContext = Container.Resolve<ICaseServiceContext>();
			_rsapiClient = Container.Resolve<IRSAPIClient>();
			_workspaceDbContext = Container.Resolve<IWorkspaceDBContext>();
			_eddsServiceContext = Container.Resolve<IEddsServiceContext>();
			_repositoryFactory = Container.Resolve<IRepositoryFactory>();
			_jobHistoryService = Container.Resolve<IJobHistoryService>();
			_contextContainerFactory = Container.Resolve<IContextContainerFactory>();

			_agentService = new AgentService(_helper, new Guid(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID));
			_jobService = new JobService(_agentService, _helper);
			_managerFactory = new ManagerFactory();
		}

	    private void CheckForSynchronization(Type type, Job job, IntegrationPoint integrationPointDto, ScheduleQueueAgentBase agentBase)
	    {
			object[] attributes = type.GetCustomAttributes(false);
			foreach (var attribute in attributes)
			{
				if (attribute is SynchronizedTaskAttribute)
				{
					if (HasOtherJobsExecuting(job))
					{
						DropJobAndThrowException(job, integrationPointDto, agentBase);
					}
				}
			}
		}

	    private IntegrationPoint GetIntegrationPoint(Job job)
	    {
			IChoiceQuery choiceQuery = new ChoiceQuery(_rsapiClient);
			JobResourceTracker jobResourceTracker = new JobResourceTracker(_repositoryFactory, _workspaceDbContext);
			JobTracker jobTracker = new JobTracker(jobResourceTracker);
			IJobManager jobManager = new AgentJobManager(_eddsServiceContext, _jobService, _serializer, jobTracker);

			IntegrationPointService integrationPointService = new IntegrationPointService(_helper, _caseServiceContext, _contextContainerFactory, _repositoryFactory, _serializer, choiceQuery, jobManager, _jobHistoryService, _managerFactory);

			IntegrationPoint integrationPoint = integrationPointService.GetRdo(job.RelatedObjectArtifactID);

			if (integrationPoint == null)
			{
				throw new NullReferenceException(
				  $"Unable to retrieve the integration point for the following job: {job.JobId}");
			}

		    return integrationPoint;
	    }

	    private JobHistory GetJobHistory(Job job)
	    {
			TaskParameters taskParameters = _serializer.Deserialize<TaskParameters>(job.JobDetails);
			JobHistory jobHistory = _jobHistoryService.GetRdo(taskParameters.BatchInstance);

			if (jobHistory == null)
			{
				throw new NullReferenceException(
				  $"Unable to retrieve job history information for the following job batch: {taskParameters.BatchInstance}");
			}

		    return jobHistory;
	    }

        private void UpdateJobHistoryOnFailure(Job job, IntegrationPoint integrationPointDto, Exception e)
        {
	        JobHistory jobHistory = GetJobHistory(job);

            JobHistoryErrorService jobHistoryErrorService = new JobHistoryErrorService(_caseServiceContext)
            {
                IntegrationPoint = integrationPointDto,
                JobHistory = jobHistory
            };

            jobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, e);
            jobHistoryErrorService.CommitErrors();

            jobHistory.JobStatus = JobStatusChoices.JobHistoryErrorJobFailed;
            _jobHistoryService.UpdateRdo(jobHistory);

            // No updates to IP since the job history error service handles IP updates
        }

		internal bool HasOtherJobsExecuting(Job job)
		{
			IContextContainer contextContainer = _contextContainerFactory.CreateContextContainer(_helper);
			IQueueManager queueManager = _managerFactory.CreateQueueManager(contextContainer);

			bool hasOtherJobsExecuting = queueManager.HasJobsExecuting(job.WorkspaceID, job.RelatedObjectArtifactID, job.JobId, job.NextRunTime);

			return hasOtherJobsExecuting;
		}

		internal void DropJobAndThrowException(Job job, IntegrationPoint integrationPointDto, ScheduleQueueAgentBase agentBase)
		{
			string exceptionMessage = "Unable to execute Integration Point job: There is already a job currently running.";
			
			//check if it's a scheduled job
			if (!String.IsNullOrEmpty(job.ScheduleRuleType))
			{
				integrationPointDto.NextScheduledRuntimeUTC = _jobService.GetJobNextUtcRunDateTime(job, agentBase.ScheduleRuleFactory, new TaskResult() { Status = TaskStatusEnum.None });
				exceptionMessage = $@"{exceptionMessage} Job is re-scheduled for {integrationPointDto.NextScheduledRuntimeUTC}.";
			}
			else
			{
				JobHistory jobHistory = GetJobHistory(job);
				RemoveJobHistoryFromIntegrationPoint(integrationPointDto, jobHistory.ArtifactId);
			}

			throw new AgentDropJobException(exceptionMessage);
		}

		internal void RemoveJobHistoryFromIntegrationPoint(IntegrationPoint integrationPointDto, int jobHistoryIdToRemove)
		{
			List<int> jobHistoryIds = integrationPointDto.JobHistory.ToList();
			jobHistoryIds.Remove(jobHistoryIdToRemove);
			integrationPointDto.JobHistory = jobHistoryIds.ToArray();
			_caseServiceContext.RsapiService.IntegrationPointLibrary.Update(integrationPointDto);

			_jobHistoryService.DeleteRdo(jobHistoryIdToRemove);
		}
	}
}