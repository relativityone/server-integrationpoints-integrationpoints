using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Services.DataContracts.DTOs;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Executors.Validation
{
	internal sealed class FieldMappingsValidator : IValidator
	{
		private const int _DOCUMENT_ARTIFACT_TYPE_ID = (int) ArtifactType.Document;

		private readonly ISourceServiceFactoryForUser _sourceServiceFactoryForUser;
		private readonly IDestinationServiceFactoryForUser _destinationServiceFactoryForUser;
		private readonly ISyncLog _logger;

		public FieldMappingsValidator(ISourceServiceFactoryForUser sourceServiceFactoryForUser, IDestinationServiceFactoryForUser destinationServiceFactoryForUser, ISyncLog logger)
		{
			_sourceServiceFactoryForUser = sourceServiceFactoryForUser;
			_destinationServiceFactoryForUser = destinationServiceFactoryForUser;
			_logger = logger;
		}

		public async Task<ValidationResult> ValidateAsync(IValidationConfiguration configuration, CancellationToken token)
		{
			_logger.LogVerbose("Validating field mappings");

			try
			{
				IList<FieldMap> fieldMaps = configuration.GetFieldMappings();
				Task<ValidationMessage> validateDestinationFieldsTask = ValidateDestinationFields(configuration, fieldMaps, token);
				Task<ValidationMessage> validateSourceFieldsTask = ValidateSourceFields(configuration, fieldMaps, token);

				var allMessages = new List<ValidationMessage>();
				ValidationMessage[] fieldMappingValidationMessages = await Task.WhenAll(validateDestinationFieldsTask, validateSourceFieldsTask).ConfigureAwait(false);
				allMessages.AddRange(fieldMappingValidationMessages);

				ValidationMessage validateUniqueIdentifier = ValidateUniqueIdentifier(fieldMaps);
				allMessages.Add(validateUniqueIdentifier);

				ValidationMessage validateFieldOverlayBehavior = ValidateFieldOverlayBehavior(configuration);
				allMessages.Add(validateFieldOverlayBehavior);

				return new ValidationResult(allMessages.ToArray());
			}
			catch (Exception ex)
			{
				const string message = "Exception occurred during field mappings validation. See logs for more details.";
				_logger.LogError(ex, message);
				return new ValidationResult(new ValidationMessage(message));
			}
		}

		private ValidationMessage ValidateUniqueIdentifier(IList<FieldMap> mappedFields)
		{
			_logger.LogVerbose("Validating unique identifier");

			bool isIdentifierMapped = mappedFields.Any(x => x.FieldMapType == FieldMapType.Identifier &&
													x.SourceField != null &&
													x.SourceField.IsIdentifier);

			if (!isIdentifierMapped)
			{
				return new ValidationMessage("The unique identifier must be mapped.");
			}

			bool anyIdentifierNotMatchingAnother = mappedFields.Any(x =>
													x.SourceField != null &&
													x.DestinationField != null &&
													x.SourceField.IsIdentifier &&
													!x.DestinationField.IsIdentifier);

			if (anyIdentifierNotMatchingAnother)
			{
				return new ValidationMessage("Identifier must be mapped with another identifier.");
			}

			return null;
		}

		private ValidationMessage ValidateFieldOverlayBehavior(IValidationConfiguration configuration)
		{
			_logger.LogVerbose("Validating field overlay behavior");

			ValidationMessage validationMessage = null;

			if (configuration.ImportOverwriteMode == ImportOverwriteMode.AppendOnly &&
				configuration.FieldOverlayBehavior != FieldOverlayBehavior.UseFieldSettings)
			{
				validationMessage = new ValidationMessage("For Append Only should be set \"Use Field Settings\" overlay behavior.");
			}

			return validationMessage;
		}

		private async Task<ValidationMessage> ValidateDestinationFields(IValidationConfiguration configuration, IList<FieldMap> fieldMaps, CancellationToken token)
		{
			_logger.LogVerbose("Validating fields in destination workspace");

			ValidationMessage validationMessage = null;

			IDictionary<int, string> mappedFieldIdNames = fieldMaps.ToDictionary(x => x.DestinationField.FieldIdentifier, x => x.DestinationField.DisplayName);
			IDictionary<int, string> missingFields = await GetMissingFieldsAsync(_destinationServiceFactoryForUser, mappedFieldIdNames, configuration.DestinationWorkspaceArtifactId, token).ConfigureAwait(false);
			if (missingFields.Count > 0)
			{
				validationMessage =
					new ValidationMessage("20.005", $"Destination field(s) mapped may no longer be available or has been renamed. Review the mapping for the following field(s): {string.Join(",", missingFields.Values)}.");
			}

			return validationMessage;
		}

		private async Task<ValidationMessage> ValidateSourceFields(IValidationConfiguration configuration, IList<FieldMap> fieldMaps, CancellationToken token)
		{
			_logger.LogVerbose("Validating fields in source workspace");

			ValidationMessage validationMessage = null;

			IDictionary<int, string> mappedFieldIdNames = fieldMaps.ToDictionary(x => x.SourceField.FieldIdentifier, x => x.SourceField.DisplayName);
			IDictionary<int, string> missingFields = await GetMissingFieldsAsync(_sourceServiceFactoryForUser, mappedFieldIdNames, configuration.SourceWorkspaceArtifactId, token).ConfigureAwait(false);
			if (missingFields.Count > 0)
			{
				validationMessage =
					new ValidationMessage($"Source field(s) mapped may no longer be available or has been renamed. Review the mapping for the following field(s): {string.Join(",", missingFields.Values)}.");
			}

			return validationMessage;
		}

		private static async Task<IDictionary<int, string>> GetMissingFieldsAsync(IProxyFactory proxyFactory, IDictionary<int, string> mappedFieldIdNames, int workspaceArtifactId, CancellationToken token)
		{
			string fieldArtifactIds = string.Join(",", mappedFieldIdNames.Keys);
			using (IObjectManager objectManager = await proxyFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				QueryRequest request = new QueryRequest()
				{
					Condition = $"(('FieldArtifactTypeID' == {_DOCUMENT_ARTIFACT_TYPE_ID} AND 'ArtifactID' IN [{fieldArtifactIds}]))",
					ObjectType = new ObjectTypeRef()
					{
						Name = "Field"
					},
					IncludeNameInQueryResult = true
				};

				const int start = 0;
				QueryResult queryResult = await objectManager.QueryAsync(workspaceArtifactId, request, start, mappedFieldIdNames.Count, token, new EmptyProgress<ProgressReport>()).ConfigureAwait(false);
				IDictionary<int, string> refFieldIdNames = queryResult.Objects.ToDictionary(x => x.ArtifactID, x => x.Name);

				var missingFields = mappedFieldIdNames.Except(refFieldIdNames).ToDictionary(x => x.Key, x => x.Value);

				return missingFields;
			}
		}
	}
}