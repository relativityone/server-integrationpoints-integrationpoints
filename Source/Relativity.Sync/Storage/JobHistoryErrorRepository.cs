using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;
using Relativity.API;
using Relativity.Services.Exceptions;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Storage
{
    internal sealed class JobHistoryErrorRepository : IJobHistoryErrorRepository
    {
        public double SecondsBetweenRetriesBase { get; set; }

        private const int _MAX_NUMBER_OF_RETRIES = 3;
        private const string _REQUEST_ENTITY_TOO_LARGE_EXCEPTION = "Request Entity Too Large";
        private const string ViolationOfPrimaryKeyConstraint = "Violation of PRIMARY KEY constraint";
        private const string CannotInsertDuplicateKeyInObject = "Cannot insert duplicate key in object";

        private readonly IDateTime _dateTime;
        private readonly ISourceServiceFactoryForUser _serviceFactoryForUser;
        private readonly IJobHistoryErrorRepositoryConfigration _repositoryConfigration;
        private readonly IRdoGuidConfiguration _rdoConfiguration;
        private readonly IAPILog _logger;
        private readonly IRandom _random;

        public JobHistoryErrorRepository(ISourceServiceFactoryForUser serviceFactoryForUser, IJobHistoryErrorRepositoryConfigration repositoryConfigration,
            IRdoGuidConfiguration rdoConfiguration, IDateTime dateTime, IAPILog logger, IRandom random)
        {
            _serviceFactoryForUser = serviceFactoryForUser;
            _repositoryConfigration = repositoryConfigration;
            _rdoConfiguration = rdoConfiguration;
            _dateTime = dateTime;
            _logger = logger;
            _random = random;
        }

        public async Task<IEnumerable<int>> MassCreateAsync(int workspaceArtifactId, int jobHistoryArtifactId,
            IList<CreateJobHistoryErrorDto> createJobHistoryErrorDtos)
        {
            CreateJobHistoryErrorDto[] filteredErrors = createJobHistoryErrorDtos
                .Where(x => _repositoryConfigration.LogItemLevelErrors || x.ErrorType == ErrorType.Job)
                .ToArray();

            if (!filteredErrors.Any())
            {
                return Enumerable.Empty<int>();
            }

            _logger.LogInformation("Mass creating errors count: {count}", filteredErrors.Length);
            return await MassCreateAsyncInternal(workspaceArtifactId, jobHistoryArtifactId, filteredErrors);
        }

        public async Task<int> CreateAsync(int workspaceArtifactId, int jobHistoryArtifactId,
            CreateJobHistoryErrorDto createJobHistoryErrorDto)
        {
            IEnumerable<int> massCreateResult = await MassCreateAsync(workspaceArtifactId, jobHistoryArtifactId,
                new List<CreateJobHistoryErrorDto> { createJobHistoryErrorDto }).ConfigureAwait(false);
            return massCreateResult.First();
        }

        public async Task<IJobHistoryError> GetLastJobErrorAsync(int workspaceArtifactId, int jobHistoryArtifactId)
        {
            IJobHistoryError jobHistoryError = null;

            using (IObjectManager objectManager = await _serviceFactoryForUser.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
            {
                ReadRequest readRequest = new ReadRequest
                {
                    Object = new RelativityObjectRef
                    {
                        Guid = _rdoConfiguration.JobHistoryError.JobLevelErrorGuid
                    }
                };
                ReadResult jobErrorType =
                    await objectManager.ReadAsync(workspaceArtifactId, readRequest).ConfigureAwait(false);
                QueryRequest request = new QueryRequest
                {
                    ObjectType = new ObjectTypeRef { Guid = _rdoConfiguration.JobHistoryError.TypeGuid },
                    Condition =
                        $"'{_rdoConfiguration.JobHistoryError.JobHistoryRelationGuid}' == OBJECT {jobHistoryArtifactId} AND '{_rdoConfiguration.JobHistoryError.ErrorTypeGuid}' == CHOICE {jobErrorType.Object.ArtifactID}",
                    Fields = GetFields(),
                    Sorts = new[]
                    {
                        new Sort
                        {
                            Direction = SortEnum.Descending,
                            FieldIdentifier = new FieldRef { Guid = _rdoConfiguration.JobHistoryError.TimeStampGuid }
                        }
                    }
                };
                QueryResult result = await objectManager.QueryAsync(workspaceArtifactId, request, 0, 1)
                    .ConfigureAwait(false);
                if (result.TotalCount > 0)
                {
                    RelativityObject jobError = result.Objects.First();

                    int artifactId = jobError.ArtifactID;
                    string errorMessage = (string)jobError[_rdoConfiguration.JobHistoryError.ErrorMessagesGuid].Value;
                    ErrorStatus errorStatus =
                        ((Choice)jobError[_rdoConfiguration.JobHistoryError.ErrorStatusGuid].Value).Name
                        .GetEnumFromDescription<ErrorStatus>();
                    ErrorType errorType = ((Choice)jobError[_rdoConfiguration.JobHistoryError.ErrorTypeGuid].Value)
                        .Name.GetEnumFromDescription<ErrorType>();
                    string name = (string)jobError[_rdoConfiguration.JobHistoryError.NameGuid].Value;
                    string sourceUniqueId =
                        (string)jobError[_rdoConfiguration.JobHistoryError.SourceUniqueIdGuid].Value;
                    string stackTrace = (string)jobError[_rdoConfiguration.JobHistoryError.StackTraceGuid].Value;
                    DateTime timestampUtc = (DateTime)jobError[_rdoConfiguration.JobHistoryError.TimeStampGuid].Value;

                    jobHistoryError = new JobHistoryError(artifactId, errorMessage, errorStatus, errorType,
                        jobHistoryArtifactId, name, sourceUniqueId, stackTrace, timestampUtc);
                }
            }

            return jobHistoryError;
        }

        public async Task<bool> HasErrorsAsync(int workspaceArtifactId, int jobHistoryArtifactId)
        {
            try
            {
                using (IObjectManager objectManager = await _serviceFactoryForUser.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
                {
                    QueryRequest request = new QueryRequest
                    {
                        ObjectType = new ObjectTypeRef()
                        {
                            Guid = _rdoConfiguration.JobHistoryError.TypeGuid
                        },
                        Condition = $"('{_rdoConfiguration.JobHistoryError.JobHistoryRelationGuid}' IN OBJECT [{jobHistoryArtifactId}]) AND ('{_rdoConfiguration.JobHistoryError.ErrorTypeGuid}' == CHOICE {_rdoConfiguration.JobHistoryError.ItemLevelErrorGuid})"
                    };
                    QueryResult itemLevelErrors = await objectManager.QueryAsync(workspaceArtifactId, request, 0, 1).ConfigureAwait(false);
                    _logger.LogInformation("Job history Artifact ID {jobHistoryArtifactId} has item level errors: {hasItemLevelErrors}", jobHistoryArtifactId, itemLevelErrors.ResultCount > 0);

                    QueryRequest requestForJobHistory = new QueryRequest()
                    {
                        ObjectType = new ObjectTypeRef()
                        {
                            Guid = _rdoConfiguration.JobHistory.TypeGuid
                        },
                        Condition = $"'Artifact ID' == '{jobHistoryArtifactId}'",
                        Fields = new[]
                        {
                            new FieldRef()
                            {
                                Guid = _rdoConfiguration.JobHistory.FailedItemsFieldGuid
                            }
                        }
                    };

                    QueryResult jobHistoryFromQuery = await objectManager.QueryAsync(workspaceArtifactId, requestForJobHistory, 0, 1).ConfigureAwait(false);
                    RelativityObject jobHistoryObject = jobHistoryFromQuery.Objects.Single();
                    string jobHistoryItemsWithErrorsStr = jobHistoryObject.FieldValues.Single().Value.ToString();
                    int jobHistoryItemsWithErrors = int.Parse(jobHistoryItemsWithErrorsStr);

                    _logger.LogInformation("Job history Artifact ID {jobHistoryArtifactId} failed items field value: {jobHistoryItemsWithErrors}", jobHistoryArtifactId, jobHistoryItemsWithErrors);

                    return itemLevelErrors.ResultCount > 0 || jobHistoryItemsWithErrors > 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to determine if job history {jobHistoryArtifactId} has errors", jobHistoryArtifactId);
                throw;
            }
        }

        private async Task<IEnumerable<int>> MassCreateAsyncInternal(int workspaceArtifactId, int jobHistoryArtifactId,
            IList<CreateJobHistoryErrorDto> createJobHistoryErrorDtos)
        {
            using (IObjectManager objectManager =
                   await _serviceFactoryForUser.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
            {
                List<List<object>> values = createJobHistoryErrorDtos.Select(x => new List<object>()
                {
                    x.ErrorMessage,
                    GetErrorStatusChoice(ErrorStatus.New),
                    GetErrorTypeChoice(x.ErrorType),
                    Guid.NewGuid().ToString(),
                    x.SourceUniqueId,
                    x.StackTrace,
                    _dateTime.UtcNow
                }).ToList();

                try
                {
                    IEnumerable<int> artifactIds = new List<int>();
                    MassCreateRequest request = new MassCreateRequest
                    {
                        ObjectType = GetObjectTypeRef(),
                        ParentObject = GetParentObject(jobHistoryArtifactId),
                        Fields = GetFields(),
                        ValueLists = values
                    };

                    RetryPolicy massCreateRetryPolicy = GetMassCreateRetryPolicy();

                    await massCreateRetryPolicy.ExecuteAsync(async () =>
                    {
                        MassCreateResult result = await objectManager.CreateAsync(workspaceArtifactId, request)
                            .ConfigureAwait(false);

                        if (!result.Success)
                        {
                            throw new SyncException(
                                $"Mass creation of item level errors was not successful. Message: {result.Message}");
                        }

                        if (result.Objects != null && result.Objects.Count < 1 && createJobHistoryErrorDtos.Count > 0)
                        {
                            throw new SyncException(
                                $"No objects were created while Mass creation of item level errors. Message: {result.Message}");
                        }

                        _logger.LogInformation(
                            "Successfully mass-created item level errors: {count}",
                            createJobHistoryErrorDtos.Count);
                        artifactIds = result.Objects.Select(x => x.ArtifactID);
                    }).ConfigureAwait(false);

                    return artifactIds;
                }
                catch (SyncException)
                {
                    throw new SyncException(
                        $"Maximum number of retries ({_MAX_NUMBER_OF_RETRIES}) has been reached for Mass creation of item level errors.");
                }
                catch (ServiceException ex) when (ex.Message.Contains(_REQUEST_ENTITY_TOO_LARGE_EXCEPTION))
                {
                    _logger.LogWarning(
                        ex,
                        "Job History Errors mass creation failed. Attempt to retry by creating errors in chunks");
                    return await MassCreateInBatchesAsync(workspaceArtifactId, jobHistoryArtifactId,
                        createJobHistoryErrorDtos).ConfigureAwait(false);
                }
            }
        }

        private RetryPolicy GetMassCreateRetryPolicy()
        {
            RetryPolicy massCreateRetryPolicy = Policy
                .Handle<SyncException>()
                .Or<Relativity.Services.Exceptions.ServiceException>(ex => ex.Message.Contains(ViolationOfPrimaryKeyConstraint) && ex.Message.Contains(CannotInsertDuplicateKeyInObject))
                .WaitAndRetryAsync(_MAX_NUMBER_OF_RETRIES, retryAttempt =>
                    {
                        const int maxJitterMs = 100;
                        TimeSpan delay = TimeSpan.FromSeconds(Math.Pow(SecondsBetweenRetriesBase, retryAttempt));
                        TimeSpan jitter = TimeSpan.FromMilliseconds(_random.Next(0, maxJitterMs));
                        return delay + jitter;
                    },
                    (ex, waitTime, retryCount, context) =>
                    {
                        _logger.LogWarning(
                            ex,
                            "Encountered issue while Mass creation of item level errors, attempting to retry. Retry count: {retryCount} Wait time: {waitTimeMs} (ms)",
                            retryCount, waitTime.TotalMilliseconds);
                    });

            return massCreateRetryPolicy;
        }

        private async Task<IEnumerable<int>> MassCreateInBatchesAsync(int workspaceArtifactId, int jobHistoryArtifactId,
            IList<CreateJobHistoryErrorDto> createJobHistoryErrorDtos)
        {
            List<int> result = new List<int>();

            const double numOfBatches = 2;
            int batchSize = (int)Math.Ceiling(createJobHistoryErrorDtos.Count() / numOfBatches);

            if (batchSize == createJobHistoryErrorDtos.Count())
            {
                throw new SyncException(
                    $"Mass creation of item level errors failed, because single item is still to large");
            }

            foreach (IList<CreateJobHistoryErrorDto> errorsBatchList in createJobHistoryErrorDtos.SplitList(batchSize))
            {
                IEnumerable<int> artifactIDs =
                    await MassCreateAsync(workspaceArtifactId, jobHistoryArtifactId, errorsBatchList)
                        .ConfigureAwait(false);
                result.AddRange(artifactIDs);
            }

            return result;
        }

        private ObjectTypeRef GetObjectTypeRef()
        {
            return new ObjectTypeRef { Guid = _rdoConfiguration.JobHistoryError.TypeGuid };
        }

        private RelativityObjectRef GetParentObject(int jobHistoryArtifactId)
        {
            return new RelativityObjectRef { ArtifactID = jobHistoryArtifactId };
        }

        private FieldRef[] GetFields()
        {
            return new[]
            {
                new FieldRef { Guid = _rdoConfiguration.JobHistoryError.ErrorMessagesGuid },
                new FieldRef { Guid = _rdoConfiguration.JobHistoryError.ErrorStatusGuid },
                new FieldRef { Guid = _rdoConfiguration.JobHistoryError.ErrorTypeGuid },
                new FieldRef { Guid = _rdoConfiguration.JobHistoryError.NameGuid },
                new FieldRef { Guid = _rdoConfiguration.JobHistoryError.SourceUniqueIdGuid },
                new FieldRef { Guid = _rdoConfiguration.JobHistoryError.StackTraceGuid },
                new FieldRef { Guid = _rdoConfiguration.JobHistoryError.TimeStampGuid }
            };
        }

        private ChoiceRef GetErrorStatusChoice(ErrorStatus errorStatus)
        {
            ChoiceRef errorStatusChoice = new ChoiceRef();
            switch (errorStatus)
            {
                case ErrorStatus.New:
                    errorStatusChoice.Guid = _rdoConfiguration.JobHistoryError.NewStatusGuid;
                    break;
                default:
                    throw new ArgumentException($"Invalid Error Status {errorStatus}");
            }

            return errorStatusChoice;
        }

        private ChoiceRef GetErrorTypeChoice(ErrorType errorType)
        {
            ChoiceRef errorTypeChoice = new ChoiceRef();
            switch (errorType)
            {
                case ErrorType.Job:
                    errorTypeChoice.Guid = _rdoConfiguration.JobHistoryError.JobLevelErrorGuid;
                    break;
                case ErrorType.Item:
                    errorTypeChoice.Guid = _rdoConfiguration.JobHistoryError.ItemLevelErrorGuid;
                    break;
                default:
                    throw new ArgumentException($"Invalid Error Type {errorType}");
            }

            return errorTypeChoice;
        }
    }
}
