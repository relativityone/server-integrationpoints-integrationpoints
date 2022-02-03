using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Relativity;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Core.Validation.Abstract
{
	public abstract class BaseIntegrationPointValidator<TValidator> : IIntegrationPointValidator where TValidator : IValidator
	{
		protected readonly ILookup<string, TValidator> _validatorsMap;
		protected readonly IIntegrationPointSerializer _serializer;
		private readonly IServicesMgr _servicesMgr;

		protected BaseIntegrationPointValidator(IEnumerable<TValidator> validators, IIntegrationPointSerializer serializer, IServicesMgr servicesMgr)
		{
			_validatorsMap = validators.ToLookup(x => x.Key);
			_serializer = serializer;
			_servicesMgr = servicesMgr;
		}

		public static string GetProviderValidatorKey(string sourceProviderId, string destinationProviderId)
		{
			sourceProviderId = sourceProviderId.ToUpperInvariant();
			destinationProviderId = destinationProviderId.ToUpperInvariant();

			return $"{sourceProviderId}+{destinationProviderId}";
		}

		public IntegrationPointProviderValidationModel CreateValidationModel(
			IntegrationPointModelBase model,
			SourceProvider sourceProvider,
			DestinationProvider destinationProvider,
			IntegrationPointType integrationPointType,
			Guid objectTypeGuid,
			int userId)
		{
			ImportSettings destinationConfiguration = _serializer.Deserialize<ImportSettings>(model.Destination);

			return new IntegrationPointProviderValidationModel(model)
			{
				ArtifactId = model.ArtifactID,
				ArtifactTypeId = destinationConfiguration.ArtifactTypeId,
				UserId = userId,
				SourceProviderIdentifier = sourceProvider.Identifier,
				SourceProviderArtifactId = sourceProvider.ArtifactId,
				SourceConfiguration = model.SourceConfiguration,
				DestinationProviderIdentifier = destinationProvider.Identifier,
				DestinationProviderArtifactId = destinationProvider.ArtifactId,
				DestinationConfiguration = model.Destination,
				FieldsMap = model.Map,
				Type = model.Type,
				IntegrationPointTypeIdentifier = integrationPointType.Identifier,
				ObjectTypeGuid = objectTypeGuid,
				SecuredConfiguration = model.SecuredConfiguration,
				CreateSavedSearch = destinationConfiguration.CreateSavedSearchForTagging
			};
		}

		protected string GetTransferredObjectObjectTypeGuid(IntegrationPointProviderValidationModel validationModel)
		{
			string objectTypeGuid;
			ImportSettings destinationConfiguration = _serializer.Deserialize<ImportSettings>(validationModel.DestinationConfiguration);
			int destinationWorkspaceArtifactId = destinationConfiguration.CaseArtifactId;

			using (IObjectManager objectManager = _servicesMgr.CreateProxy<IObjectManager>(ExecutionIdentity.System))
			{
				var request = new QueryRequest()
				{
					ObjectType = new ObjectTypeRef() { ArtifactTypeID = (int)ArtifactType.ObjectType },
					Condition = $"(('Artifact Type ID' == {validationModel.ArtifactTypeId}))"
				};

				QueryResult results = objectManager.QueryAsync(destinationWorkspaceArtifactId, request, 0, 1)
					.GetAwaiter().GetResult();
				objectTypeGuid = results.Objects.Single().Guids.Single().ToString();
			}
			
			return objectTypeGuid;
		}


		public abstract ValidationResult Validate(
			IntegrationPointModelBase model,
			SourceProvider sourceProvider,
			DestinationProvider destinationProvider,
			IntegrationPointType integrationPointType,
			Guid objectTypeGuid,
			int userId);
	}
}
