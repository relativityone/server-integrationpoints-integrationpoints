using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Relativity.API;
using Relativity.Services.Interfaces.Group;
using Relativity.Services.Objects.DataContracts;
using System;
using System.Linq;
using Relativity.Toggles;

namespace kCura.IntegrationPoints.Core.Validation.Parts
{
    public class NativeCopyLinksValidator : IPermissionValidator
    {
        private const string _COPY_NATIVE_FILES_BY_LINKS_LACK_OF_PERMISSION =
            "You do not have permission to perform this operation because it uses referential links to files. " +
            "You must either log in as a system administrator or change the settings to upload files.";
        private const string _ENABLE_NON_ADMIN_SYNC_LINKS_TOGGLE =
            "Relativity.Sync.Toggles.EnableNonAdminSyncLinksToggle";
        private readonly IAPILog _logger;
        private readonly IHelper _helper;
        private readonly ISerializer _serializer;
        private readonly IManagerFactory _managerFactory;
        private readonly IToggleProvider _toggleProvider;

        public string Key => Constants.IntegrationPoints.Validation.NATIVE_COPY_LINKS_MODE;

        public NativeCopyLinksValidator(IAPILog logger, IHelper helper, ISerializer serializer, IManagerFactory managerFactory, IToggleProvider toggleProvider)
        {
            _logger = logger;
            _helper = helper;
            _serializer = serializer;
            _managerFactory = managerFactory;
            _toggleProvider = toggleProvider;
        }

        public ValidationResult Validate(object value)
        {
            _logger.LogInformation("Validating Native File links copy Restriction");

            var validationResult = new ValidationResult();

            try
            {
                IntegrationPointProviderValidationModel validationModel = CastToValidationModel(value);
                ImportSettings settings = _serializer.Deserialize<ImportSettings>(validationModel.DestinationConfiguration);

                if (settings.ImportNativeFileCopyMode != ImportNativeFileCopyModeEnum.SetFileLinks)
                {
                    return validationResult;
                }

                bool isRestrictReferentialFileLinksOnImport = _managerFactory.CreateInstanceSettingsManager()
                    .RetrieveRestrictReferentialFileLinksOnImport();
                bool executingUserIsAdmin = UserIsAdmin(validationModel.UserId);
                bool nonAdminCanSyncUsingLinks = _toggleProvider.IsEnabledByName(_ENABLE_NON_ADMIN_SYNC_LINKS_TOGGLE);

                _logger.LogInformation("Restrict Referential File Links on Import : {isRestricted}, User is Admin : {isAdmin}, Toggle {toggleName}: {toggleValue}",
                    isRestrictReferentialFileLinksOnImport, executingUserIsAdmin, _ENABLE_NON_ADMIN_SYNC_LINKS_TOGGLE, nonAdminCanSyncUsingLinks );

                if (isRestrictReferentialFileLinksOnImport && !executingUserIsAdmin && !nonAdminCanSyncUsingLinks)
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

        private bool UserIsAdmin(int userId)
        {
            _logger.LogInformation("Check if User {userId} is Admin", userId);
            using (IGroupManager proxy = _helper.GetServicesManager().CreateProxy<IGroupManager>(ExecutionIdentity.System))
            {
                QueryRequest request = BuildAdminGroupsQuery();
                QueryResultSlim result = proxy.QueryGroupsByUserAsync(request, 0, 1, userId)
                    .ConfigureAwait(false).GetAwaiter().GetResult();

                return result.Objects.Any();
            }
        }

        private static QueryRequest BuildAdminGroupsQuery()
        {
            const string adminGroupType = "System Admin";
            var request = new QueryRequest()
            {
                Condition = $"'Group Type' == '{adminGroupType}'",
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
