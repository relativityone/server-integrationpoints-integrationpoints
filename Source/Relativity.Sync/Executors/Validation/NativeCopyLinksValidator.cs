using Relativity.API;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Pipelines;
using Relativity.Sync.Pipelines.Extensions;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Executors.Validation
{
	internal sealed class NativeCopyLinksValidator : CopyLinksValidatorBase
	{
		public NativeCopyLinksValidator(IInstanceSettings instanceSettings, IUserContextConfiguration userContext, ISourceServiceFactoryForAdmin serviceFactoryForAdmin, INonAdminCanSyncUsingLinks nonAdminCanSyncUsingUsingLinks, IUserService userService, IAPILog logger) : base(instanceSettings, userContext, nonAdminCanSyncUsingUsingLinks, userService, logger)
		{
		}

		protected override string ValidatorKind => "native files";

		public override bool ShouldValidate(ISyncPipeline pipeline) => pipeline.IsDocumentPipeline();

		protected override bool ShouldNotValidateReferentialFileLinksRestriction(IValidationConfiguration configuration)
		{
			return configuration.ImportNativeFileCopyMode != ImportNativeFileCopyMode.SetFileLinks;
		}
	}
}
