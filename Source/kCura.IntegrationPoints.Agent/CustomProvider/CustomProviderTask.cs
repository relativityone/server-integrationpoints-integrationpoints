using System;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Agent.CustomProvider.DTO;
using kCura.IntegrationPoints.Agent.CustomProvider.Services;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.JobCancellation;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.JobDetails;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.SourceProvider;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts.Provider;
using Relativity.Sync;

namespace kCura.IntegrationPoints.Agent.CustomProvider
{
    internal class CustomProviderTask : ICustomProviderTask
    {
        private readonly ICancellationTokenFactory _cancellationTokenFactory;
        private readonly IAgentValidator _agentValidator;
        private readonly IJobDetailsService _jobDetailsService;
        private readonly IIntegrationPointService _integrationPointService;
        private readonly ISourceProviderService _sourceProviderService;
        private readonly IImportJobRunner _importJobRunner;
        private readonly IAPILog _logger;

        public CustomProviderTask(ICancellationTokenFactory cancellationTokenFactory, IAgentValidator agentValidator, IJobDetailsService jobDetailsService, IIntegrationPointService integrationPointService, ISourceProviderService sourceProviderService, IImportJobRunner importJobRunner, IAPILog logger)
        {
            _cancellationTokenFactory = cancellationTokenFactory;
            _agentValidator = agentValidator;
            _jobDetailsService = jobDetailsService;
            _integrationPointService = integrationPointService;
            _sourceProviderService = sourceProviderService;
            _importJobRunner = importJobRunner;
            _logger = logger;
        }

        public void Execute(Job job)
        {
            ExecuteAsync(job).GetAwaiter().GetResult();
        }

        private async Task ExecuteAsync(Job job)
        {
            try
            {
                IntegrationPointDto integrationPointDto = _integrationPointService.Read(job.RelatedObjectArtifactID);

                _agentValidator.Validate(integrationPointDto, job.SubmittedBy);

                CustomProviderJobDetails jobDetails = await _jobDetailsService.GetJobDetailsAsync(job.WorkspaceID, job.JobDetails).ConfigureAwait(false);
                IDataSourceProvider sourceProvider = await _sourceProviderService.GetSourceProviderAsync(job.WorkspaceID, integrationPointDto.SourceProvider);

                CompositeCancellationToken token = _cancellationTokenFactory.GetCancellationToken(jobDetails.JobHistoryGuid, job.JobId);

                await _importJobRunner
                    .RunJobAsync(job, jobDetails, integrationPointDto, sourceProvider, token)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute Custom Provider job.");
                throw;
            }
        }
    }
}
