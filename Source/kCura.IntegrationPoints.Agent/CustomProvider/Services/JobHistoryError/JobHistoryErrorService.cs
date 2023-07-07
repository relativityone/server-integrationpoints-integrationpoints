using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Common.Handlers;
using kCura.IntegrationPoints.Common.Helpers;
using kCura.IntegrationPoints.Common.Kepler;
using kCura.IntegrationPoints.Core.Extensions;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Transformers;
using kCura.IntegrationPoints.Domain.Exceptions;
using Relativity.API;
using Relativity.Services.Exceptions;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services.JobHistoryError
{
    internal class JobHistoryErrorService : IJobHistoryErrorService
    {
        private const string _REQUEST_ENTITY_TOO_LARGE_EXCEPTION = "Request Entity Too Large";

        private readonly IGuidService _guidService;
        private readonly IDateTime _dateTime;
        private readonly IRetryHandler _retryHandler;
        private readonly IKeplerServiceFactory _keplerServiceFactory;
        private readonly IAPILog _log;

        public JobHistoryErrorService(
            IKeplerServiceFactory keplerServiceFactory,
            IGuidService guidService,
            IDateTime dateTime,
            IRetryHandler retryHandler,
            IAPILog log)
        {
            _keplerServiceFactory = keplerServiceFactory;
            _guidService = guidService;
            _dateTime = dateTime;
            _retryHandler = retryHandler;
            _log = log;
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

        public async Task CreateItemLevelErrorsAsync(int workspaceId, int jobHistoryId, IList<ItemLevelError> errors)
        {
            using (IObjectManager objectManager = await _keplerServiceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
            {
                List<List<object>> values = errors.Select(x => new List<object>()
                {
                    x.ErrorMessage,
                    GetErrorStatusChoice(),
                    GetErrorTypeChoice(ErrorType.Item),
                    Guid.NewGuid().ToString(),
                    x.SourceUniqueId,
                    _dateTime.UtcNow
                }).ToList();

                MassCreateRequest request = new MassCreateRequest
                {
                    ObjectType = new ObjectTypeRef { Guid = ObjectTypeGuids.JobHistoryErrorGuid },
                    ParentObject = new RelativityObjectRef { ArtifactID = jobHistoryId },
                    Fields = GetFields(),
                    ValueLists = values
                };

                await _retryHandler.ExecuteWithRetriesAsync(async () =>
                {
                    try
                    {
                        MassCreateResult result = await objectManager.CreateAsync(workspaceId, request).ConfigureAwait(false);

                        if (!result.Success || result.Objects?.Count == 0)
                        {
                            _log.LogError("Mass creation of item level errors failed. Message - {@result}", result);
                            throw new IntegrationPointsException($"Mass creation of item level errors failed. Message: {result.Message}");
                        }
                    }
                    catch (ServiceException ex) when (ex.Message.Contains(_REQUEST_ENTITY_TOO_LARGE_EXCEPTION))
                    {
                        _log.LogWarning(
                            ex,
                            "Job History Errors mass creation failed. Attempt to retry by creating errors in chunks");
                        await CreateItemLevelErrorsInBatchesAsync(workspaceId, jobHistoryId, errors).ConfigureAwait(false);
                    }
                }).ConfigureAwait(false);
            }
        }

        public async Task<Data.JobHistoryError> GetLastJobLevelError(int workspaceId, int jobHistoryId)
        {
            string historyCondition = $"'{JobHistoryErrorFields.JobHistory}' == OBJECT {jobHistoryId}";
            string expectedChoiceGuids = string.Join(",", ErrorTypeChoices.JobHistoryErrorJob.Guids.Select(x => x.ToString()));
            string choiceJobErrorCondition = $"'{JobHistoryErrorFields.ErrorType}' IN CHOICE [{expectedChoiceGuids}]";
            string condition = $"{historyCondition} AND {choiceJobErrorCondition}";

            Data.JobHistoryError jobHistoryError = null;
            try
            {
                QueryRequest query = new QueryRequest
                {
                    ObjectType = new ObjectTypeRef { Guid = ObjectTypeGuids.JobHistoryErrorGuid },
                    Fields = GetFields(),
                    Sorts = new List<Sort>()
                    {
                        new Sort
                        {
                          Direction = SortEnum.Descending,
                          FieldIdentifier = new FieldRef { Name = "Artifact ID" }
                        }
                    },
                    Condition = condition
                };

                using (IObjectManager objectManager = await _keplerServiceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
                {
                    QueryResult result = await objectManager.QueryAsync(workspaceId, query, 0, 1).ConfigureAwait(false);
                    RelativityObject relativityObject = result.Objects.FirstOrDefault();
                    jobHistoryError = relativityObject?.ToRDO<Data.JobHistoryError>();
                }
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "Error on GetLastJobLevelError occurred for job history ID: {id}", jobHistoryId);
            }

            return jobHistoryError;
        }

        private async Task CreateItemLevelErrorsInBatchesAsync(int workspaceArtifactId, int jobHistoryArtifactId, IList<ItemLevelError> createJobHistoryErrorDtos)
        {
            const double numOfBatches = 2;
            int batchSize = (int)Math.Ceiling(createJobHistoryErrorDtos.Count() / numOfBatches);

            if (batchSize == createJobHistoryErrorDtos.Count())
            {
                throw new IntegrationPointsException(
                    $"Mass creation of item level errors failed, because single item is still to large");
            }

            foreach (IList<ItemLevelError> errorsBatchList in createJobHistoryErrorDtos.SplitList(batchSize))
            {
                await CreateItemLevelErrorsAsync(workspaceArtifactId, jobHistoryArtifactId, errorsBatchList)
                    .ConfigureAwait(false);
            }
        }

        private FieldRef[] GetFields()
        {
            return new[]
            {
                new FieldRef { Guid = JobHistoryErrorFieldGuids.ErrorGuid },
                new FieldRef { Guid = JobHistoryErrorFieldGuids.ErrorStatusGuid },
                new FieldRef { Guid = JobHistoryErrorFieldGuids.ErrorTypeGuid },
                new FieldRef { Guid = JobHistoryErrorFieldGuids.NameGuid },
                new FieldRef { Guid = JobHistoryErrorFieldGuids.SourceUniqueIDGuid },
                new FieldRef { Guid = JobHistoryErrorFieldGuids.TimestampUTCGuid }
            };
        }

        private ChoiceRef GetErrorStatusChoice()
        {
            ChoiceRef errorStatusChoice = new ChoiceRef
            {
                Guid = ErrorStatusChoices.JobHistoryErrorNewGuid
            };
            return errorStatusChoice;
        }

        private ChoiceRef GetErrorTypeChoice(ErrorType type)
        {
            switch (type)
            {
                case ErrorType.Job:
                    return new ChoiceRef { Guid = ErrorTypeChoices.JobHistoryErrorJobGuid };
                case ErrorType.Item:
                    return new ChoiceRef { Guid = ErrorTypeChoices.JobHistoryErrorItemGuid };
                default:
                    throw new InvalidInputException($"Error Type is not supported - {type}");
            }
        }
    }
}
