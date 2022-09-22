using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Contracts.BatchReporter;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Extensions;
using kCura.IntegrationPoints.Domain.Managers;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;
using FieldRef = Relativity.Services.Objects.DataContracts.FieldRef;

namespace kCura.IntegrationPoints.Core.Services
{
    public class JobHistoryErrorService : IJobHistoryErrorService
    {
        public const int ERROR_BATCH_SIZE = 5000;

        private readonly Guid _jobHistoryErrorObject = new Guid("17E7912D-4F57-4890-9A37-ABC2B8A37BDB");
        private readonly Guid _errorMessageField = new Guid("4112B894-35B0-4E53-AB99-C9036D08269D");
        private readonly Guid _errorStatusField = new Guid("DE1A46D2-D615-427A-B9F2-C10769BC2678");
        private readonly Guid _errorTypeField = new Guid("EEFFA5D3-82E3-46F8-9762-B4053D73F973");
        private readonly Guid _nameField = new Guid("84E757CC-9DA2-435D-B288-0C21EC589E66");
        private readonly Guid _sourceUniqueIdField = new Guid("5519435E-EE82-4820-9546-F1AF46121901");
        private readonly Guid _stackTraceField = new Guid("0353DBDE-9E00-4227-8A8F-4380A8891CFF");
        private readonly Guid _timestampUtcField = new Guid("B9CBA772-E7C9-493E-B7F8-8D605A6BFE1F");

        private readonly IIntegrationPointRepository _integrationPointRepository;
        private readonly IRelativityObjectManager _relativityObjectManager;
        private readonly IAPILog _logger;

        private ConcurrentQueue<JobHistoryError> _jobHistoryErrorQueue;
        private bool _errorOccurredDuringJob;

        public JobHistoryErrorService(IRelativityObjectManager relativityObjectManager, IHelper helper, IIntegrationPointRepository integrationPointRepository)
        {
            _integrationPointRepository = integrationPointRepository;
            _relativityObjectManager = relativityObjectManager;
            _logger = helper.GetLoggerFactory().GetLogger().ForContext<IJobHistoryErrorService>();
            _jobHistoryErrorQueue = new ConcurrentQueue<JobHistoryError>();
            _errorOccurredDuringJob = false;
        }

        public Data.JobHistory JobHistory { get; set; }

        public Data.IntegrationPoint IntegrationPoint { get; set; }

        public IJobStopManager JobStopManager { get; set; }

        internal int PendingErrorCount => _jobHistoryErrorQueue.Count;

        public void SubscribeToBatchReporterEvents(object batchReporter)
        {
            if (batchReporter is IBatchReporter)
            {
                ((IBatchReporter)batchReporter).OnJobError += OnJobError;
                ((IBatchReporter)batchReporter).OnDocumentError += OnRowError;
            }
        }

        public void CommitErrors()
        {
            try
            {
                _logger.LogInformation("Mass-creating item level errors count: {count}", _jobHistoryErrorQueue.Count);

                IReadOnlyList<IReadOnlyList<object>> values = _jobHistoryErrorQueue.Select(x => new List<object>()
                {
                    x.Error,
                    GetErrorStatusChoice(),
                    GetErrorTypeChoice(x.ErrorType),
                    Guid.NewGuid().ToString(),
                    x.SourceUniqueID,
                    x.StackTrace,
                    DateTime.UtcNow
                }).ToList();

                if (_jobHistoryErrorQueue.Any())
                {
                    _errorOccurredDuringJob = true;

                    if (IntegrationPoint != null)
                    {
                        IntegrationPoint.HasErrors = true;
                    }

                    MassCreateRequest request = new MassCreateRequest
                    {
                        ObjectType = GetObjectTypeRef(),
                        ParentObject = GetParentObject(_jobHistoryErrorQueue.FirstOrDefault()?.JobHistory ?? 0),
                        Fields = GetFields(),
                        ValueLists = values
                    };

                    MassCreateResult result = _relativityObjectManager.MassCreateAsync(request).GetAwaiter().GetResult();
                    if (!result.Success)
                    {
                        throw new IntegrationPointsException($"Mass creation of item level errors was not successful. Message: {result.Message}");
                    }

                    _logger.LogInformation("Successfully mass-created item level errors count: {count}", _jobHistoryErrorQueue.Count);
                }

                if ((IntegrationPoint != null && !_errorOccurredDuringJob) || (JobStopManager?.IsStopRequested() == true))
                {
                    IntegrationPoint.HasErrors = false;
                }

                _logger.LogInformation("Successfully mass-created item level errors count: {count}", _jobHistoryErrorQueue.Count);
            }
            catch (Exception ex)
            {
                // if failed to commit, throw all buffered errors as part of an exception
                List<string> errorList = _jobHistoryErrorQueue.Select(x =>
                    x.ErrorType.Name.Equals(ErrorTypeChoices.JobHistoryErrorJob.Name)
                        ? $"{x.TimestampUTC} Type: {x.ErrorType.Name}    Error: {x.Error}"
                        : $"{x.TimestampUTC} Type: {x.ErrorType.Name}    Identifier: {x.SourceUniqueID}    Error: {x.Error}").ToList();

                string allErrors = string.Join(Environment.NewLine, errorList.ToArray());
                allErrors += string.Format("{0}{0}Reason for exception: {1}", Environment.NewLine, ex.FlattenErrorMessagesWithStackTrace());

                LogCommittingErrorsFailed(ex, allErrors);

                _logger.LogError("Could not commit Job History Errors. These are uncommitted errors: {allErrors}", allErrors);
                throw new Exception("Could not commit Job History Errors. These are uncommitted errors:" + Environment.NewLine + allErrors);
            }
            finally
            {
                _jobHistoryErrorQueue = new ConcurrentQueue<JobHistoryError>();
                UpdateIntegrationPoint();
            }
        }

