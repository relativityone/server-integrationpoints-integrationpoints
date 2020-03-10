using System;
using System.Threading;
using System.Threading.Tasks;

namespace Relativity.Sync
{
	internal sealed class SyncJobWithUnhandledExceptionLogging : ISyncJob
	{
		private readonly ISyncJob _syncJob;
		private readonly IAppDomain _appDomain;
		private readonly ISyncLog _logger;

		public SyncJobWithUnhandledExceptionLogging(ISyncJob syncJob, IAppDomain appDomain, ISyncLog logger)
		{
			_syncJob = syncJob;
			_appDomain = appDomain;
			_logger = logger;
		}

		public Task ExecuteAsync(CancellationToken token)
		{
			return ExecuteWithUnhandledExceptionLoggingAsync(_syncJob.ExecuteAsync, token);
		}

		public Task ExecuteAsync(IProgress<SyncJobState> progress, CancellationToken token)
		{
			return ExecuteWithUnhandledExceptionLoggingAsync(_syncJob.ExecuteAsync, progress, token);
		}

		public Task RetryAsync(CancellationToken token)
		{
			return ExecuteWithUnhandledExceptionLoggingAsync(_syncJob.RetryAsync, token);
		}

		public Task RetryAsync(IProgress<SyncJobState> progress, CancellationToken token)
		{
			return ExecuteWithUnhandledExceptionLoggingAsync(_syncJob.RetryAsync, progress, token);
		}

		private async Task ExecuteWithUnhandledExceptionLoggingAsync(Func<CancellationToken, Task> action, CancellationToken token)
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

		private async Task ExecuteWithUnhandledExceptionLoggingAsync(Func<IProgress<SyncJobState>, CancellationToken, Task> action, IProgress<SyncJobState> progress, CancellationToken token)
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