using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Pipelines;
using Relativity.Sync.Pipelines.Extensions;

namespace Relativity.Sync.Executors.Validation
{
	internal sealed class FieldMappingValidator : FieldMappingValidatorBase
	{
		public FieldMappingValidator(ISourceServiceFactoryForUser sourceServiceFactoryForUser, IDestinationServiceFactoryForUser destinationServiceFactoryForUser, ISyncLog logger)
			: base(sourceServiceFactoryForUser, destinationServiceFactoryForUser, logger)
		{
		}

		public override async Task<ValidationResult> ValidateAsync(IValidationConfiguration configuration, CancellationToken token)
		{
			_logger.LogInformation("Validating field mappings");

			try
			{
				List<ValidationMessage> allMessages = await BaseValidateAsync(configuration, onlyIdentifierShouldBeMapped: false, token).ConfigureAwait(false);
				
				return new ValidationResult(allMessages.ToArray());
			}
			catch (Exception ex)
			{
				const string message = "Exception occurred during field mappings validation. See logs for more details.";
				_logger.LogError(ex, message);
				throw;
			}
		}

		public override bool ShouldValidate(ISyncPipeline pipeline) => pipeline.IsDocumentPipeline() || pipeline.IsNonDocumentPipeline();
	}
}