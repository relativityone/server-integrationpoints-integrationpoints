using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Pipelines;
using Relativity.Sync.Pipelines.Extensions;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Executors.Validation
{
    internal sealed class FieldMappingValidator : FieldMappingValidatorBase
    {
        private readonly IFieldManager _fieldManager;

        public FieldMappingValidator(ISourceServiceFactoryForUser sourceServiceFactoryForUser, IDestinationServiceFactoryForUser destinationServiceFactoryForUser, IFieldManager fieldManager, IAPILog logger)
            : base(sourceServiceFactoryForUser, destinationServiceFactoryForUser, logger)
        {
            _fieldManager = fieldManager;
        }

        public override async Task<ValidationResult> ValidateAsync(IValidationConfiguration configuration, CancellationToken token)
        {
            _logger.LogInformation("Validating field mappings");

            try
            {
                List<ValidationMessage> allMessages = await BaseValidateAsync(configuration, onlyIdentifierShouldBeMapped: false, token).ConfigureAwait(false);

                ValidationMessage validateUnsupportedFields = await ValidateUnsupportedTypesOfMappedFields(token).ConfigureAwait(false);
                allMessages.Add(validateUnsupportedFields);

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

        private async Task<ValidationMessage> ValidateUnsupportedTypesOfMappedFields(CancellationToken token)
        {
            _logger.LogInformation("Validating mapped fields types");

            ValidationMessage validationMessage = null;

            IList<FieldInfoDto> mappedFields = await _fieldManager.GetMappedFieldsAsync(token).ConfigureAwait(false);
            List<string> unsupportedFieldsNames = mappedFields.Where(x => x.RelativityDataType == RelativityDataType.File).Select(x => x.SourceFieldName).ToList();
            if (unsupportedFieldsNames.Count != 0)
            {
                validationMessage = new ValidationMessage($"Following fields have unsupported type '{nameof(RelativityDataType.File)}': {string.Join(",", unsupportedFieldsNames)}.");
            }

            return validationMessage;
        }
    }
}
