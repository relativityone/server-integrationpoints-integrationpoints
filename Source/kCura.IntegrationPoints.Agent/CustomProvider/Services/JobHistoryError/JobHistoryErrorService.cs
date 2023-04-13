using System;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Common.Helpers;
using kCura.IntegrationPoints.Common.Kepler;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Transformers;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services.JobHistoryError
{
    internal class JobHistoryErrorService : IJobHistoryErrorService
    {
        private readonly IGuidService _guidService;
        private readonly IDateTime _dateTime;
        private readonly IKeplerServiceFactory _keplerServiceFactory;

        public JobHistoryErrorService(
            IKeplerServiceFactory keplerServiceFactory,
            IGuidService guidService,
            IDateTime dateTime)
        {
            _keplerServiceFactory = keplerServiceFactory;
            _guidService = guidService;
            _dateTime = dateTime;
        }

        public async Task AddJobErrorAsync(int workspaceId, int jobHistoryId, Exception ex)
        {
            using (IObjectManager objectManager = await _keplerServiceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
            {
                Data.JobHistoryError jobHistoryError = new Data.JobHistoryError
                {
                    ParentArtifactId = jobHistoryId,
                    JobHistory = jobHistoryId,
                    Name = _guidService.NewGuid().ToString(),
                    ErrorType = ErrorTypeChoices.JobHistoryErrorJob,
                    ErrorStatus = ErrorStatusChoices.JobHistoryErrorNew,
                    SourceUniqueID = string.Empty,
                    Error = ex.Message,
                    StackTrace = ex.StackTrace,
                    TimestampUTC = _dateTime.UtcNow
                };

                CreateRequest request = new CreateRequest
                {
                    ParentObject = new RelativityObjectRef { ArtifactID = jobHistoryId },
                    ObjectType = jobHistoryError.ToObjectType(),
                    FieldValues = jobHistoryError.ToFieldValues()
                };

                await objectManager.CreateAsync(workspaceId, request).ConfigureAwait(false);
            }
        }
    }
}
