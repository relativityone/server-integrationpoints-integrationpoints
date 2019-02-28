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
			return ExecuteWithUnhandledExceptionLogging(_syncJob.ExecuteAsync, token);
		}

		public Task ExecuteAsync(IProgress<SyncProgress> progress, CancellationToken token)
		{
			return ExecuteWithUnhandledExceptionLogging(_syncJob.ExecuteAsync, progress, token);
		}

		public Task RetryAsync(CancellationToken token)
		{
			return ExecuteWithUnhandledExceptionLogging(_syncJob.RetryAsync, token);
		}

		public Task RetryAsync(IProgress<SyncProgress> progress, CancellationToken token)
		{
			return ExecuteWithUnhandledExceptionLogging(_syncJob.RetryAsync, progress, token);
		}

		private Task ExecuteWithUnhandledExceptionLogging(Func<CancellationToken, Task> action, CancellationToken token)
		{
			_appDomain.UnhandledException += OnUnhandledException;

			try
			{
				return action(token);
			}
			finally
			{
				_appDomain.UnhandledException -= OnUnhandledException;
			}
		}

		private Task ExecuteWithUnhandledExceptionLogging(Func<IProgress<SyncProgress>, CancellationToken, Task> action, IProgress<SyncProgress> progress, CancellationToken token)
		{
			_appDomain.UnhandledException += OnUnhandledException;

			try
			{
				return action(progress, token);
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