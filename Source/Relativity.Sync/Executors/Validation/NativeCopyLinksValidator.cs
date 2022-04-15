using Relativity.API;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Pipelines;
using Relativity.Sync.Pipelines.Extensions;

namespace Relativity.Sync.Executors.Validation
{
	internal sealed class NativeCopyLinksValidator : CopyLinksValidatorBase
	{
		public NativeCopyLinksValidator(IInstanceSettings instanceSettings, IUserContextConfiguration userContext, ISourceServiceFactoryForAdmin serviceFactoryForAdmin, IAPILog logger) : base(instanceSettings, userContext, serviceFactoryForAdmin, logger)
		{
		}

		protected override string ValidatorKind => "native files";

		public override bool ShouldValidate(ISyncPipeline pipeline) => pipeline.IsDocumentPipeline();

		protected override bool ShouldValidateReferentialFileLinksRestriction(IValidationConfiguration configuration)
		{
			return configuration.ImportNativeFileCopyMode != ImportNativeFileCopyMode.SetFileLinks;
		}
	}
}
