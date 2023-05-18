using System;
using System.Threading;
using kCura.IntegrationPoints.Common.Agent;
using kCura.IntegrationPoints.Core.BatchStatusCommands.Implementations;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Helpers.Implementations;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Core.RelativitySourceRdo;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Tagging;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Helpers;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Logging;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Factories.Implementations
{
    public class ManagerFactory : IManagerFactory
    {
        private readonly IHelper _helper;
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IRemovableAgent _agent;
        private readonly IAPILog _logger;

        public ManagerFactory(IHelper helper, IRemovableAgent agent)
            : this(helper, new RepositoryFactory(helper, helper.GetServicesManager()), agent)
        { }

        public ManagerFactory(IHelper helper, IRepositoryFactory repositoryFactory, IRemovableAgent agent)
        {
            _helper = helper;
            _repositoryFactory = repositoryFactory;
            _agent = agent;
            _logger = _helper.GetLoggerFactory().GetLogger();
        }

        public IArtifactGuidManager CreateArtifactGuidManager()
        {
            return new ArtifactGuidManager(_repositoryFactory);
        }

        public IFieldManager CreateFieldManager()
        {
            return new FieldManager(_repositoryFactory);
        }

        public IJobHistoryManager CreateJobHistoryManager()
        {
            var massUpdateHelper = new MassUpdateHelper(Config.Config.Instance, _logger);
            return new JobHistoryManager(_repositoryFactory, _logger, massUpdateHelper);
        }

        public IJobHistoryErrorManager CreateJobHistoryErrorManager(int sourceWorkspaceArtifactId, string uniqueJobId)
        {
            return new JobHistoryErrorManager(_repositoryFactory, _helper, sourceWorkspaceArtifactId, uniqueJobId);
        }

        public IObjectTypeManager CreateObjectTypeManager()
        {
            return new ObjectTypeManager(_repositoryFactory);
        }

        public IQueueManager CreateQueueManager()
        {
            return new QueueManager(_repositoryFactory);
        }

        public IErrorManager CreateErrorManager()
        {
            return new ErrorManager(_repositoryFactory);
        }

        public IJobStopManager CreateJobStopManager(
            IJobService jobService,
            IJobHistoryService jobHistoryService,
            Guid jobIdentifier,
            long jobId,
            bool supportsDrainStop,
            IDiagnosticLog diagnosticLog,
            CancellationTokenSource stopCancellationTokenSource = null,
            CancellationTokenSource drainStopCancellationTokenSource = null)
        {
            _logger.LogInformation("Create JobStopManager for Job: {jobId}", jobId);
            JobStopManager jobStopManager = new JobStopManager(
                jobService,
                jobHistoryService,
                _helper,
                jobIdentifier,
                jobId,
                _agent,
                supportsDrainStop,
                stopCancellationTokenSource ?? new CancellationTokenSource(),
                drainStopCancellationTokenSource ?? new CancellationTokenSource(),
                diagnosticLog);
            jobStopManager.ActivateTimer();
            return jobStopManager;
        }

        public IAuditManager CreateAuditManager(int workspaceArtifactId)
        {
            IRelativityAuditRepository relativityAuditRepository = _repositoryFactory.GetRelativityAuditRepository(workspaceArtifactId);
            return new AuditManager(relativityAuditRepository);
        }

        public IStateManager CreateStateManager()
        {
            return new StateManager();
        }

        public IFederatedInstanceManager CreateFederatedInstanceManager()
        {
            return new FederatedInstanceManager();
        }

        public IWorkspaceManager CreateWorkspaceManager()
        {
            IWorkspaceManager workspaceManager = new WorkspaceManager(_repositoryFactory);
            return workspaceManager;
        }

        public IPermissionManager CreatePermissionManager()
        {
            IPermissionManager permissionManager = new PermissionManager(_repositoryFactory);
            return permissionManager;
        }

        public ITagsCreator CreateTagsCreator()
        {
            ISourceJobManager sourceJobManager = new SourceJobManager(_repositoryFactory, _helper);
            ISourceWorkspaceManager sourceWorkspaceManager = new SourceWorkspaceManager(_repositoryFactory, _helper);
            var relativitySourceRdoHelpersFactory = new RelativitySourceRdoHelpersFactory(_repositoryFactory);
            IRelativitySourceJobRdoInitializer sourceJobRdoInitializer = new RelativitySourceJobRdoInitializer(_helper, _repositoryFactory, relativitySourceRdoHelpersFactory);
            IRelativitySourceWorkspaceRdoInitializer sourceWorkspaceRdoInitializer = new RelativitySourceWorkspaceRdoInitializer(_helper, _repositoryFactory, relativitySourceRdoHelpersFactory);
            return new TagsCreator(sourceJobManager, sourceWorkspaceManager, sourceJobRdoInitializer, sourceWorkspaceRdoInitializer, _helper);
        }

        public IInstanceSettingsManager CreateInstanceSettingsManager()
        {
            return new InstanceSettingsManager(_repositoryFactory);
        }

        public ITagSavedSearchManager CreateTaggingSavedSearchManager()
        {
            ITagSavedSearch tagSavedSearch = new TagSavedSearch(_repositoryFactory, new MultiObjectSavedSearchCondition(), _helper);
            ITagSavedSearchFolder tagSavedSearchFolder = new TagSavedSearchFolder(_repositoryFactory, _helper);
            return new TagSavedSearchManager(tagSavedSearch, tagSavedSearchFolder);
        }

        public ISourceWorkspaceTagCreator CreateSourceWorkspaceTagsCreator(SourceConfiguration sourceConfiguration)
        {
            IFederatedInstanceManager federatedInstanceManager = CreateFederatedInstanceManager();
            return new SourceWorkspaceTagCreator(_repositoryFactory.GetDestinationWorkspaceRepository(sourceConfiguration.SourceWorkspaceArtifactId),
                _repositoryFactory.GetWorkspaceRepository(), federatedInstanceManager, _helper.GetLoggerFactory().GetLogger());
        }
    }
}
