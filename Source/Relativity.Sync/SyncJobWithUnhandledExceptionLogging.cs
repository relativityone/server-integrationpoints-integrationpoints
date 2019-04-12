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

		public async Task ExecuteAsync(CancellationToken token)
		{
			await ExecuteWithUnhandledExceptionLogging(_syncJob.ExecuteAsync, token).ConfigureAwait(false);
		}

		public async Task ExecuteAsync(IProgress<SyncJobState> progress, CancellationToken token)
		{
			await ExecuteWithUnhandledExceptionLogging(_syncJob.ExecuteAsync, progress, token).ConfigureAwait(false);
		}

		public async Task RetryAsync(CancellationToken token)
		{
			await ExecuteWithUnhandledExceptionLogging(_syncJob.RetryAsync, token).ConfigureAwait(false);
		}

		public async Task RetryAsync(IProgress<SyncJobState> progress, CancellationToken token)
		{
			await ExecuteWithUnhandledExceptionLogging(_syncJob.RetryAsync, progress, token).ConfigureAwait(false);
		}

		private async Task ExecuteWithUnhandledExceptionLogging(Func<CancellationToken, Task> action, CancellationToken token)
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

		private async Task ExecuteWithUnhandledExceptionLogging(Func<IProgress<SyncJobState>, CancellationToken, Task> action, IProgress<SyncJobState> progress, CancellationToken token)
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