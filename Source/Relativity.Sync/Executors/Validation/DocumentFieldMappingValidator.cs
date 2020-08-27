using System;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Pipelines;

namespace Relativity.Sync.Executors.Validation
{
	internal sealed class DocumentFieldMappingValidator : BaseFieldMappingValidatorBase
	{
		public DocumentFieldMappingValidator(ISourceServiceFactoryForUser sourceServiceFactoryForUser, IDestinationServiceFactoryForUser destinationServiceFactoryForUser, ISyncLog logger) : base(sourceServiceFactoryForUser, destinationServiceFactoryForUser, logger)
		{
		}

		public override async Task<ValidationResult> ValidateAsync(IValidationConfiguration configuration, CancellationToken token)
		{
			_logger.LogVerbose("Validating field mappings");

			try
			{
				var allMessages = await BaseValidateAsync(configuration, onlyIdentifierShouldBeMapped: false, token).ConfigureAwait(false);
				
				return new ValidationResult(allMessages.ToArray());
			}
			catch (Exception ex)
			{
				const string message = "Exception occurred during field mappings validation. See logs for more details.";
				_logger.LogError(ex, message);
				return new ValidationResult(new ValidationMessage(message));
			}
		}

		public override bool ShouldValidate(ISyncPipeline pipeline)
		{
			Type pipelineType = pipeline.GetType();

			return pipelineType == typeof(SyncDocumentRetryPipeline) || pipelineType == typeof(SyncDocumentRunPipeline);
		}
	}
}