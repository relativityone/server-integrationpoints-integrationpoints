using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Relativity;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Core.Validation
{
	public class IntegrationPointProviderValidator : BaseIntegrationPointValidator<IValidator>, IIntegrationPointProviderValidator
	{
		private readonly IRelativityObjectManagerFactory _relativityObjectManagerFactory;

		public IntegrationPointProviderValidator(IEnumerable<IValidator> validators, IIntegrationPointSerializer serializer, IRelativityObjectManagerFactory relativityObjectManagerFactory)
			: base(validators, serializer)
		{
			_relativityObjectManagerFactory = relativityObjectManagerFactory;
		}

		public override ValidationResult Validate(
			IntegrationPointModelBase model,
			SourceProvider sourceProvider,
			DestinationProvider destinationProvider,
			IntegrationPointType integrationPointType,
			Guid objectTypeGuid,
			int userId)
		{
			var result = new ValidationResult();

			if (model.Scheduler.EnableScheduler)
			{
				foreach (IValidator validator in _validatorsMap[Constants.IntegrationPointProfiles.Validation.SCHEDULE])
				{
					result.Add(validator.Validate(model.Scheduler));
				}
			}

			foreach (IValidator validator in _validatorsMap[Constants.IntegrationPointProfiles.Validation.EMAIL])
			{
				result.Add(validator.Validate(model.NotificationEmails));
			}

			foreach (IValidator validator in _validatorsMap[Constants.IntegrationPointProfiles.Validation.NAME])
			{
				result.Add(validator.Validate(model.Name));
			}

			IntegrationPointProviderValidationModel validationModel = CreateValidationModel(model, sourceProvider, destinationProvider, integrationPointType, objectTypeGuid, userId);

			foreach (IValidator validator in _validatorsMap[Constants.IntegrationPointProfiles.Validation.INTEGRATION_POINT_TYPE])
			{
				result.Add(validator.Validate(validationModel));
			}

			foreach (IValidator validator in _validatorsMap[GetProviderValidatorKey(sourceProvider.Identifier, destinationProvider.Identifier)])
			{
				result.Add(validator.Validate(validationModel));
			}
			
			foreach (IValidator validator in _validatorsMap[GetTransferredObjectObjectTypeGuid(validationModel).ToString()])
            {
            	result.Add(validator.Validate(validationModel));
            }

			return result;
		}

		protected Guid GetTransferredObjectObjectTypeGuid(IntegrationPointProviderValidationModel validationModel)
		{
			ImportSettings destinationConfiguration = _serializer.Deserialize<ImportSettings>(validationModel.DestinationConfiguration);
			int destinationWorkspaceArtifactId = destinationConfiguration.CaseArtifactId;
			IRelativityObjectManager relativityObjectManager =
				_relativityObjectManagerFactory.CreateRelativityObjectManager(destinationWorkspaceArtifactId);
				
			var request = new QueryRequest()
			{
				ObjectType = new ObjectTypeRef() { ArtifactTypeID = (int)ArtifactType.ObjectType },
				Condition = $"(('Artifact Type ID' == {validationModel.ArtifactTypeId}))"
			};

			var results = relativityObjectManager.QueryAsync( request, 0, 1, false, ExecutionIdentity.System)
				.GetAwaiter().GetResult();
			if (results.TotalCount == 0)
			{
				return Guid.Empty;
			}
			return results.Items.Single().Guids.FirstOrDefault();

		}
	}
}