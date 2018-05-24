using System;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts
{
	public class RelativityProviderPermissionValidator : BasePermissionValidator
	{
		private readonly IAPILog _logger;
		private readonly IContextContainerFactory _contextContainerFactory;
		private readonly IManagerFactory _managerFactory;
		private readonly IHelperFactory _helperFactory;
		private readonly IHelper _helper;

		public RelativityProviderPermissionValidator(ISerializer serializer, IServiceContextHelper contextHelper,
			IHelper helper, IHelperFactory helperFactory, IContextContainerFactory contextContainerFactory, IManagerFactory managerFactory)
			: base(serializer, contextHelper)
		{
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<RelativityProviderPermissionValidator>();
			_helper = helper;
			_helperFactory = helperFactory;
			_contextContainerFactory = contextContainerFactory;
			_managerFactory = managerFactory;
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
			var result = new ValidationResult();
			IPermissionManager sourceWorkspacePermissionManager = CreatePermissionManager(_helper);

			if (!sourceWorkspacePermissionManager.UserCanExport(ContextHelper.WorkspaceID))
			{
				result.Add(Constants.IntegrationPoints.PermissionErrors.SOURCE_WORKSPACE_NO_EXPORT);
			}

			if (!sourceWorkspacePermissionManager.UserCanEditDocuments(ContextHelper.WorkspaceID))
			{
				result.Add(Constants.IntegrationPoints.NO_PERMISSION_TO_EDIT_DOCUMENTS);
			}
			return result;
		}

		private ValidationResult ValidateDestinationWorkspacePermission(IntegrationPointProviderValidationModel model)
		{
			var result = new ValidationResult();

			SourceConfiguration sourceConfiguration = Serializer.Deserialize<SourceConfiguration>(model.SourceConfiguration);
			IPermissionManager destinationWorkspacePermissionManager = CreateDestinationPermissionManager(model, sourceConfiguration);
			IWorkspaceManager destinationWorkspaceManager = CreateDestinationWorkspaceManager(model, sourceConfiguration);

			if (!destinationWorkspaceManager.WorkspaceExists(sourceConfiguration.TargetWorkspaceArtifactId))
			{
                var message = new ValidationMessage(
                    Constants.IntegrationPoints.ValidationErrorCodes.DESTINATION_WORKSPACE_NOT_AVAILABLE,
                    Constants.IntegrationPoints.ValidationErrors.DESTINATION_WORKSPACE_NOT_AVAILABLE
                );
				_logger.LogError(message.ToString());
                result.Add(message);
				return result; // destination workspace does not exist
			}

			if (!destinationWorkspacePermissionManager.UserHasPermissionToAccessWorkspace(sourceConfiguration.TargetWorkspaceArtifactId))
			{
                var message = new ValidationMessage(
                    Constants.IntegrationPoints.PermissionErrorCodes.DESTINATION_WORKSPACE_NO_ACCESS, 
                    Constants.IntegrationPoints.PermissionErrors.DESTINATION_WORKSPACE_NO_ACCESS
                );
				_logger.LogError(message.ToString());
				result.Add(message);
				return result; // it does not make sense to validate other destination workspace permissions
			}

			if (!destinationWorkspacePermissionManager.UserCanImport(sourceConfiguration.TargetWorkspaceArtifactId))
			{
				result.Add(Constants.IntegrationPoints.PermissionErrors.DESTINATION_WORKSPACE_NO_IMPORT);
			}

			DestinationConfiguration destinationConfiguration = Serializer.Deserialize<DestinationConfiguration>(model.DestinationConfiguration);
			if (!destinationWorkspacePermissionManager.UserHasArtifactTypePermissions(sourceConfiguration.TargetWorkspaceArtifactId, destinationConfiguration.ArtifactTypeId,
				new[] { ArtifactPermission.View, ArtifactPermission.Edit, ArtifactPermission.Create }))
			{
				result.Add(Constants.IntegrationPoints.PermissionErrors.MISSING_DESTINATION_RDO_PERMISSIONS);
			}

			return result;
		}

		private IPermissionManager CreateDestinationPermissionManager(IntegrationPointProviderValidationModel model, SourceConfiguration sourceConfiguration)
		{
			IHelper targetHelper = CreateTargetHelper(model, sourceConfiguration);
			IPermissionManager destinationWorkspacePermissionManager = CreatePermissionManager(targetHelper);
			return destinationWorkspacePermissionManager;
		}

		private IWorkspaceManager CreateDestinationWorkspaceManager(IntegrationPointProviderValidationModel model, SourceConfiguration sourceConfiguration)
		{
			IHelper targetHelper = CreateTargetHelper(model, sourceConfiguration);
			IWorkspaceManager workspaceManager = CreateWorkspaceManager(targetHelper);
			return workspaceManager;
		}

		private IHelper CreateTargetHelper(IntegrationPointProviderValidationModel model,
			SourceConfiguration sourceConfiguration)
		{
			try
			{
				return _helperFactory.CreateTargetHelper(_helper, sourceConfiguration.FederatedInstanceArtifactId,
					model.SecuredConfiguration);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occurred creating target helper. FederatedInstance: {instanceArtifactId}. Model: {modelArtifactId}", sourceConfiguration.FederatedInstanceArtifactId, model.ArtifactId);
				throw new IntegrationPointsException(IntegrationPointsExceptionMessages.ERROR_OCCURED_CONTACT_ADMINISTRATOR, ex)
				{
					ExceptionSource = IntegrationPointsExceptionSource.VALIDATION,
					ShouldAddToErrorsTab = false
				};
			}
		}

		private IPermissionManager CreatePermissionManager(IHelper helper)
		{
			try
			{
				return _managerFactory.CreatePermissionManager(_contextContainerFactory.CreateContextContainer(helper));
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occurred creating permission manager.");
				throw new IntegrationPointsException(IntegrationPointsExceptionMessages.ERROR_OCCURED_CONTACT_ADMINISTRATOR, ex)
				{
					ExceptionSource = IntegrationPointsExceptionSource.VALIDATION,
					ShouldAddToErrorsTab = false
				};
			}
		}

		private IWorkspaceManager CreateWorkspaceManager(IHelper helper)
		{
			try
			{
				return _managerFactory.CreateWorkspaceManager(_contextContainerFactory.CreateContextContainer(helper));
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occurred creating workspace manager.");
				throw new IntegrationPointsException(IntegrationPointsExceptionMessages.ERROR_OCCURED_CONTACT_ADMINISTRATOR, ex)
				{
					ExceptionSource = IntegrationPointsExceptionSource.VALIDATION,
					ShouldAddToErrorsTab = false
				};
			}
		}
	}
}