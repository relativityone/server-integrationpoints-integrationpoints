using System;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Logging;

namespace kCura.IntegrationPoints.Core.Services.JobHistory
{
    /// <summary>
    ///     Multiple agents may be trying to access Job History data at the same time. Since agents can run across
    ///     machines our database is used to act as a distributed lock coordinator. The mutex is managed by
    ///     sp_getapplock and sp_releaseapplock.
    ///     This prevents read -> read -> update -> update
    ///     when we require read -> update -> read -> update.
    ///     For more information see:
    ///     sp_getapplock - https://msdn.microsoft.com/en-us/library/ms189823.aspx
    ///     sp_releaseapplock - https://msdn.microsoft.com/en-us/library/ms178602.aspx
    /// </summary>
    internal class JobHistoryMutex : IDisposable
    {
        private bool _disposed = false;
        private readonly IWorkspaceDBContext _context;
        private readonly IDiagnosticLog _diagnosticLog;

        public JobHistoryMutex(IWorkspaceDBContext context, Guid lockIdentifier, IDiagnosticLog diagnosticLog)
        {
            _context = context;
            _diagnosticLog = diagnosticLog;
            _context.BeginTransaction();
            EnableMutex(lockIdentifier);
        }

        /// <summary>
        ///     This method must be called within a transaction
        /// </summary>
        private void EnableMutex(Guid identifier)
        {
            _diagnosticLog.LogDiagnostic("EnableMutex for Identifier {identifier}", identifier);

            string enableJobHistoryMutex = $@"
                DECLARE @res INT
                EXEC @res = sp_getapplock
                                @Resource = '{identifier}',
                                @LockMode = 'Exclusive',
                                @LockOwner = 'Transaction',
                                @LockTimeout = 5000,
                                @DbPrincipal = 'public'

                IF @res NOT IN (0, 1)
                        BEGIN
                            RAISERROR ( 'Unable to acquire mutex', 16, 1 )
                        END";

            _context.ExecuteNonQuerySQLStatement(enableJobHistoryMutex);
        }

        public void Dispose()
        {
            _diagnosticLog.LogDiagnostic("Dispose JobHistoryMutex");

            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // The mutex will be removed when the transaction is finalized
                _context.CommitTransaction();
            }

            _disposed = true;
        }
    }
}