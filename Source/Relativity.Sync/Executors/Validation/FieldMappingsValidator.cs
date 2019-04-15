﻿using System;
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
				IList<FieldMap> fieldMaps = configuration.FieldMappings;
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
				const string message = "Exception occurred during field mappings validation.";
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
				configuration.FieldOverlayBehavior != FieldOverlayBehavior.Default)
			{
				validationMessage = new ValidationMessage("For Append Only should be set \"Use Field Settings\" overlay behavior.");
			}

			return validationMessage;
		}

		private async Task<ValidationMessage> ValidateDestinationFields(IValidationConfiguration configuration, IList<FieldMap> fieldMaps, CancellationToken token)
		{
			_logger.LogVerbose("Validating fields in destination workspace");

			ValidationMessage validationMessage = null;

			List<int> fieldIds = fieldMaps.Select(x => x.DestinationField.FieldIdentifier).ToList();
			IList<int> missingFields = await GetMissingFieldsAsync(_destinationServiceFactoryForUser, fieldIds, configuration.DestinationWorkspaceArtifactId, token).ConfigureAwait(false);
			if (missingFields.Count > 0)
			{
				IEnumerable<string> fieldNames =
					fieldMaps.Where(fm => missingFields.Contains(fm.DestinationField.FieldIdentifier)).Select(fm => $"'{fm.DestinationField.DisplayName}'");
				validationMessage =
					new ValidationMessage("20.005", $"Destination field(s) mapped may no longer be available or has been renamed. Review the mapping for the following field(s): {fieldNames}.");
			}

			return validationMessage;
		}

		private async Task<ValidationMessage> ValidateSourceFields(IValidationConfiguration configuration, IList<FieldMap> fieldMaps, CancellationToken token)
		{
			_logger.LogVerbose("Validating fields in source workspace");

			ValidationMessage validationMessage = null;

			List<int> fieldIds = fieldMaps.Select(x => x.SourceField.FieldIdentifier).ToList();
			IList<int> missingFields = await GetMissingFieldsAsync(_sourceServiceFactoryForUser, fieldIds, configuration.SourceWorkspaceArtifactId, token).ConfigureAwait(false);
			if (missingFields.Count > 0)
			{
				IEnumerable<string> fieldNames =
					fieldMaps.Where(fm => missingFields.Contains(fm.SourceField.FieldIdentifier)).Select(fm => $"'{fm.SourceField.DisplayName}'");
				validationMessage =
					new ValidationMessage($"Source field(s) mapped may no longer be available or has been renamed. Review the mapping for the following field(s): {fieldNames}.");
			}

			return validationMessage;
		}

		private async Task<IList<int>> GetMissingFieldsAsync(IProxyFactory proxyFactory, IList<int> fieldIds, int workspaceArtifactId, CancellationToken token)
		{
			string fieldArtifactIds = string.Join(",", fieldIds);
			using (IObjectManager objectManager = await proxyFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				QueryRequest request = new QueryRequest()
				{
					Condition = $"(('FieldArtifactTypeID' == 10 AND 'ArtifactID' IN [{fieldArtifactIds}]))",
					ObjectType = new ObjectTypeRef()
					{
						Name = "Field"
					},
					IncludeNameInQueryResult = true
				};

				const int start = 0;
				QueryResult queryResult = await objectManager.QueryAsync(workspaceArtifactId, request, start, fieldIds.Count, token, new EmptyProgress<ProgressReport>()).ConfigureAwait(false);
				IEnumerable<int> artifactIds = queryResult.Objects.Select(x => x.ArtifactID);

				List<int> missingFields = fieldIds.Except(artifactIds).ToList();
				return missingFields;
			}
		}
	}
}