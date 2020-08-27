using Relativity.Sync.Configuration;

namespace Relativity.Sync.Pipelines
{
	internal class PipelineSelector : IPipelineSelector
	{
		private readonly IPipelineSelectorConfiguration _pipelineSelectorConfiguration;
		private readonly ISyncLog _logger;

		private ISyncPipeline _selectedPipeline;

		public PipelineSelector(IPipelineSelectorConfiguration pipelineSelectorConfiguration, ISyncLog logger)
		{
			_pipelineSelectorConfiguration = pipelineSelectorConfiguration;
			_logger = logger;
		}

		public ISyncPipeline GetPipeline()
		{
			return _selectedPipeline ?? (_selectedPipeline = GetPipelineInternal());
		}

		private ISyncPipeline GetPipelineInternal()
		{
			_logger.LogInformation("Getting pipeline type");
			const string messageTemplate = "Returning {pipelineType}";
			if (IsDocumentRetry())
			{
				_logger.LogInformation(messageTemplate, nameof(SyncDocumentRetryPipeline));
				return new SyncDocumentRetryPipeline();
			}

			_logger.LogInformation(messageTemplate, nameof(SyncDocumentRunPipeline));
			return new SyncDocumentRunPipeline();
		}

		private bool IsDocumentRetry()
		{
			return _pipelineSelectorConfiguration.JobHistoryToRetryId != null;
		}
	}
}