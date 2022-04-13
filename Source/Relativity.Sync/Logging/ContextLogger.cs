using System;
using Relativity.API;

namespace Relativity.Sync.Logging
{
	internal sealed class ContextLogger : ISyncLog
	{
		private readonly IAPILog _logger;

		public ContextLogger(IAPILog logger)
		{
			_logger = logger;
		}

		public void LogVerbose(string messageTemplate, params object[] propertyValues)
		{
			_logger.LogVerbose(messageTemplate, propertyValues);
		}

		public void LogVerbose(Exception exception, string messageTemplate, params object[] propertyValues)
		{
			_logger.LogVerbose(exception, messageTemplate, propertyValues);
		}

		public void LogDebug(string messageTemplate, params object[] propertyValues)
		{
			_logger.LogDebug(messageTemplate, propertyValues);
		}

		public void LogDebug(Exception exception, string messageTemplate, params object[] propertyValues)
		{
			_logger.LogDebug(exception, messageTemplate, propertyValues);
		}

		public void LogInformation(string messageTemplate, params object[] propertyValues)
		{
			_logger.LogInformation(messageTemplate, propertyValues);
		}

		public void LogInformation(Exception exception, string messageTemplate, params object[] propertyValues)
		{
			_logger.LogInformation(exception, messageTemplate, propertyValues);
		}

		public void LogWarning(string messageTemplate, params object[] propertyValues)
		{
			_logger.LogWarning(messageTemplate, propertyValues);
		}

		public void LogWarning(Exception exception, string messageTemplate, params object[] propertyValues)
		{
			_logger.LogWarning(exception, messageTemplate, propertyValues);
		}

		public void LogError(string messageTemplate, params object[] propertyValues)
		{
			_logger.LogError(messageTemplate, propertyValues);
		}

		public void LogError(Exception exception, string messageTemplate, params object[] propertyValues)
		{
			_logger.LogError(exception, messageTemplate, propertyValues);
		}

		public void LogFatal(string messageTemplate, params object[] propertyValues)
		{
			_logger.LogFatal(messageTemplate, propertyValues);
		}

		public void LogFatal(Exception exception, string messageTemplate, params object[] propertyValues)
		{
			_logger.LogFatal(exception, messageTemplate, propertyValues);
		}
	}
}