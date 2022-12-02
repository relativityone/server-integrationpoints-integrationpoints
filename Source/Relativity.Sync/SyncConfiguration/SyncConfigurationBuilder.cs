using System;
using System.Data;
using System.Linq.Expressions;
using Relativity.API;
using Relativity.Sync.SyncConfiguration.Options;
using Relativity.Sync.Utils;

namespace Relativity.Sync.SyncConfiguration
{
    /// <inheritdoc />
    public class SyncConfigurationBuilder : ISyncConfigurationBuilder
    {
        private readonly ISyncContext _syncContext;
        private readonly IServicesMgr _servicesMgr;
        private readonly IAPILog _logger;
        private readonly ISerializer _serializer;

        /// <summary>
        /// Creates new instance of <see cref="SyncConfigurationBuilder"/> class.
        /// </summary>
        /// <param name="syncContext">Sync configuration context.</param>
        /// <param name="servicesMgr">Sync Service Manager</param>
        /// <param name="logger">logs data for debug purposes</param>
        public SyncConfigurationBuilder(ISyncContext syncContext, IServicesMgr servicesMgr, IAPILog logger)
        {
            _syncContext = syncContext;
            _servicesMgr = servicesMgr;
            _logger = logger;
            _serializer = new JSONSerializer();
        }

        /// <inheritdoc />
        public ISyncJobConfigurationBuilder ConfigureRdos(RdoOptions rdoOptions)
        {
            ValidateInput(rdoOptions);
            return new SyncJobConfigurationBuilder(_syncContext, _servicesMgr, rdoOptions, _serializer, _logger);
        }

        private void ValidateInput(RdoOptions rdoOptions)
        {
            ValidateJobHistoryGuids(rdoOptions.JobHistory);
            ValidateJobHistoryStatusGuids(rdoOptions.JobHistoryStatus);
            ValidateJobHistoryErrorGuids(rdoOptions.JobHistoryError);
            ValidateDestinationWorkspaceGuids(rdoOptions.DestinationWorkspace);
        }

        private void ValidateDestinationWorkspaceGuids(DestinationWorkspaceOptions options)
        {
            ValidateProperty(options, x => x.TypeGuid);
            ValidateProperty(options, x => x.NameGuid);
            ValidateProperty(options, x => x.DestinationWorkspaceNameGuid);
            ValidateProperty(options, x => x.DestinationWorkspaceArtifactIdGuid);
            ValidateProperty(options, x => x.DestinationInstanceNameGuid);
            ValidateProperty(options, x => x.DestinationInstanceArtifactIdGuid);
            ValidateProperty(options, x => x.JobHistoryOnDocumentGuid);
            ValidateProperty(options, x => x.DestinationWorkspaceOnDocument);
        }

        private void ValidateJobHistoryErrorGuids(JobHistoryErrorOptions options)
        {
            ValidateProperty(options, x => x.TypeGuid);
            ValidateProperty(options, x => x.NameGuid);
            ValidateProperty(options, x => x.SourceUniqueIdGuid);
            ValidateProperty(options, x => x.ErrorMessageGuid);
            ValidateProperty(options, x => x.TimeStampGuid);
            ValidateProperty(options, x => x.ErrorTypeGuid);
            ValidateProperty(options, x => x.StackTraceGuid);
            ValidateProperty(options, x => x.ErrorStatusGuid);
            ValidateProperty(options, x => x.JobHistoryRelationGuid);
            ValidateProperty(options, x => x.ItemLevelErrorChoiceGuid);
            ValidateProperty(options, x => x.JobLevelErrorChoiceGuid);
            ValidateProperty(options, x => x.NewStatusGuid);
        }

        private void ValidateJobHistoryGuids(JobHistoryOptions options)
        {
            ValidateProperty(options, x => x.JobHistoryTypeGuid);
            ValidateProperty(options, x => x.JobIdGuid);
            ValidateProperty(options, x => x.StatusGuid);
            ValidateProperty(options, x => x.CompletedItemsCountGuid);
            ValidateProperty(options, x => x.TotalItemsCountGuid);
            ValidateProperty(options, x => x.FailedItemsCountGuid);
            ValidateProperty(options, x => x.DestinationWorkspaceInformationGuid);
            ValidateProperty(options, x => x.StartTimeGuid);
            ValidateProperty(options, x => x.EndTimeGuid);
        }

        private void ValidateJobHistoryStatusGuids(JobHistoryStatusOptions options)
        {
            ValidateProperty(options, x => x.ValidatingGuid);
            ValidateProperty(options, x => x.ValidationFailedGuid);
            ValidateProperty(options, x => x.ProcessingGuid);
            ValidateProperty(options, x => x.CompletedGuid);
            ValidateProperty(options, x => x.CompletedWithErrorsGuid);
            ValidateProperty(options, x => x.JobFailedGuid);
            ValidateProperty(options, x => x.StoppingGuid);
            ValidateProperty(options, x => x.StoppedGuid);
            ValidateProperty(options, x => x.SuspendingGuid);
            ValidateProperty(options, x => x.SuspendedGuid);
        }

        private void ValidateProperty<TRdo>(TRdo rdo, Expression<Func<TRdo, Guid>> expression)
        {
            Guid guid = expression.Compile().Invoke(rdo);
            if (!IsValidGuid(guid))
            {
                MemberExpression memberExpression = (expression.Body as MemberExpression) ??
                                                    throw new InvalidExpressionException(
                                                        "Expression must be a member expression");

                throw GetExceptionForInvalidInput(
                    memberExpression.Member.Name,
                    typeof(TRdo).Name, guid.ToString());
            }
        }

        private bool IsValidGuid(Guid guid)
        {
            return guid != Guid.Empty;
        }

        private InvalidSyncConfigurationException GetExceptionForInvalidInput(string propertyName, string rdoName,
            string invalidGuid)
        {
            return new InvalidSyncConfigurationException(
                $"GUID value for {rdoName}.{propertyName} is invalid: {invalidGuid}");
        }
    }
}
