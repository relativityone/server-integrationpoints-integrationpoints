using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using kCura.Apps.Common.Utils.Serializers;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using Relativity.API;
using Relativity.DataTransfer.MessageService;

namespace kCura.IntegrationPoints.EventHandlers.Installers
{
    [Description("Update Job History with Relativity Instance information")]
    [RunOnce(true)]
    [Guid("72BDF6BC-A222-4D53-B470-F9A521F22121")]
    public class UpdateJobHistoryDestinationWorkspace : PostInstallEventHandlerBase
    {
        private IJobHistoryService _jobHistoryService;
        private IDestinationParser _destinationParser;

        public UpdateJobHistoryDestinationWorkspace()
        {
        }

        protected override IAPILog CreateLogger()
        {
            return Helper.GetLoggerFactory().GetLogger().ForContext<UpdateJobHistoryDestinationWorkspace>();
        }

        protected override string SuccessMessage => "Updating Job History/Destination Workspace update completed";

        protected override string GetFailureMessage(Exception ex)
        {
            return "Updating Job History/Destination Workspace failed";
        }

        protected override void Run()
        {
            ResolveDependencies();
            ExecuteInternal();
        }

        internal UpdateJobHistoryDestinationWorkspace(IJobHistoryService jobHistoryService,
            IDestinationParser destinationParser)
        {
            _jobHistoryService = jobHistoryService;
            _destinationParser = destinationParser;
        }

        internal void ExecuteInternal()
        {
            IList<Data.JobHistory> jobHistories = _jobHistoryService.GetAll();

            foreach (Data.JobHistory jobHistory in jobHistories)
            {
                string[] elements = _destinationParser.GetElements(jobHistory.DestinationWorkspace);

                if (elements.Length == 2)
                {
                    jobHistory.DestinationWorkspace = FederatedInstanceManager.LocalInstance.Name + " - " + jobHistory.DestinationWorkspace;
                    _jobHistoryService.UpdateRdoWithoutDocuments(jobHistory);
                }
            }
        }

        private void ResolveDependencies()
        {
            _jobHistoryService = CreateJobHistoryService();
            _destinationParser = new DestinationParser();
        }

        private IJobHistoryService CreateJobHistoryService()
        {
            var caseContext = ServiceContextFactory.CreateCaseServiceContext(Helper, Helper.GetActiveCaseID());
            IRepositoryFactory repositoryFactory = new RepositoryFactory(Helper, Helper.GetServicesManager());
            IWorkspaceManager workspaceManager = new WorkspaceManager(repositoryFactory);
            ISerializer serializer = new IntegrationPointSerializer(Logger);
            IProviderTypeService providerTypeService = new ProviderTypeService(CreateObjectManager(Helper, Helper.GetActiveCaseID()));
            IMessageService messageService = new MessageService();
            return new JobHistoryService(
                caseContext.RelativityObjectManagerService.RelativityObjectManager,
                workspaceManager,
                Logger,
                serializer);
        }

        private IRelativityObjectManager CreateObjectManager(IEHHelper helper, int workspaceId)
        {
            return new RelativityObjectManagerFactory(helper).CreateRelativityObjectManager(workspaceId);
        }
    }
}
