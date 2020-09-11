﻿using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Pipelines;
using Relativity.Sync.Pipelines.Extensions;

namespace Relativity.Sync.Executors.Validation
{
	internal sealed class ImageCopyLinksValidator : CopyLinksValidatorBase
	{
		public ImageCopyLinksValidator(IInstanceSettings instanceSettings, IUserContextConfiguration userContext, ISourceServiceFactoryForAdmin serviceFactory, ISyncLog logger) : base(instanceSettings, userContext, serviceFactory, logger)
		{
		}

		protected override string ValidatorKind => "images";
		
		public override bool ShouldValidate(ISyncPipeline pipeline) => pipeline.IsImagePipeline();

		protected override bool ShouldValidateReferentialFileLinksRestriction(IValidationConfiguration configuration)
		{
			return configuration.ImportImageFileCopyMode != ImportImageFileCopyMode.SetFileLinks;
		}
	}
}
