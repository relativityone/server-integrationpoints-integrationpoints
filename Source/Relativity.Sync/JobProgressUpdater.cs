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
        private readonly IAPILog _logger;

        public JobProgressUpdater(ISourceServiceFactoryForAdmin serviceFactoryForAdmin, IRdoGuidConfiguration rdoGuidConfiguration, int workspaceArtifactId, int jobHistoryArtifactId, IDateTime dateTime, IJobHistoryErrorRepository jobHistoryErrorRepository, IAPILog logger)
        {
            _serviceFactoryForAdmin = serviceFactoryForAdmin;
            _rdoGuidConfiguration = rdoGuidConfiguration;
            _workspaceArtifactId = workspaceArtifactId;
            _jobHistoryArtifactId = jobHistoryArtifactId;
            _dateTime = dateTime;
            _jobHistoryErrorRepository = jobHistoryErrorRepository;
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
                    Value = _jobHistoryArtifactId.ToString()
                }
            }).ConfigureAwait(false);
        }

        public async Task UpdateJobStatusAsync(JobHistoryStatus status)
        {
            Guid statusGuid = Guid.Empty;
            bool? hasErrors = null;
            DateTime endTime = _dateTime.UtcNow;

            switch (status)
            {
                case JobHistoryStatus.Validating:
                    statusGuid = _rdoGuidConfiguration.JobHistoryStatus.ValidatingGuid;
                    break;
                case JobHistoryStatus.ValidationFailed:
                    statusGuid = _rdoGuidConfiguration.JobHistoryStatus.ValidationFailedGuid;
                    break;
                case JobHistoryStatus.Processing:
                    statusGuid = _rdoGuidConfiguration.JobHistoryStatus.ProcessingGuid;
                    break;
                case JobHistoryStatus.Completed:
                    hasErrors = await HasErrorsAsync().ConfigureAwait(false);
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
                    hasErrors = await HasErrorsAsync().ConfigureAwait(false);
                    break;
                case JobHistoryStatus.Suspending:
                    statusGuid = _rdoGuidConfiguration.JobHistoryStatus.SuspendingGuid;
                    break;
                case JobHistoryStatus.Suspended:
                    statusGuid = _rdoGuidConfiguration.JobHistoryStatus.SuspendedGuid;
                    hasErrors = await HasErrorsAsync().ConfigureAwait(false);
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
                },
                new FieldRefValuePair()
                {
                    Field = new FieldRef()
                    {
                        Guid = _rdoGuidConfiguration.JobHistory.EndTimeGuid
                    },
                    Value = endTime
                }
            };

            await TryUpdateJobHistory(fields).ConfigureAwait(false);
            await TryUpdateIntegrationPoint(hasErrors, endTime).ConfigureAwait(false);
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

        private async Task<bool> HasErrorsAsync()
        {
            try
            {
                using (IObjectManager objectManager = await _serviceFactoryForAdmin.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
                {
                    QueryRequest request = new QueryRequest
                    {
                        ObjectType = new ObjectTypeRef()
                        {
                            Guid = _rdoGuidConfiguration.JobHistoryError.TypeGuid
                        },
                        Condition = $"('{_rdoGuidConfiguration.JobHistoryError.JobHistoryRelationGuid}' IN OBJECT [{_jobHistoryArtifactId}]) AND ('{_rdoGuidConfiguration.JobHistoryError.ErrorTypeGuid}' == CHOICE {_rdoGuidConfiguration.JobHistoryError.ItemLevelErrorGuid})"
                    };

                    QueryResult itemLevelErrors = await objectManager.QueryAsync(_workspaceArtifactId, request, 0, 1).ConfigureAwait(false);
                    _logger.LogInformation("Job history Artifact ID {jobHistoryArtifactId} has item level errors: {hasItemLevelErrors}", _jobHistoryArtifactId, itemLevelErrors.ResultCount > 0);

                    QueryRequest requestForJobHistory = new QueryRequest()
                    {
                        ObjectType = new ObjectTypeRef()
                        {
                            Guid = _rdoGuidConfiguration.JobHistory.TypeGuid
                        },
                        Condition = $"'Artifact ID' == '{_jobHistoryArtifactId}'",
                        Fields = new []
                        {
                            new FieldRef()
                            {
                                Guid = _rdoGuidConfiguration.JobHistory.FailedItemsFieldGuid
                            }
                        }
                    };

                    QueryResult jobHistoryFromQuery = await objectManager.QueryAsync(_workspaceArtifactId, requestForJobHistory, 0, 1).ConfigureAwait(false);
                    RelativityObject jobHistoryObject = jobHistoryFromQuery.Objects.Single();
                    string jobHistoryItemsWithErrorsStr = jobHistoryObject.FieldValues.Single().Value.ToString();
                    int jobHistoryItemsWithErrors = int.Parse(jobHistoryItemsWithErrorsStr);

                    _logger.LogInformation("Job history Artifact ID {jobHistoryArtifactId} failed items field value: {jobHistoryItemsWithErrors}", jobHistoryItemsWithErrors);

                    return itemLevelErrors.ResultCount > 0 || jobHistoryItemsWithErrors > 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to determine if job history has errors");
                return false;
            }
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

        /// <summary>
        /// This is a workaround to update Has Errors and Last Runtime on Integration Point RDO.
        /// </summary>
        private async Task TryUpdateIntegrationPoint(bool? hasErrors, DateTime lastRuntime)
        {
            Guid ripJobHistoryTypeGuid = new Guid("08f4b1f7-9692-4a08-94ab-b5f3a88b6cc9");
            Guid hasErrorsFieldGuid = new Guid("a9853e55-0ba0-43d8-a766-747a61471981");
            Guid lastRuntimeFieldGuid = new Guid("90d58af1-f79f-40ae-85fc-7e42f84dbcc1");

            if (_rdoGuidConfiguration.JobHistory.TypeGuid != ripJobHistoryTypeGuid || !hasErrors.HasValue)
            {
                return;
            }

            try
            {
                using (IObjectManager objectManager = await _serviceFactoryForAdmin.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
                {
                    QueryRequest queryRequest = new QueryRequest()
                    {
                        ObjectType = new ObjectTypeRef()
                        {
                            Guid = ripJobHistoryTypeGuid
                        },
                        Condition = $"'Artifact ID' == {_jobHistoryArtifactId}",
                        Fields = new []
                        {
                            new FieldRef()
                            {
                                Name = "Integration Point"
                            }
                        }
                    };
                    QueryResult queryResult = await objectManager.QueryAsync(_workspaceArtifactId, queryRequest, 0, 1).ConfigureAwait(false);
                    RelativityObject jobHistoryObject = queryResult.Objects.Single();
                    List<RelativityObjectValue> integrationPointFieldValues = (List<RelativityObjectValue>)jobHistoryObject.FieldValues.Single().Value;
                    int integrationPointArtifactId = integrationPointFieldValues.Single().ArtifactID;

                    UpdateRequest updateRequest = new UpdateRequest()
                    {
                        Object = new RelativityObjectRef()
                        {
                            ArtifactID = integrationPointArtifactId
                        },
                        FieldValues = new []
                        {
                            new FieldRefValuePair()
                            {
                                Field = new FieldRef()
                                {
                                    Guid = hasErrorsFieldGuid
                                },
                                Value = hasErrors.Value
                            },
                            new FieldRefValuePair()
                            {
                                Field = new FieldRef()
                                {
                                    Guid = lastRuntimeFieldGuid
                                },
                                Value = lastRuntime
                            }
                        }
                    };
                    await objectManager.UpdateAsync(_workspaceArtifactId, updateRequest).ConfigureAwait(false);
                    _logger.LogInformation("Integration Point ID {integrationPointArtifactId} has been successfully updated", integrationPointArtifactId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update Integration Point. Has Errors: {hasErrors} Last Runtime: {lastRuntime}", hasErrors, lastRuntime);
            }
        }
    }
}
