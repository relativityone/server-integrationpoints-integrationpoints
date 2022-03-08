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
			InvokeWithJobInfo(_logger.LogVerbose, messageTemplate, propertyValues);
		}

		public void LogVerbose(Exception exception, string messageTemplate, params object[] propertyValues)
		{
			InvokeWithJobInfo(_logger.LogVerbose, exception, messageTemplate, propertyValues);
		}

		public void LogDebug(string messageTemplate, params object[] propertyValues)
		{
			InvokeWithJobInfo(_logger.LogDebug, messageTemplate, propertyValues);
		}

		public void LogDebug(Exception exception, string messageTemplate, params object[] propertyValues)
		{
			InvokeWithJobInfo(_logger.LogDebug, exception, messageTemplate, propertyValues);
		}

		public void LogInformation(string messageTemplate, params object[] propertyValues)
		{
			InvokeWithJobInfo(_logger.LogInformation, messageTemplate, propertyValues);
		}

		public void LogInformation(Exception exception, string messageTemplate, params object[] propertyValues)
		{
			InvokeWithJobInfo(_logger.LogInformation, exception, messageTemplate, propertyValues);
		}

		public void LogWarning(string messageTemplate, params object[] propertyValues)
		{
			InvokeWithJobInfo(_logger.LogWarning, messageTemplate, propertyValues);
		}

		public void LogWarning(Exception exception, string messageTemplate, params object[] propertyValues)
		{
			InvokeWithJobInfo(_logger.LogWarning, exception, messageTemplate, propertyValues);
		}

		public void LogError(string messageTemplate, params object[] propertyValues)
		{
			InvokeWithJobInfo(_logger.LogError, messageTemplate, propertyValues);
		}

		public void LogError(Exception exception, string messageTemplate, params object[] propertyValues)
		{
			InvokeWithJobInfo(_logger.LogError, exception, messageTemplate, propertyValues);
		}

		public void LogFatal(string messageTemplate, params object[] propertyValues)
		{
			InvokeWithJobInfo(_logger.LogFatal, messageTemplate, propertyValues);
		}

		public void LogFatal(Exception exception, string messageTemplate, params object[] propertyValues)
		{
			InvokeWithJobInfo(_logger.LogFatal, exception, messageTemplate, propertyValues);
		}

		private void InvokeWithJobInfo(Action<Exception, string, object[]> logAction, Exception exception, string messageTemplate, object[] propertyValues)
		{
			string messageTemplateWithJobInfo = ModifyTemplate(messageTemplate);
			object[] propertyValuesExtendedByJobParams = AddJobParamtersToPropertyValues(propertyValues);
			logAction(exception, messageTemplateWithJobInfo, propertyValuesExtendedByJobParams);
		}

		private void InvokeWithJobInfo(Action<string, object[]> logAction, string messageTemplate, object[] propertyValues)
		{
			string messageTemplateWithJobInfo = ModifyTemplate(messageTemplate);
			object[] propertyValuesExtendedByJobParams = AddJobParamtersToPropertyValues(propertyValues);
			logAction(messageTemplateWithJobInfo, propertyValuesExtendedByJobParams);
		}

		private object[] AddJobParamtersToPropertyValues(object[] propertyValues)
		{
			List<object> properties = new List<object>();
			properties.AddRange(propertyValues);
			properties.AddRange(new object[]
			{
				_jobParameters.WorkflowId,
				_jobParameters.SyncConfigurationArtifactId,
				_jobParameters.SyncBuildVersion
			});
			return properties.ToArray();
		}

		private static string ModifyTemplate(string messageTemplate)
		{
			return messageTemplate +
				" Sync job properties: " +
				$"{nameof(SyncJobParameters.WorkflowId)}: {{{nameof(SyncJobParameters.WorkflowId)}}} " +
				$"{nameof(SyncJobParameters.SyncConfigurationArtifactId)}: {{{nameof(SyncJobParameters.SyncConfigurationArtifactId)}}} " +
				$"{nameof(SyncJobParameters.SyncBuildVersion)}: {{{nameof(SyncJobParameters.SyncBuildVersion)}}} ";
		}
	}
}