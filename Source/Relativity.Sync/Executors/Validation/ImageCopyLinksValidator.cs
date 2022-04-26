using Relativity.API;
using Relativity.Sync.Configuration;
using Relativity.Sync.Pipelines;
using Relativity.Sync.Pipelines.Extensions;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Executors.Validation
{
	internal sealed class ImageCopyLinksValidator : CopyLinksValidatorBase
	{
		public ImageCopyLinksValidator(IInstanceSettings instanceSettings, IUserContextConfiguration userContext, INonAdminCanSyncUsingLinks nonAdminCanSyncUsingLinks, IUserService userService, IAPILog logger) : base(instanceSettings, userContext, nonAdminCanSyncUsingLinks, userService, logger)
		{
		}

		protected override string ValidatorKind => "images";
		
		public override bool ShouldValidate(ISyncPipeline pipeline) => pipeline.IsImagePipeline();

		protected override bool ShouldNotValidateReferentialFileLinksRestriction(IValidationConfiguration configuration)
		{
			return configuration.ImportImageFileCopyMode != ImportImageFileCopyMode.SetFileLinks;
		}
	}
}