        public void AddError(global::Relativity.Services.Choice.ChoiceRef errorType, Exception ex)
        {
            string message = ex.FlattenErrorMessagesWithStackTrace();

            if (ex is IntegrationPointValidationException)
            {
                var ipException = ex as IntegrationPointValidationException;
                message = string.Join(Environment.NewLine, ipException.ValidationResult.MessageTexts);
            }

            AddError(errorType, string.Empty, ex.Message, message);
        }

        public void AddError(global::Relativity.Services.Choice.ChoiceRef errorType, string documentIdentifier, string errorMessage, string stackTrace)
        {
            if (JobHistory != null && JobHistory.ArtifactId > 0)
            {
                DateTime now = DateTime.UtcNow;

                JobHistoryError jobHistoryError = new JobHistoryError
                {
                    ParentArtifactId = JobHistory.ArtifactId,
                    JobHistory = JobHistory.ArtifactId,
                    Name = Guid.NewGuid().ToString(),
                    ErrorType = errorType,
                    ErrorStatus = ErrorStatusChoices.JobHistoryErrorNew,
                    SourceUniqueID = documentIdentifier,
                    Error = errorMessage,
                    StackTrace = stackTrace,
                    TimestampUTC = now
                };

                _jobHistoryErrorQueue.Enqueue(jobHistoryError);

                if (errorType == ErrorTypeChoices.JobHistoryErrorJob)
                {
                    CommitErrors();
                }
                else if (_jobHistoryErrorQueue.Count == ERROR_BATCH_SIZE)
                {
                    CommitErrors();
                }
            }
            else
            {
                LogMissingJobHistoryError();

                // we can't create JobHistoryError without JobHistory,
                // in such case log error into Error Tab by throwing Exception.
                throw new Exception($"Type:{errorType.Name} Id:{documentIdentifier}  Error:{errorMessage}");
            }
        }

        private ObjectTypeRef GetObjectTypeRef()
        {
            return new ObjectTypeRef { Guid = _jobHistoryErrorObject };
        }

        private RelativityObjectRef GetParentObject(int jobHistoryArtifactId)
        {
            return new RelativityObjectRef { ArtifactID = jobHistoryArtifactId };
        }

        private FieldRef[] GetFields()
        {
            return new[]
            {
                new FieldRef { Guid = _errorMessageField },
                new FieldRef { Guid = _errorStatusField },
                new FieldRef { Guid = _errorTypeField },
                new FieldRef { Guid = _nameField },
                new FieldRef { Guid = _sourceUniqueIdField },
                new FieldRef { Guid = _stackTraceField },
                new FieldRef { Guid = _timestampUtcField }
            };
        }

        private ChoiceRef GetErrorStatusChoice()
        {
            var errorStatusChoice = new ChoiceRef
            {
                Guid = ErrorStatusChoices.JobHistoryErrorNewGuid
            };
            return errorStatusChoice;
        }

        private ChoiceRef GetErrorTypeChoice(global::Relativity.Services.Choice.ChoiceRef errorType)
        {
            var errorTypeChoice = new ChoiceRef
            {
                Guid = errorType.Guids.Single()
            };
            return errorTypeChoice;
        }

        private void OnRowError(string documentIdentifier, string errorMessage)
        {
            if (IntegrationPoint.LogErrors.GetValueOrDefault(false))
            {
                if (JobStopManager?.IsStopRequested() == true)
                {
                    return;
                }

                AddError(ErrorTypeChoices.JobHistoryErrorItem, documentIdentifier, errorMessage, errorMessage);
            }
        }

        private void OnJobError(Exception ex)
        {
            AddError(ErrorTypeChoices.JobHistoryErrorJob, ex);
        }

        private void UpdateIntegrationPoint()
        {
            try
            {
                if (IntegrationPoint != null)
                {
                    _integrationPointRepository.Update(IntegrationPoint);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to update Integration Point's Has Error field.");

                // Ignore error, if we can't update the Integration Point's Has Errors Field, just continue on.
                // The field may be out of state with the true job status, or subsequent Update calls may succeed.
            }
        }

        #region Logging

        private void LogCommittingErrorsFailed(Exception ex, string allErrors)
        {
            _logger.LogError(ex, "Could not commit Job History Errors. These are uncommitted errors: {Errors}.", allErrors);
        }

        private void LogMissingJobHistoryError()
        {
            _logger.LogError("Failed to create Job History Error: Job History doesn't exists.");
        }

        #endregion
    }
}
