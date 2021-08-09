using System;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Pipelines;
using Relativity.Sync.Pipelines.Extensions;

namespace Relativity.Sync.Executors.Validation
{
	internal sealed class ImageFieldMappingValidator : FieldMappingValidatorBase
	{
		public ImageFieldMappingValidator(ISourceServiceFactoryForUser sourceServiceFactoryForUser, IDestinationServiceFactoryForUser destinationServiceFactoryForUser, ISyncLog logger) : base(sourceServiceFactoryForUser, destinationServiceFactoryForUser, logger)
		{
		}

		public override async Task<ValidationResult> ValidateAsync(IValidationConfiguration configuration, CancellationToken token)
		{
			_logger.LogVerbose("Validating image field mappings");

			try
			{
				var allMessages = await BaseValidateAsync(configuration, onlyIdentifierShouldBeMapped: true, token).ConfigureAwait(false);

				return new ValidationResult(allMessages.ToArray());
			}
			catch (Exception ex)
			{
				const string message = "Exception occurred during image field mappings validation. See logs for more details.";
				_logger.LogError(ex, message);
				throw;
			}
		}

		public override bool ShouldValidate(ISyncPipeline pipeline) => pipeline.IsImagePipeline();
	}
}
