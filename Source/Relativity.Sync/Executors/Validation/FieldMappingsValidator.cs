using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using kCura.Apps.Common.Utils.Serializers;
using Relativity.Services.DataContracts.DTOs;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Executors.Validation
{
	internal sealed class FieldMappingsValidator : IValidator
	{
		private const string _FIELD_MAP_FIELD_OVERLAY_BEHAVIOR_MERGE = "Merge Values";
		private const string _FIELD_MAP_FIELD_OVERLAY_BEHAVIOR_REPLACE = "Replace Values";
		private const string _FIELD_MAP_FIELD_OVERLAY_BEHAVIOR_DEFAULT = "Use Field Settings";

		private readonly ISourceServiceFactoryForUser _sourceServiceFactoryForUser;
		private readonly IDestinationServiceFactoryForUser _destinationServiceFactoryForUser;
		private readonly ISerializer _serializer;
		private readonly ISyncLog _logger;

		public FieldMappingsValidator(ISourceServiceFactoryForUser sourceServiceFactoryForUser, IDestinationServiceFactoryForUser destinationServiceFactoryForUser, ISerializer serializer, ISyncLog logger)
		{
			_sourceServiceFactoryForUser = sourceServiceFactoryForUser;
			_destinationServiceFactoryForUser = destinationServiceFactoryForUser;
			_serializer = serializer;
			_logger = logger;
		}

		public async Task<ValidationResult> ValidateAsync(IValidationConfiguration configuration, CancellationToken token)
		{
			_logger.LogVerbose("Validating field mappings");

			try
			{
				List<FieldMap> fieldMaps = _serializer.Deserialize<List<FieldMap>>(configuration.FieldsMap);
				Task<ValidationMessage> validateDestinationFieldsTask = ValidateDestinationFields(configuration, fieldMaps, token);
				Task<ValidationMessage> validateSourceFieldsTask = ValidateSourceFields(configuration, fieldMaps, token);

				List<ValidationMessage> allMessages = new List<ValidationMessage>();
				ValidationMessage[] fieldMappingValidationMessages = await Task.WhenAll(validateDestinationFieldsTask, validateSourceFieldsTask).ConfigureAwait(false);
				allMessages.AddRange(fieldMappingValidationMessages);
				allMessages.Add(ValidateUniqueIdentifier(fieldMaps));
				allMessages.Add(ValidateFieldOverlayBehavior(configuration));

				return new ValidationResult(allMessages);
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

			ValidationMessage validationMessage = null;
			bool isIdentifierMapped = mappedFields.Any(x => x.FieldMapType == FieldMapType.Identifier &&
													x.SourceField != null &&
													x.SourceField.IsIdentifier);

			if (!isIdentifierMapped)
			{
				validationMessage = new ValidationMessage("The unique identifier must be mapped.");
			}

			return validationMessage;
		}

		private ValidationMessage ValidateFieldOverlayBehavior(IValidationConfiguration configuration)
		{
			_logger.LogVerbose("Validating field overlay behavior");

			ValidationMessage validationMessage = null;

			if (configuration.ImportOverwriteMode == ImportOverwriteMode.AppendOnly)
			{
				if (configuration.FieldOverlayBehavior != _FIELD_MAP_FIELD_OVERLAY_BEHAVIOR_DEFAULT)
				{
					validationMessage = new ValidationMessage("For Append Only should be set \"Use Field Settings\" overlay behaior.");
				}
			}
			else
			{
				if (configuration.FieldOverlayBehavior != _FIELD_MAP_FIELD_OVERLAY_BEHAVIOR_MERGE &&
					configuration.FieldOverlayBehavior != _FIELD_MAP_FIELD_OVERLAY_BEHAVIOR_REPLACE &&
					configuration.FieldOverlayBehavior != _FIELD_MAP_FIELD_OVERLAY_BEHAVIOR_DEFAULT)
				{
					validationMessage = new ValidationMessage($"Invalid Overlay Behavior: {configuration.FieldOverlayBehavior}");
				}
			}

			return validationMessage;
		}

		private async Task<ValidationMessage> ValidateDestinationFields(IValidationConfiguration configuration, IList<FieldMap> fieldMaps, CancellationToken token)
		{
			_logger.LogVerbose("Validating fields in destination workspace");

			ValidationMessage validationMessage = null;

			List<int> fieldIds = fieldMaps.Select(x => int.Parse(x.DestinationField.FieldIdentifier, CultureInfo.InvariantCulture)).ToList();
			IList<int> missingFields = await GetMissingFieldsAsync(_destinationServiceFactoryForUser, fieldIds, configuration.DestinationWorkspaceArtifactId, token).ConfigureAwait(false);
			if (missingFields.Count > 0)
			{
				IEnumerable<string> fieldNames =
					fieldMaps.Where(fm => missingFields.Contains(int.Parse(fm.DestinationField.FieldIdentifier, CultureInfo.InvariantCulture))).Select(fm => $"'{fm.DestinationField.DisplayName}'");
				validationMessage =
					new ValidationMessage("20.005", $"Destination field(s) mapped may no longer be available or has been renamed. Review the mapping for the following field(s): {fieldNames}.");
			}

			return validationMessage;
		}

		private async Task<ValidationMessage> ValidateSourceFields(IValidationConfiguration configuration, IList<FieldMap> fieldMaps, CancellationToken token)
		{
			_logger.LogVerbose("Validating fields in source workspace");

			ValidationMessage validationMessage = null;

			List<int> fieldIds = fieldMaps.Select(x => int.Parse(x.SourceField.FieldIdentifier, CultureInfo.InvariantCulture)).ToList();
			IList<int> missingFields = await GetMissingFieldsAsync(_sourceServiceFactoryForUser, fieldIds, configuration.SourceWorkspaceArtifactId, token).ConfigureAwait(false);
			if (missingFields.Count > 0)
			{
				IEnumerable<string> fieldNames =
					fieldMaps.Where(fm => missingFields.Contains(int.Parse(fm.SourceField.FieldIdentifier, CultureInfo.InvariantCulture))).Select(fm => $"'{fm.SourceField.DisplayName}'");
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