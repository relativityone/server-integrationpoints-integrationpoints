using System;

namespace Relativity.Sync.Pipelines.Extensions
{
	internal static class ISyncPipelineExtensions
	{	
		public static bool IsDocumentPipeline(this ISyncPipeline syncPipeline)
		{
			Type pipelineType = syncPipeline.GetType();

			return pipelineType == typeof(SyncDocumentRunPipeline)
				|| pipelineType == typeof(SyncDocumentRetryPipeline);
		}

		public static bool IsImagePipeline(this ISyncPipeline syncPipeline)
		{
			Type pipelineType = syncPipeline.GetType();

			return pipelineType == typeof(SyncImageRunPipeline)
				|| pipelineType == typeof(SyncImageRetryPipeline);
		}

		public static bool IsRetryPipeline(this ISyncPipeline syncPipeline)
		{
			Type pipelineType = syncPipeline.GetType();

			return pipelineType == typeof(SyncDocumentRetryPipeline)
				|| pipelineType == typeof(SyncImageRetryPipeline);
		}

		public static bool IsNonDocumentPipeline(this ISyncPipeline pipeline) => pipeline is SyncNonDocumentRunPipeline;
	}
}
