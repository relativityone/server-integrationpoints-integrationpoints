using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Utils;

namespace Relativity.Sync
{
    internal sealed class JobProgressUpdater : IJobProgressUpdater
    {
        private readonly int _workspaceArtifactId;
        private readonly int _jobHistoryArtifactId;
        private readonly IDateTime _dateTime;
        private readonly ISourceServiceFactoryForAdmin _serviceFactoryForAdmin;
        private readonly IRdoGuidConfiguration _rdoGuidConfiguration;
        private readonly IAPILog _logger;

        public JobProgressUpdater(ISourceServiceFactoryForAdmin serviceFactoryForAdmin, IRdoGuidConfiguration rdoGuidConfiguration, int workspaceArtifactId, int jobHistoryArtifactId, IDateTime dateTime, IAPILog logger)
        {
            _serviceFactoryForAdmin = serviceFactoryForAdmin;
            _rdoGuidConfiguration = rdoGuidConfiguration;
            _workspaceArtifactId = workspaceArtifactId;
            _jobHistoryArtifactId = jobHistoryArtifactId;
            _dateTime = dateTime;
            _logger = logger;
        }

        public async Task SetTotalItemsCountAsync(int totalItemsCount)
        {
            await TryUpdateJobHistory(new[]
            {
                new FieldRefValuePair()
                {
                    Field = new FieldRef()
                    {
                        Guid = _rdoGuidConfiguration.JobHistory.TotalItemsFieldGuid
                    },
                    Value = totalItemsCount
                }
            }).ConfigureAwait(false);
        }

        public async Task SetJobStartedAsync()
        {
            await TryUpdateJobHistory(new[]
            {
                new FieldRefValuePair()
                {
                    Field = new FieldRef()
                    {
                        Guid = _rdoGuidConfiguration.JobHistory.StartTimeGuid
                    },
                    Value = _dateTime.UtcNow
                },
                new FieldRefValuePair()
                {
                    Field = new FieldRef()
                    {
                        Guid = _rdoGuidConfiguration.JobHistory.JobIdGuid
                    },
                    Value = _jobHistoryArtifactId
                }
            }).ConfigureAwait(false);
        }

        public async Task UpdateJobStatusAsync(JobHistoryStatus status, Exception ex)
        {

        }

        public async Task UpdateJobProgressAsync(int completedRecordsCount, int failedRecordsCount)
        {
            await TryUpdateJobHistory(new[]
            {
                new FieldRefValuePair()
                {
                    Field = new FieldRef()
                    {
                        Guid = _rdoGuidConfiguration.JobHistory.CompletedItemsFieldGuid
                    },
                    Value = completedRecordsCount
                },
                new FieldRefValuePair()
                {
                    Field = new FieldRef()
                    {
                        Guid = _rdoGuidConfiguration.JobHistory.FailedItemsFieldGuid
                    },
                    Value = failedRecordsCount
                },
            }).ConfigureAwait(false);
        }

        private async Task TryUpdateJobHistory(IEnumerable<FieldRefValuePair> fieldValues)
        {
            try
            {
                using (IObjectManager objectManager = await _serviceFactoryForAdmin.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
                {
                    UpdateRequest updateRequest = new UpdateRequest()
                    {
                        Object = new RelativityObjectRef()
                        {
                            ArtifactID = _jobHistoryArtifactId
                        },
                        FieldValues = fieldValues
                    };
                    await objectManager.UpdateAsync(_workspaceArtifactId, updateRequest).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update job history: {artifactId}", _jobHistoryArtifactId);
            }
        }
    }
}
