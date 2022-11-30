using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using kCura.Apps.Common.Utils.Serializers;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Facades.SecretStore.Implementation;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using Relativity.API;
using IFederatedInstanceManager = kCura.IntegrationPoints.Domain.Managers.IFederatedInstanceManager;

namespace kCura.IntegrationPoints.EventHandlers.Installers
{
    [Guid("5E882EE9-9E9D-4AFA-9B2C-EAC6C749A8D4")]
    [Description("Updates the Has Errors field on existing Integration Points.")]
    [RunOnce(true)]
    public class SetHasErrorsField : PostInstallEventHandlerBase
    {
        private IIntegrationPointRepository _integrationPointRepository;
        private IJobHistoryService _jobHistoryService;

        public SetHasErrorsField()
        {
        }

        internal SetHasErrorsField(IIntegrationPointRepository integrationPointRepository, IJobHistoryService jobHistoryService)
        {
            _integrationPointRepository = integrationPointRepository;
            _jobHistoryService = jobHistoryService;
        }

        protected override IAPILog CreateLogger()
        {
            return Helper.GetLoggerFactory().GetLogger().ForContext<SetHasErrorsField>();
        }

        protected override string SuccessMessage
            => "Updating the Has Errors field on the Integration Point object completed successfully";

        protected override string GetFailureMessage(Exception ex)
        {
            return "Updating the Has Errors field on the Integration Point object failed.";
        }

        protected override void Run()
        {
            CreateServices();
            ExecuteInstanced();
        }

        internal void ExecuteInstanced()
        {
            IList<Data.IntegrationPoint> integrationPoints = _integrationPointRepository.ReadAll();

            foreach (Data.IntegrationPoint integrationPoint in integrationPoints)
            {
                UpdateIntegrationPointHasErrorsField(integrationPoint);
            }
        }

        /// <summary>
        ///     It is best to use the Castle Windsor container here instead of manually creating the dependencies.
        ///     TODO: replace the below with the container and resolve the dependencies.
        /// </summary>
        private void CreateServices()
        {
            IServiceContextHelper serviceContextHelper = new ServiceContextHelperForEventHandlers(Helper, Helper.GetActiveCaseID());
            ICaseServiceContext caseServiceContext = new CaseServiceContext(serviceContextHelper);
            IRepositoryFactory repositoryFactory = new RepositoryFactory(Helper, Helper.GetServicesManager());
            ISerializer integrationPointSerializer = new IntegrationPointSerializer(Logger);
            IWorkspaceManager workspaceManager = new WorkspaceManager(repositoryFactory);
            IFederatedInstanceManager federatedInstanceManager = new FederatedInstanceManager();

            _jobHistoryService = new JobHistoryService(
                caseServiceContext.RelativityObjectManagerService.RelativityObjectManager,
                federatedInstanceManager,
                workspaceManager,
                Logger,
                integrationPointSerializer
                );

            ISecretsRepository secretsRepository = new SecretsRepository(
                SecretStoreFacadeFactory_Deprecated.Create(Helper.GetSecretStore, Logger),
                Logger
            );
            _integrationPointRepository = new IntegrationPointRepository(
                caseServiceContext.RelativityObjectManagerService.RelativityObjectManager,
                secretsRepository,
                Logger);
        }

        internal void UpdateIntegrationPointHasErrorsField(IntegrationPoint integrationPoint)
        {
            integrationPoint.HasErrors = false;

            if (integrationPoint.JobHistory.Length > 0)
            {
                IList<JobHistory> jobHistories = _jobHistoryService.GetJobHistory(integrationPoint.JobHistory);

                JobHistory lastCompletedJob = jobHistories?
                    .Where(jobHistory => jobHistory.EndTimeUTC != null)
                    .OrderByDescending(jobHistory => jobHistory.EndTimeUTC)
                    .FirstOrDefault();

                if ((lastCompletedJob != null) && (lastCompletedJob.JobStatus.Name != JobStatusChoices.JobHistoryCompleted.Name))
                {
                    integrationPoint.HasErrors = true;
                }
            }

            _integrationPointRepository.Update(integrationPoint);
        }
    }
}
