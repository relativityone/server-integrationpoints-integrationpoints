using System;

namespace Relativity.Sync
{
	internal sealed class ContextLogger : ISyncLog
	{
		private readonly CorrelationId _id;
		private readonly ISyncLog _logger;

		public ContextLogger(CorrelationId id, ISyncLog logger)
		{
			_id = id;
			_logger = logger;
		}

		public void LogVerbose(string messageTemplate, params object[] propertyValues)
		{
			_logger.LogVerbose(FormatMessage(messageTemplate), ModifyPropertyValues(propertyValues));
		}

		public void LogVerbose(Exception exception, string messageTemplate, params object[] propertyValues)
		{
			_logger.LogVerbose(exception, FormatMessage(messageTemplate), ModifyPropertyValues(propertyValues));
		}

		public void LogDebug(string messageTemplate, params object[] propertyValues)
		{
			_logger.LogDebug(FormatMessage(messageTemplate), ModifyPropertyValues(propertyValues));
		}

		public void LogDebug(Exception exception, string messageTemplate, params object[] propertyValues)
		{
			_logger.LogDebug(exception, FormatMessage(messageTemplate), ModifyPropertyValues(propertyValues));
		}

		public void LogInformation(string messageTemplate, params object[] propertyValues)
		{
			_logger.LogInformation(FormatMessage(messageTemplate), ModifyPropertyValues(propertyValues));
		}

		public void LogInformation(Exception exception, string messageTemplate, params object[] propertyValues)
		{
			_logger.LogInformation(exception, FormatMessage(messageTemplate), ModifyPropertyValues(propertyValues));
		}

		public void LogWarning(string messageTemplate, params object[] propertyValues)
		{
			_logger.LogWarning(FormatMessage(messageTemplate), ModifyPropertyValues(propertyValues));
		}

		public void LogWarning(Exception exception, string messageTemplate, params object[] propertyValues)
		{
			_logger.LogWarning(exception, FormatMessage(messageTemplate), ModifyPropertyValues(propertyValues));
		}

		public void LogError(string messageTemplate, params object[] propertyValues)
		{
			_logger.LogError(FormatMessage(messageTemplate), ModifyPropertyValues(propertyValues));
		}

		public void LogError(Exception exception, string messageTemplate, params object[] propertyValues)
		{
			_logger.LogError(exception, FormatMessage(messageTemplate), ModifyPropertyValues(propertyValues));
		}

		public void LogFatal(string messageTemplate, params object[] propertyValues)
		{
			_logger.LogFatal(FormatMessage(messageTemplate), ModifyPropertyValues(propertyValues));
		}

		public void LogFatal(Exception exception, string messageTemplate, params object[] propertyValues)
		{
			_logger.LogFatal(exception, FormatMessage(messageTemplate), ModifyPropertyValues(propertyValues));
		}

		private static string FormatMessage(string messageTemplate)
		{
			return $"{{key}} {messageTemplate}";
		}

		private object[] ModifyPropertyValues(params object[] propertyValues)
		{
			object[] newValues = new object[propertyValues.Length + 1];
			newValues[0] = _id.Value;
			propertyValues.CopyTo(newValues, 1);
			return newValues;
		}
	}
}