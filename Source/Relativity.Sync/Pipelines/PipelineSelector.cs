﻿using Relativity.Sync.Configuration;

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
			return _selectedPipeline ?? (_selectedPipeline = GetPipelineInternal(IsRetryJob(), IsImageJob()));
		}

		private ISyncPipeline GetPipelineInternal(bool isRetryJob, bool isImageJob)
		{
			_logger.LogInformation("Getting pipeline for parameters: {isRetryJob}, {isImageJob}", isRetryJob, isImageJob);

			ISyncPipeline selectedPipeline;

			switch (isImageJob)
			{
				case (false) when isRetryJob:
					selectedPipeline = new SyncDocumentRetryPipeline();
					break;
				case (false):
					selectedPipeline = new SyncDocumentRunPipeline();
					break;
				case (true) when isRetryJob:
					selectedPipeline = new SyncImageRetryPipeline();
					break;
				case (true):
					selectedPipeline = new SyncImageRunPipeline();
					break;
			}

			const string messageTemplate = "Selected pipeline of type: {pipelineType}";

			_logger.LogInformation(messageTemplate, selectedPipeline.GetType().Name);
			return selectedPipeline;
		}

		private bool IsRetryJob()
		{
			return _pipelineSelectorConfiguration.JobHistoryToRetryId != null;
		}

		private bool IsImageJob()
		{
			return _pipelineSelectorConfiguration.IsImageJob;
		}
	}
}