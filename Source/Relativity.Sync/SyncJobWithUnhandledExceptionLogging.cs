using System;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Sync.Utils;

namespace Relativity.Sync
{
    internal sealed class SyncJobWithUnhandledExceptionLogging : ISyncJob
    {
        private readonly ISyncJob _syncJob;
        private readonly IAppDomain _appDomain;
        private readonly IAPILog _logger;

        public SyncJobWithUnhandledExceptionLogging(ISyncJob syncJob, IAppDomain appDomain, IAPILog logger)
        {
            _syncJob = syncJob;
            _appDomain = appDomain;
            _logger = logger;
        }

        public Task ExecuteAsync(CompositeCancellationToken token)
        {
            return ExecuteWithUnhandledExceptionLoggingAsync(_syncJob.ExecuteAsync, token);
        }

        public Task ExecuteAsync(IProgress<SyncJobState> progress, CompositeCancellationToken token)
        {
            return ExecuteWithUnhandledExceptionLoggingAsync(_syncJob.ExecuteAsync, progress, token);
        }
        
        private async Task ExecuteWithUnhandledExceptionLoggingAsync(Func<CompositeCancellationToken, Task> action, CompositeCancellationToken token)
        {
            _appDomain.UnhandledException += OnUnhandledException;

            try
            {
                await action(token).ConfigureAwait(false);
            }
            finally
            {
                _appDomain.UnhandledException -= OnUnhandledException;
            }
        }

        private async Task ExecuteWithUnhandledExceptionLoggingAsync(Func<IProgress<SyncJobState>, CompositeCancellationToken, Task> action, IProgress<SyncJobState> progress, CompositeCancellationToken token)
        {
            _appDomain.UnhandledException += OnUnhandledException;

            try
            {
                await action(progress, token).ConfigureAwait(false);
            }
            finally
            {
                _appDomain.UnhandledException -= OnUnhandledException;
            }
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _logger.LogFatal(e.ExceptionObject as Exception, "Unhandled exception in RelativitySync occurred!");
        }
    }
}
