using System;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Pipelines;

namespace Relativity.Sync.Executors.Validation
{
	internal sealed class NativeCopyLinksValidator : CopyLinksValidatorBase
	{
		public NativeCopyLinksValidator(IInstanceSettings instanceSettings, IUserContextConfiguration userContext, ISourceServiceFactoryForAdmin serviceFactory, ISyncLog logger) : base(instanceSettings, userContext, serviceFactory, logger)
		{
		}

		protected override string ValidatorKind => "native files";

		public override bool ShouldValidate(ISyncPipeline pipeline)
		{
			Type pipelineType = pipeline.GetType();

			return pipelineType == typeof(SyncDocumentRunPipeline) || pipelineType == typeof(SyncDocumentRetryPipeline);
		}

		protected override bool ShouldSkipValidation(IValidationConfiguration configuration)
		{
			return configuration.ImportNativeFileCopyMode != ImportNativeFileCopyMode.SetFileLinks;
		}
	}
}