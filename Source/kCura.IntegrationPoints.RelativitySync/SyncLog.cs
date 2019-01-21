﻿using System;
using Relativity.API;
using Relativity.Sync;

namespace kCura.IntegrationPoints.RelativitySync
{
	internal sealed class SyncLog : ISyncLog
	{
		private readonly IAPILog _logger;

		public SyncLog(IAPILog logger)
		{
			_logger = logger;
		}

		public bool IsEnabled { get; set; } = true;

		public void LogVerbose(string messageTemplate, params object[] propertyValues)
		{
			if (IsEnabled)
			{
				_logger.LogVerbose(messageTemplate, propertyValues);
			}
		}

		public void LogVerbose(Exception exception, string messageTemplate, params object[] propertyValues)
		{
			if (IsEnabled)
			{
				_logger.LogVerbose(exception, messageTemplate, propertyValues);
			}
		}

		public void LogDebug(string messageTemplate, params object[] propertyValues)
		{
			if (IsEnabled)
			{
				_logger.LogDebug(messageTemplate, propertyValues);
			}
		}

		public void LogDebug(Exception exception, string messageTemplate, params object[] propertyValues)
		{
			if (IsEnabled)
			{
				_logger.LogDebug(exception, messageTemplate, propertyValues);
			}
		}

		public void LogInformation(string messageTemplate, params object[] propertyValues)
		{
			if (IsEnabled)
			{
				_logger.LogInformation(messageTemplate, propertyValues);
			}
		}

		public void LogInformation(Exception exception, string messageTemplate, params object[] propertyValues)
		{
			if (IsEnabled)
			{
				_logger.LogInformation(exception, messageTemplate, propertyValues);
			}
		}

		public void LogWarning(string messageTemplate, params object[] propertyValues)
		{
			if (IsEnabled)
			{
				_logger.LogWarning(messageTemplate, propertyValues);
			}
		}

		public void LogWarning(Exception exception, string messageTemplate, params object[] propertyValues)
		{
			if (IsEnabled)
			{
				_logger.LogWarning(exception, messageTemplate, propertyValues);
			}
		}

		public void LogError(string messageTemplate, params object[] propertyValues)
		{
			if (IsEnabled)
			{
				_logger.LogError(messageTemplate, propertyValues);
			}
		}

		public void LogError(Exception exception, string messageTemplate, params object[] propertyValues)
		{
			if (IsEnabled)
			{
				_logger.LogError(exception, messageTemplate, propertyValues);
			}
		}

		public void LogFatal(string messageTemplate, params object[] propertyValues)
		{
			if (IsEnabled)
			{
				_logger.LogFatal(messageTemplate, propertyValues);
			}
		}

		public void LogFatal(Exception exception, string messageTemplate, params object[] propertyValues)
		{
			if (IsEnabled)
			{
				_logger.LogFatal(exception, messageTemplate, propertyValues);
			}
		}
	}
}