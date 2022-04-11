using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Pipelines;
using Relativity.Sync.Pipelines.Extensions;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Executors.Validation
{
	internal sealed class ImageCopyLinksValidator : CopyLinksValidatorBase
	{
		public ImageCopyLinksValidator(IInstanceSettings instanceSettings, IUserContextConfiguration userContext, ISourceServiceFactoryForAdmin serviceFactoryForAdmin, INonAdminCanSyncUsingLinks nonAdminCanSyncUsingLinks, ISyncLog logger) : base(instanceSettings, userContext, serviceFactoryForAdmin, nonAdminCanSyncUsingLinks, logger)
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
