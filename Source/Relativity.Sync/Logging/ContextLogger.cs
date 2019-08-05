using System;
using System.Collections.Generic;

namespace Relativity.Sync.Logging
{
	internal sealed class ContextLogger : ISyncLog
	{
		private readonly SyncJobParameters _jobParameters;
		private readonly ISyncLog _logger;

		public ContextLogger(SyncJobParameters jobParameters, ISyncLog logger)
		{
			_jobParameters = jobParameters;
			_logger = logger;
		}

		public void LogVerbose(string messageTemplate, params object[] propertyValues)
		{
			_logger.LogVerbose(messageTemplate, ModifyPropertyValues(propertyValues));
		}

		public void LogVerbose(Exception exception, string messageTemplate, params object[] propertyValues)
		{
			_logger.LogVerbose(exception, messageTemplate, ModifyPropertyValues(propertyValues));
		}

		public void LogDebug(string messageTemplate, params object[] propertyValues)
		{
			_logger.LogDebug(messageTemplate, ModifyPropertyValues(propertyValues));
		}

		public void LogDebug(Exception exception, string messageTemplate, params object[] propertyValues)
		{
			_logger.LogDebug(exception, messageTemplate, ModifyPropertyValues(propertyValues));
		}

		public void LogInformation(string messageTemplate, params object[] propertyValues)
		{
			_logger.LogInformation(messageTemplate, ModifyPropertyValues(propertyValues));
		}

		public void LogInformation(Exception exception, string messageTemplate, params object[] propertyValues)
		{
			_logger.LogInformation(exception, messageTemplate, ModifyPropertyValues(propertyValues));
		}

		public void LogWarning(string messageTemplate, params object[] propertyValues)
		{
			_logger.LogWarning(messageTemplate, ModifyPropertyValues(propertyValues));
		}

		public void LogWarning(Exception exception, string messageTemplate, params object[] propertyValues)
		{
			_logger.LogWarning(exception, messageTemplate, ModifyPropertyValues(propertyValues));
		}

		public void LogError(string messageTemplate, params object[] propertyValues)
		{
			_logger.LogError(messageTemplate, ModifyPropertyValues(propertyValues));
		}

		public void LogError(Exception exception, string messageTemplate, params object[] propertyValues)
		{
			_logger.LogError(exception, messageTemplate, ModifyPropertyValues(propertyValues));
		}

		public void LogFatal(string messageTemplate, params object[] propertyValues)
		{
			_logger.LogFatal(messageTemplate, ModifyPropertyValues(propertyValues));
		}

		public void LogFatal(Exception exception, string messageTemplate, params object[] propertyValues)
		{
			_logger.LogFatal(exception, messageTemplate, ModifyPropertyValues(propertyValues));
		}

		private object[] ModifyPropertyValues(object[] propertyValues)
		{
			var result = new List<object>(propertyValues) {_jobParameters};
			return result.ToArray();
		}
	}
}