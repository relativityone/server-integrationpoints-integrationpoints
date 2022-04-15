using System;
using Banzai.Logging;
using Relativity.API;

namespace Relativity.Sync.Logging
{
	internal sealed class SyncLogWriter : ILogWriter
	{
		private const string _UNABLE_TO_RESOLVE_MESSAGE = "Unable to resolve logging message due to framework limitation.";

		private readonly IAPILog _logger;

		public SyncLogWriter(IAPILog logger)
		{
			_logger = logger;
		}

		public void Fatal(string message, Exception exception = null)
		{
			_logger.LogFatal(exception, message);
		}

		public void Fatal(string format, params object[] formatArgs)
		{
			_logger.LogFatal(format, formatArgs);
		}

		public void Fatal(Func<string> deferredWrite, Exception exception = null)
		{
			// we cannot invoke deferredWrite here
			// .NET Framework 4.6.2 is not working well with Banzai (.NET Standard 2.0)
			_logger.LogFatal(exception, _UNABLE_TO_RESOLVE_MESSAGE);
		}

		public void Error(string message, Exception exception = null)
		{
			_logger.LogError(exception, message);
		}

		public void Error(string format, params object[] formatArgs)
		{
			_logger.LogError(format, formatArgs);
		}

		public void Error(Func<string> deferredWrite, Exception exception = null)
		{
			// we cannot invoke deferredWrite here
			// .NET Framework 4.6.2 is not working well with Banzai (.NET Standard 2.0)
			_logger.LogError(exception, _UNABLE_TO_RESOLVE_MESSAGE);
		}

		public void Warn(string message, Exception exception = null)
		{
			_logger.LogWarning(exception, message);
		}

		public void Warn(string format, params object[] formatArgs)
		{
			_logger.LogWarning(format, formatArgs);
		}

		public void Warn(Func<string> deferredWrite, Exception exception = null)
		{
			// we cannot invoke deferredWrite here
			// .NET Framework 4.6.2 is not working well with Banzai (.NET Standard 2.0)
			_logger.LogWarning(exception, _UNABLE_TO_RESOLVE_MESSAGE);
		}

		public void Info(string message, Exception exception = null)
		{
			_logger.LogInformation(exception, message);
		}

		public void Info(string format, params object[] formatArgs)
		{
			_logger.LogInformation(format, formatArgs);
		}

		public void Info(Func<string> deferredWrite, Exception exception = null)
		{
			// we cannot invoke deferredWrite here
			// .NET Framework 4.6.2 is not working well with Banzai (.NET Standard 2.0)
			_logger.LogInformation(exception, _UNABLE_TO_RESOLVE_MESSAGE);
		}

		public void Debug(string message, Exception exception = null)
		{
			_logger.LogDebug(exception, message);
		}

		public void Debug(string format, params object[] formatArgs)
		{
			_logger.LogDebug(format, formatArgs);
		}

		public void Debug(Func<string> deferredWrite, Exception exception = null)
		{
			// we cannot invoke deferredWrite here
			// .NET Framework 4.6.2 is not working well with Banzai (.NET Standard 2.0)
			_logger.LogDebug(exception, _UNABLE_TO_RESOLVE_MESSAGE);
		}
	}
}
