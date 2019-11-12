using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Core.Validation.Parts
{
	public class NativeCopyLinksValidator : IValidator
	{
		private const string _COPY_NATIVE_FILES_BY_LINKS_LACK_OF_PERMISSION =
			"You do not have permission to perform this export because it uses referential links to files. " +
			"You must either log in as a system administrator or change the settings to upload files.";

		private readonly IAPILog _logger;
		private readonly ISerializer _serializer;
		private readonly IRelativityObjectManagerFactory _relativityObjectManagerFactory;
		private readonly IManagerFactory _managerFactory;

		public string Key => Constants.IntegrationPointProfiles.Validation.NATIVE_COPY_LINKS_MODE;

		public NativeCopyLinksValidator(IAPILog logger, ISerializer serializer, IRelativityObjectManagerFactory relativityObjectManagerFactory, IManagerFactory managerFactory)
		{
			_logger = logger;
			_serializer = serializer;
			_managerFactory = managerFactory;
			_relativityObjectManagerFactory = relativityObjectManagerFactory;
		}

		public ValidationResult Validate(object value)
		{
			_logger.LogVerbose("Validating Native File links copy Restriction");

			var validationResult = new ValidationResult();

			IntegrationPointProviderValidationModel validationModel = CastToValidationModel(value);

			try
			{
				ImportSettings settings = _serializer.Deserialize<ImportSettings>(validationModel.DestinationConfiguration);

				if (settings.ImportNativeFileCopyMode != ImportNativeFileCopyModeEnum.SetFileLinks)
				{
					return validationResult;
				}

				IInstanceSettingsManager instanceSettings = _managerFactory.CreateInstanceSettingsManager();
				bool isRestrictReferentialFileLinksOnImport = instanceSettings.RetrieveRestrictReferentialFileLinksOnImport();
				bool executingUserIsAdmin = UserIsAdmin(settings.CaseArtifactId, validationModel.UserId);
				if (isRestrictReferentialFileLinksOnImport && !executingUserIsAdmin)
				{
					validationResult.Add(_COPY_NATIVE_FILES_BY_LINKS_LACK_OF_PERMISSION);
				}
			}
			catch (Exception ex)
			{
				const string message = "Exception occurred during native file copy by links validation.";
				_logger.LogError(ex, message);
				validationResult.Add(message);
			}

			return validationResult;
		}

		private bool UserIsAdmin(int workspaceId, int userId)
		{
			QueryRequest request = BuildAdminGroupsQuery();
			IRelativityObjectManager objectManager = _relativityObjectManagerFactory.CreateRelativityObjectManager(workspaceId);
			RelativityObject adminGroup = objectManager.Query(request).Single();

			IPermissionManager permissionManager = _managerFactory.CreatePermissionManager();
			return permissionManager.UserBelongsToGroup(workspaceId, userId, adminGroup.ArtifactID);
		}

		private static QueryRequest BuildAdminGroupsQuery()
		{
			const string adminGroupType = "System Admin";
			const int groupArtifactTypeId = 3;
			var request = new QueryRequest()
			{
				ObjectType = new ObjectTypeRef()
				{
					ArtifactTypeID = groupArtifactTypeId
				},
				Condition = $"(('Group Type' == '{adminGroupType}'))",
			};

			return request;
		}

		private IntegrationPointProviderValidationModel CastToValidationModel(object value)
		{
			var result = value as IntegrationPointProviderValidationModel;
			if (result != null)
			{
				return result;
			}

			_logger.LogError("Converstion to {validationModel} failed. Actual type: {type}", nameof(IntegrationPointProviderValidationModel), value?.GetType());
			throw new IntegrationPointsException(IntegrationPointsExceptionMessages.ERROR_OCCURED_CONTACT_ADMINISTRATOR)
			{
				ExceptionSource = IntegrationPointsExceptionSource.VALIDATION,
				ShouldAddToErrorsTab = false
			};
		}
	}
}
