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
			_logger.LogVerbose(ModifyTemplate(messageTemplate), ExtendPropertyValues(propertyValues));
		}

		public void LogVerbose(Exception exception, string messageTemplate, params object[] propertyValues)
		{
			_logger.LogVerbose(exception, ModifyTemplate(messageTemplate), ExtendPropertyValues(propertyValues));
		}

		public void LogDebug(string messageTemplate, params object[] propertyValues)
		{
			_logger.LogDebug(ModifyTemplate(messageTemplate), ExtendPropertyValues(propertyValues));
		}

		public void LogDebug(Exception exception, string messageTemplate, params object[] propertyValues)
		{
			_logger.LogDebug(exception, ModifyTemplate(messageTemplate), ExtendPropertyValues(propertyValues));
		}

		public void LogInformation(string messageTemplate, params object[] propertyValues)
		{
			_logger.LogInformation(ModifyTemplate(messageTemplate), ExtendPropertyValues(propertyValues));
		}

		public void LogInformation(Exception exception, string messageTemplate, params object[] propertyValues)
		{
			_logger.LogInformation(exception, ModifyTemplate(messageTemplate), ExtendPropertyValues(propertyValues));
		}

		public void LogWarning(string messageTemplate, params object[] propertyValues)
		{
			_logger.LogWarning(ModifyTemplate(messageTemplate), ExtendPropertyValues(propertyValues));
		}

		public void LogWarning(Exception exception, string messageTemplate, params object[] propertyValues)
		{
			_logger.LogWarning(exception, ModifyTemplate(messageTemplate), ExtendPropertyValues(propertyValues));
		}

		public void LogError(string messageTemplate, params object[] propertyValues)
		{
			_logger.LogError(ModifyTemplate(messageTemplate), ExtendPropertyValues(propertyValues));
		}

		public void LogError(Exception exception, string messageTemplate, params object[] propertyValues)
		{
			_logger.LogError(exception, ModifyTemplate(messageTemplate), ExtendPropertyValues(propertyValues));
		}

		public void LogFatal(string messageTemplate, params object[] propertyValues)
		{
			_logger.LogFatal(ModifyTemplate(messageTemplate), ExtendPropertyValues(propertyValues));
		}

		public void LogFatal(Exception exception, string messageTemplate, params object[] propertyValues)
		{
			_logger.LogFatal(exception, ModifyTemplate(messageTemplate), ExtendPropertyValues(propertyValues));
		}

		private object[] ExtendPropertyValues(object[] propertyValues)
		{
			List<object> properties = new List<object>(propertyValues)
			{
				_jobParameters.CorrelationId,
				_jobParameters.SyncConfigurationArtifactId,
				_jobParameters.WorkspaceId,
				_jobParameters.IntegrationPointArtifactId
			};
			return properties.ToArray();
		}

		private string ModifyTemplate(string messageTemplate)
		{
			return messageTemplate += " Sync job properties: " +
				"CorrelationId: {CorrelationId} " +
				"SyncConfigurationArtifactId: {SyncConfigurationArtifactId} " +
				"WorkspaceId: {WorkspaceId} " +
				"IntegrationPointArtifactId: {IntegrationPointArtifactId}";
		}
	}
}