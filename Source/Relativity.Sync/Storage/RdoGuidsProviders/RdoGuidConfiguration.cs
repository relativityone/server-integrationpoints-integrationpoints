using System;
using System.Linq.Expressions;
using System.Windows.Forms;
using Relativity.Sync.Configuration;
using Relativity.Sync.RDOs;

namespace Relativity.Sync.Storage.RdoGuidsProviders
{
    internal class RdoGuidConfiguration : IRdoGuidConfiguration
    {
        private readonly IConfiguration _cache;
        private readonly ISyncLog _syncLog;

        public RdoGuidConfiguration(IConfiguration cache, ISyncLog syncLog)
        {
            _cache = cache;
            _syncLog = syncLog;

            JobHistory = new JobHistoryRdoGuidsProvider(ParseGuid);
            JobHistoryError = new JobHistoryErrorGuidsProvider(ParseGuid);
        }
        
        public IJobHistoryRdoGuidsProvider JobHistory { get; }
        public IJobHistoryErrorGuidsProvider JobHistoryError { get; }


        private Guid ParseGuid(Expression<Func<SyncConfigurationRdo, Guid>> fieldExpression)
        {
            Guid guid;
            string guidString = _cache.GetFieldValue<string>(fieldExpression);
            
            if (Guid.TryParse(guidString, out guid))
            {
                return guid;
            }

            _syncLog.LogError("Unable to parse GUID: {guidString}.", guidString);
            throw new ArgumentException($"Argument needs to be valid GUID, but {guidString} found.");
        }
    }
}