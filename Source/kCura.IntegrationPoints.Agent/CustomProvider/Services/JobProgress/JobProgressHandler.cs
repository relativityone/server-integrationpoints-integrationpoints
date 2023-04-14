using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Agent.CustomProvider.DTO;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.JobHistory;
using kCura.IntegrationPoints.Agent.CustomProvider.Utils;
using kCura.IntegrationPoints.Common.Helpers;
using kCura.IntegrationPoints.Common.Kepler;
using Relativity.API;
using Relativity.Import.V1;
using Relativity.Import.V1.Models;
using Relativity.Import.V1.Services;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services.JobProgress
{
    public class JobProgressHandler : IJobProgressHandler
    {
        private readonly IKeplerServiceFactory _serviceFactory;
        private readonly IJobHistoryService _jobHistoryService;
        private readonly ITimerFactory _timerFactory;
        private readonly IAPILog _logger;

        public JobProgressHandler(IKeplerServiceFactory serviceFactory, IJobHistoryService jobHistoryService, ITimerFactory timerFactory, IAPILog logger)
        {
            _serviceFactory = serviceFactory;
            _jobHistoryService = jobHistoryService;
            _timerFactory = timerFactory;
            _logger = logger.ForContext<JobProgressHandler>();
        }

        public async Task BeginAsync(int workspaceId, CustomProviderJobDetails jobDetails)
        {
            int numberOfReadItems = jobDetails
                .Batches
                .Where(x => x.IsAddedToImportQueue)
                .Sum(x => x.NumberOfRecords);

            Progress importJobProgress = await GetImportJobProgressAsync(workspaceId, jobDetails.ImportJobID).ConfigureAwait(false);

        }

        private async Task<Progress> GetImportJobProgressAsync(int workspaceId, Guid importJobId)
        {
            using (IImportJobController jobController = await _serviceFactory.CreateProxyAsync<IImportJobController>().ConfigureAwait(false))
            {
                ValueResponse<ImportProgress> response = await jobController.GetProgressAsync(workspaceId, importJobId).ConfigureAwait(false);
                ImportProgress progress = response.UnwrapOrThrow();
                return new Progress(0, progress.ErroredRecords, progress.ImportedRecords);
            }
        }
    }
}
