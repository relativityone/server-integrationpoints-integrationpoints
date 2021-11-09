using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Services.DataContracts.DTOs;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Pipelines;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Executors.Validation
{
	internal abstract class FieldMappingValidatorBase : IValidator
	{
		private readonly ISourceServiceFactoryForUser _sourceServiceFactoryForUser;
		private readonly IDestinationServiceFactoryForUser _destinationServiceFactoryForUser;
		protected readonly ISyncLog _logger;

		protected FieldMappingValidatorBase(ISourceServiceFactoryForUser sourceServiceFactoryForUser, IDestinationServiceFactoryForUser destinationServiceFactoryForUser, ISyncLog logger)
		{
			_sourceServiceFactoryForUser = sourceServiceFactoryForUser;
			_destinationServiceFactoryForUser = destinationServiceFactoryForUser;
			_logger = logger;
		}

		public abstract Task<ValidationResult> ValidateAsync(IValidationConfiguration configuration, CancellationToken token);
		public abstract bool ShouldValidate(ISyncPipeline pipeline);

		protected async Task<List<ValidationMessage>> BaseValidateAsync(IValidationConfiguration configuration, bool onlyIdentifierShouldBeMapped, CancellationToken token)
		{
			IList<FieldMap> fieldMaps = configuration.GetFieldMappings();
			var allMessages = new List<ValidationMessage>();

			ValidationMessage validateUniqueIdentifier =
				ValidateUniqueIdentifier(fieldMaps, onlyIdentifierShouldBeMapped);
			allMessages.Add(validateUniqueIdentifier);

			Task<ValidationMessage> validateDestinationFieldsTask = ValidateDestinationFieldsAsync(configuration, fieldMaps, token);
			Task<ValidationMessage> validateSourceFieldsTask = ValidateSourceFieldsAsync(configuration, fieldMaps, token);

			ValidationMessage[] fieldMappingValidationMessages =
				await Task.WhenAll(validateDestinationFieldsTask, validateSourceFieldsTask).ConfigureAwait(false);
			allMessages.AddRange(fieldMappingValidationMessages);

			ValidationMessage validateFieldOverlayBehavior = ValidateFieldOverlayBehavior(configuration);
			allMessages.Add(validateFieldOverlayBehavior);
			return allMessages;
		}

		protected ValidationMessage ValidateUniqueIdentifier(IList<FieldMap> mappedFields, bool onlyIdentifierShouldBeMapped = false)
		{
			_logger.LogInformation("Validating unique identifier");

			bool isIdentifierMapped = mappedFields.Any(x => x.FieldMapType == FieldMapType.Identifier &&
															x.SourceField != null &&
															x.SourceField.IsIdentifier);

			if (!isIdentifierMapped)
			{
				return new ValidationMessage("The unique identifier must be mapped.");
			}

			if (onlyIdentifierShouldBeMapped && mappedFields.Count > 1)
			{
				return new ValidationMessage("Only unique identifier must be mapped.");
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

		protected ValidationMessage ValidateFieldOverlayBehavior(IValidationConfiguration configuration)
		{
			_logger.LogInformation("Validating field overlay behavior");

			ValidationMessage validationMessage = null;

			if (configuration.ImportOverwriteMode == ImportOverwriteMode.AppendOnly &&
				configuration.FieldOverlayBehavior != FieldOverlayBehavior.UseFieldSettings)
			{
				validationMessage = new ValidationMessage("For Append Only should be set \"Use Field Settings\" overlay behavior.");
			}

			return validationMessage;
		}

		protected async Task<ValidationMessage> ValidateDestinationFieldsAsync(IValidationConfiguration configuration, IList<FieldMap> fieldMaps, CancellationToken token)
		{
			_logger.LogInformation("Validating fields in destination workspace");

			ValidationMessage validationMessage = null;

			IDictionary<int, string> mappedFieldIdNames = fieldMaps.ToDictionary(x => x.DestinationField.FieldIdentifier, x => x.DestinationField.DisplayName);
			IDictionary<int, string> missingFields = await GetMissingFieldsAsync(_destinationServiceFactoryForUser, mappedFieldIdNames, configuration.DestinationWorkspaceArtifactId, configuration.RdoArtifactTypeId, token).ConfigureAwait(false);
			if (missingFields.Count > 0)
			{
				validationMessage =
					new ValidationMessage("20.005", $"Destination field(s) mapped may no longer be available or has been renamed. Review the mapping for the following field(s): {string.Join(",", missingFields.Values)}.");
			}

			return validationMessage;
		}

		protected async Task<ValidationMessage> ValidateSourceFieldsAsync(IValidationConfiguration configuration,
			IList<FieldMap> fieldMaps, CancellationToken token)
		{
			_logger.LogInformation("Validating fields in source workspace");

			ValidationMessage validationMessage = null;

			IDictionary<int, string> mappedFieldIdNames = fieldMaps.ToDictionary(x => x.SourceField.FieldIdentifier, x => x.SourceField.DisplayName);
			IDictionary<int, string> missingFields = await GetMissingFieldsAsync(_sourceServiceFactoryForUser, mappedFieldIdNames, configuration.SourceWorkspaceArtifactId, configuration.RdoArtifactTypeId, token).ConfigureAwait(false);
			if (missingFields.Count > 0)
			{
				validationMessage =
					new ValidationMessage($"Source field(s) mapped may no longer be available or has been renamed. Review the mapping for the following field(s): {string.Join(",", missingFields.Values)}.");
			}

			return validationMessage;
		}

		private static async Task<IDictionary<int, string>> GetMissingFieldsAsync(IProxyFactory proxyFactory, IDictionary<int, string> mappedFieldIdNames, int workspaceArtifactId, int rdoType, CancellationToken token)
		{
			string fieldArtifactIds = string.Join(",", mappedFieldIdNames.Keys);
			using (IObjectManager objectManager = await proxyFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				QueryRequest request = new QueryRequest()
				{
					Condition = $"(('FieldArtifactTypeID' == {rdoType} AND 'ArtifactID' IN [{fieldArtifactIds}]))",
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