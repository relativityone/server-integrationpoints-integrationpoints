using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Storage;
using Relativity.Sync.Utils;
using Relativity.Sync.Utils.Workarounds;

namespace Relativity.Sync
{
    internal sealed class JobProgressUpdater : IJobProgressUpdater
    {
        private readonly int _workspaceArtifactId;
        private readonly int _jobHistoryArtifactId;
        private readonly IDateTime _dateTime;
        private readonly IJobHistoryErrorRepository _jobHistoryErrorRepository;
        private readonly ISourceServiceFactoryForAdmin _serviceFactoryForAdmin;
        private readonly IRdoGuidConfiguration _rdoGuidConfiguration;
        private readonly IRipWorkarounds _ripWorkarounds;
        private readonly IAPILog _logger;

        public JobProgressUpdater(ISourceServiceFactoryForAdmin serviceFactoryForAdmin, IRdoGuidConfiguration rdoGuidConfiguration, int workspaceArtifactId, int jobHistoryArtifactId, IDateTime dateTime, IJobHistoryErrorRepository jobHistoryErrorRepository, IRipWorkarounds ripWorkarounds, IAPILog logger)
        {
            _serviceFactoryForAdmin = serviceFactoryForAdmin;
            _rdoGuidConfiguration = rdoGuidConfiguration;
            _workspaceArtifactId = workspaceArtifactId;
            _jobHistoryArtifactId = jobHistoryArtifactId;
            _dateTime = dateTime;
            _jobHistoryErrorRepository = jobHistoryErrorRepository;
            _ripWorkarounds = ripWorkarounds;
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
            try
            {
                using (IObjectManager objectManager = await _serviceFactoryForAdmin.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
                {
                    QueryRequest request = new QueryRequest()
                    {
                        ObjectType = new ObjectTypeRef()
                        {
                            Guid = _rdoGuidConfiguration.JobHistory.TypeGuid
                        },
                        Condition = $"'Artifact ID' == {_jobHistoryArtifactId}",
                        Fields = new[]
                        {
                            new FieldRef()
                            {
                                Guid = _rdoGuidConfiguration.JobHistory.JobIdGuid
                            }
                        }
                    };

                    QueryResult result = await objectManager.QueryAsync(_workspaceArtifactId, request, 0, 1).ConfigureAwait(false);
                    string jobId = result?.Objects?.FirstOrDefault()?.FieldValues.FirstOrDefault()?.Value?.ToString();

                    if (string.IsNullOrWhiteSpace(jobId))
                    {
                        // RIP didn't set Job ID which means we're executing on Sync Agent

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
                                Value = _jobHistoryArtifactId.ToString()
                            }
                        }).ConfigureAwait(false);
                    }
                    else
                    {
                        _logger.LogInformation("Job History has already Job ID set: {jobId}", jobId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update Job History: {jobHistoryArtifactId}", _jobHistoryArtifactId);
            }
        }

        public async Task UpdateJobStatusAsync(JobHistoryStatus status)
        {
            try
            {
                Guid statusGuid = Guid.Empty;
                bool? hasErrors = null;

                _logger.LogInformation("Updating Job History status: {status}", status);

                switch (status)
                {
                    case JobHistoryStatus.Validating:
                        statusGuid = _rdoGuidConfiguration.JobHistoryStatus.ValidatingGuid;
                        break;
                    case JobHistoryStatus.ValidationFailed:
                        hasErrors = true;
                        statusGuid = _rdoGuidConfiguration.JobHistoryStatus.ValidationFailedGuid;
                        break;
                    case JobHistoryStatus.Processing:
                        statusGuid = _rdoGuidConfiguration.JobHistoryStatus.ProcessingGuid;
                        break;
                    case JobHistoryStatus.Completed:
                        hasErrors = await _jobHistoryErrorRepository.HasErrorsAsync(_workspaceArtifactId, _jobHistoryArtifactId).ConfigureAwait(false);
                        if (hasErrors == true)
                        {
                            statusGuid = _rdoGuidConfiguration.JobHistoryStatus.CompletedWithErrorsGuid;
                        }
                        else
                        {
                            statusGuid = _rdoGuidConfiguration.JobHistoryStatus.CompletedGuid;
                        }

                        break;
                    case JobHistoryStatus.CompletedWithErrors:
                        statusGuid = _rdoGuidConfiguration.JobHistoryStatus.CompletedWithErrorsGuid;
                        hasErrors = true;
                        break;
                    case JobHistoryStatus.Failed:
                        statusGuid = _rdoGuidConfiguration.JobHistoryStatus.JobFailedGuid;
                        hasErrors = true;
                        break;
                    case JobHistoryStatus.Stopping:
                        statusGuid = _rdoGuidConfiguration.JobHistoryStatus.StoppingGuid;
                        break;
                    case JobHistoryStatus.Stopped:
                        statusGuid = _rdoGuidConfiguration.JobHistoryStatus.StoppedGuid;
                        hasErrors = await _jobHistoryErrorRepository.HasErrorsAsync(_workspaceArtifactId, _jobHistoryArtifactId).ConfigureAwait(false);
                        break;
                    case JobHistoryStatus.Suspending:
                        statusGuid = _rdoGuidConfiguration.JobHistoryStatus.SuspendingGuid;
                        break;
                    case JobHistoryStatus.Suspended:
                        statusGuid = _rdoGuidConfiguration.JobHistoryStatus.SuspendedGuid;
                        hasErrors = await _jobHistoryErrorRepository.HasErrorsAsync(_workspaceArtifactId, _jobHistoryArtifactId).ConfigureAwait(false);
                        break;
                    default:
                        throw new InvalidOperationException($"Invalid job status GUID: {statusGuid}");
                }

                List<FieldRefValuePair> fields = new List<FieldRefValuePair>
                {
                    new FieldRefValuePair()
                    {
                        Field = new FieldRef()
                        {
                            Guid = _rdoGuidConfiguration.JobHistory.StatusGuid
                        },
                        Value = new ChoiceRef()
                        {
                            Guid = statusGuid
                        }
                    }
                };

                switch (status)
                {
                    case JobHistoryStatus.Stopped:
                    case JobHistoryStatus.Completed:
                    case JobHistoryStatus.CompletedWithErrors:
                    case JobHistoryStatus.Failed:
                    case JobHistoryStatus.ValidationFailed:
                    case JobHistoryStatus.Suspended:
                    {
                        DateTime endTime = _dateTime.UtcNow;

                        await _ripWorkarounds.TryUpdateIntegrationPointAsync(_workspaceArtifactId, _jobHistoryArtifactId, hasErrors, endTime);

                        fields.Add(new FieldRefValuePair()
                        {
                            Field = new FieldRef()
                            {
                                Guid = _rdoGuidConfiguration.JobHistory.EndTimeGuid
                            },
                            Value = endTime
                        });
                        break;
                    }
                }

                await TryUpdateJobHistory(fields).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update job status. Job History Artifact ID: {jobHistoryArtifactId} Status: {status}", _jobHistoryArtifactId, status);
                throw;
            }
        }

        public async Task AddJobErrorAsync(Exception ex)
        {
            await _jobHistoryErrorRepository.CreateAsync(_workspaceArtifactId, _jobHistoryArtifactId, new CreateJobHistoryErrorDto(ErrorType.Job)
            {
                ErrorMessage = ex.GetExceptionMessages(),
                StackTrace = ex.StackTrace
            }).ConfigureAwait(false);
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
