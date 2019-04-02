using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using kCura.Apps.Common.Utils.Serializers;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Executors.Validation
{
	internal sealed class FieldMappingsValidator : IValidator
	{
		private readonly ISourceServiceFactoryForUser _sourceServiceFactoryForUser;
		private readonly IDestinationServiceFactoryForUser _destinationServiceFactoryForUser;
		private readonly ISerializer _serializer;

		public FieldMappingsValidator(ISourceServiceFactoryForUser sourceServiceFactoryForUser, IDestinationServiceFactoryForUser destinationServiceFactoryForUser, ISerializer serializer)
		{
			_sourceServiceFactoryForUser = sourceServiceFactoryForUser;
			_destinationServiceFactoryForUser = destinationServiceFactoryForUser;
			_serializer = serializer;
		}

		public async Task<ValidationResult> ValidateAsync(IValidationConfiguration configuration, CancellationToken token)
		{
			List<FieldMap> fieldMaps = _serializer.Deserialize<List<FieldMap>>(configuration.FieldsMap);
			Task<ValidationMessage> validateDestinationFieldsTask = ValidateDestinationFields(configuration, fieldMaps);
			Task<ValidationMessage> validateSourceFieldsTask = ValidateSourceFields(configuration, fieldMaps);

			ValidationMessage[] allMessages = await Task.WhenAll(validateDestinationFieldsTask, validateSourceFieldsTask).ConfigureAwait(false);
			return new ValidationResult(allMessages);
		}

		private async Task<ValidationMessage> ValidateDestinationFields(IValidationConfiguration configuration, IList<FieldMap> fieldMaps)
		{
			ValidationMessage validationMessage = null;

			List<int> fieldIds = fieldMaps.Select(x => int.Parse(x.DestinationField.FieldIdentifier, CultureInfo.InvariantCulture)).ToList();
			IList<int> missingFields = await GetMissingFieldsAsync(_destinationServiceFactoryForUser, fieldIds, configuration.DestinationWorkspaceArtifactId).ConfigureAwait(false);
			if (missingFields.Count > 0)
			{
				IEnumerable<string> fieldNames =
					fieldMaps.Where(fm => missingFields.Contains(int.Parse(fm.DestinationField.FieldIdentifier, CultureInfo.InvariantCulture))).Select(fm => $"'{fm.DestinationField.DisplayName}'");
				validationMessage =
					new ValidationMessage("20.005", $"Destination field(s) mapped may no longer be available or has been renamed. Review the mapping for the following field(s): {fieldNames}.");
			}

			return validationMessage;
		}

		private async Task<ValidationMessage> ValidateSourceFields(IValidationConfiguration configuration, IList<FieldMap> fieldMaps)
		{
			ValidationMessage validationMessage = null;

			List<int> fieldIds = fieldMaps.Select(x => int.Parse(x.SourceField.FieldIdentifier, CultureInfo.InvariantCulture)).ToList();
			IList<int> missingFields = await GetMissingFieldsAsync(_sourceServiceFactoryForUser, fieldIds, configuration.SourceWorkspaceArtifactId).ConfigureAwait(false);
			if (missingFields.Count > 0)
			{
				IEnumerable<string> fieldNames =
					fieldMaps.Where(fm => missingFields.Contains(int.Parse(fm.SourceField.FieldIdentifier, CultureInfo.InvariantCulture))).Select(fm => $"'{fm.SourceField.DisplayName}'");
				validationMessage =
					new ValidationMessage($"Source field(s) mapped may no longer be available or has been renamed. Review the mapping for the following field(s): {fieldNames}.");
			}

			return validationMessage;
		}

		private async Task<IList<int>> GetMissingFieldsAsync(IProxyFactory proxyFactory, IList<int> fieldIds, int workspaceArtifactId)
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
				QueryResult queryResult = await objectManager.QueryAsync(workspaceArtifactId, request, start, fieldIds.Count).ConfigureAwait(false);
				IEnumerable<int> artifactIds = queryResult.Objects.Select(x => x.ArtifactID);

				List<int> missingFields = fieldIds.Except(artifactIds).ToList();
				return missingFields;
			}
		}
	}
}