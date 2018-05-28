using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts.Interfaces;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts
{
	public class RelativityProviderPermissionValidator : BasePermissionValidator
	{
		private readonly IRelativityProviderValidatorsFactory _validatorsFactory;

		public RelativityProviderPermissionValidator(ISerializer serializer, IServiceContextHelper contextHelper, IRelativityProviderValidatorsFactory validatorsFactory)
			: base(serializer, contextHelper)
		{
			_validatorsFactory = validatorsFactory;
		}

		public override string Key
			=>
			IntegrationPointPermissionValidator.GetProviderValidatorKey(Domain.Constants.RELATIVITY_PROVIDER_GUID,
				Data.Constants.RELATIVITY_SOURCEPROVIDER_GUID.ToString());

		public override ValidationResult Validate(IntegrationPointProviderValidationModel model)
		{
			var result = new ValidationResult();
			result.Add(ValidateSourceWorkspacePermission());
			result.Add(ValidateDestinationWorkspacePermission(model));
			return result;
		}

		private ValidationResult ValidateSourceWorkspacePermission()
		{
			IRelativityProviderSourceWorkspacePermissionValidator sourceWorkspacePermissionValidator = _validatorsFactory.CreateSourceWorkspacePermissionValidator();
			return sourceWorkspacePermissionValidator.Validate(ContextHelper.WorkspaceID);
		}

		private ValidationResult ValidateDestinationWorkspacePermission(IntegrationPointProviderValidationModel model)
		{
			SourceConfiguration sourceConfiguration = Serializer.Deserialize<SourceConfiguration>(model.SourceConfiguration);
			DestinationConfiguration destinationConfiguration = Serializer.Deserialize<DestinationConfiguration>(model.DestinationConfiguration);

			var result = new ValidationResult();

			IRelativityProviderDestinationWorkspaceExistenceValidator destinationWorkspaceExistenceValidator = _validatorsFactory.CreateDestinationWorkspaceExistenceValidator(
				sourceConfiguration.FederatedInstanceArtifactId, model.SecuredConfiguration);
			result.Add(destinationWorkspaceExistenceValidator.Validate(sourceConfiguration.TargetWorkspaceArtifactId));
			if (!result.IsValid)
			{
				return result; // destination workspace doesnt exist
			}

			IRelativityProviderDestinationWorkspacePermissionValidator destinationWorkspacePermissionValidator = _validatorsFactory.CreateDestinationWorkspacePermissionValidator(
				sourceConfiguration.FederatedInstanceArtifactId, model.SecuredConfiguration);
			result.Add(destinationWorkspacePermissionValidator.Validate(sourceConfiguration.TargetWorkspaceArtifactId, destinationConfiguration.ArtifactTypeId));

			return result;
		}
	}
}