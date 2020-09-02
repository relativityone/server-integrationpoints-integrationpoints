using System;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Pipelines;

namespace Relativity.Sync.Executors.Validation
{
	internal sealed class ImageLinksValidationBase : CopyLinksValidatorBase
	{
		public ImageLinksValidationBase(IInstanceSettings instanceSettings, IUserContextConfiguration userContext, ISourceServiceFactoryForAdmin serviceFactory, ISyncLog logger) : base(instanceSettings, userContext, serviceFactory, logger)
		{
		}

		protected override string ValidatorKind => "images";
		public override bool ShouldValidate(ISyncPipeline pipeline)
		{
			Type pipelineType = pipeline.GetType();

			return pipelineType == typeof(SyncImageRunPipeline) || pipelineType == typeof(SyncImageRetryPipeline);
		}

		protected override bool ShouldSkipValidation(IValidationConfiguration configuration)
		{
			return configuration.ImportNativeFileCopyMode != ImportNativeFileCopyMode.SetFileLinks
		}
	}
}
