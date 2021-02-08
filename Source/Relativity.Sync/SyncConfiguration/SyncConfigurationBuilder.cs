using System;
using System.Data;
using Relativity.Sync.SyncConfiguration.Options;
using Relativity.Sync.Utils;
using System.Linq.Expressions;

#pragma warning disable 1591

namespace Relativity.Sync.SyncConfiguration
{
    public class SyncConfigurationBuilder
    {
        private readonly ISyncContext _syncContext;
        private readonly ISyncServiceManager _servicesMgr;
        private readonly ISerializer _serializer;

        public SyncConfigurationBuilder(ISyncContext syncContext, ISyncServiceManager servicesMgr)
        {
            _syncContext = syncContext;
            _servicesMgr = servicesMgr;

            _serializer = new JSONSerializer();
        }

        public ISyncJobConfigurationBuilder ConfigureRdos(RdoOptions rdoOptions)
        {
            ValidateInput(rdoOptions);
            return new SyncJobJobConfigurationBuilder(_syncContext, _servicesMgr, rdoOptions, _serializer);
        }

        private void ValidateInput(RdoOptions rdoOptions)
        {
            ValidateJobHistoryGuids(rdoOptions.JobHistory);
            ValidateJobHistoryErrorGuids(rdoOptions.JobHistoryError);
            ValidateJobHistoryErrorStatusGuids(rdoOptions.JobHistoryErrorStatus);
        }

        private void ValidateJobHistoryErrorStatusGuids(JobHistoryErrorStatusOptions options)
        {
            ValidateProperty(options, x => x.NewGuid);
            ValidateProperty(options, x => x.ExpiredGuid);
            ValidateProperty(options, x => x.RetriedGuid);
            ValidateProperty(options, x => x.InProgressGuid);
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
            ValidateProperty(options, x => x.ItemLevelErrorChoiceGuid);
            ValidateProperty(options, x => x.JobLevelErrorChoiceGuid);
        }

        private void ValidateJobHistoryGuids(JobHistoryOptions options)
        {
            ValidateProperty(options, x => x.CompletedItemsCountGuid);
            ValidateProperty(options, x => x.TotalItemsCountGuid);
            ValidateProperty(options, x => x.FailedItemsCountGuid);
            ValidateProperty(options, x => x.DestinationWorkspaceInformationGuid);
            ValidateProperty(options, x => x.JobHistoryTypeGuid);
        }

        private void ValidateProperty<TRdo>(TRdo rdo, Expression<Func<TRdo, Guid>> expression)
        {
            Guid guid = expression.Compile().Invoke(rdo);
            if (!IsValidGuid(guid))
            {
                MemberExpression memberExpression = (expression.Body as MemberExpression) ??
                                                    throw new InvalidExpressionException(
                                                        "Expression must be a member expression");
                
                throw GetExceptionForInvalidInput(memberExpression.Member.Name,
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