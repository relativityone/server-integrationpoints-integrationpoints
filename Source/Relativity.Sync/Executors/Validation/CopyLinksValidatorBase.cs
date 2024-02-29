using System;
using System.Threading;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Sync.Configuration;
using Relativity.Sync.Pipelines;
using Relativity.Sync.Toggles;
using Relativity.Sync.Toggles.Service;

namespace Relativity.Sync.Executors.Validation
{
    internal abstract class CopyLinksValidatorBase : IValidator
    {
        private readonly IInstanceSettings _instanceSettings;
        private readonly IUserContextConfiguration _userContext;
        private readonly ISyncToggles _syncToggles;
        private readonly IUserService _userService;
        private readonly IAPILog _logger;

        protected abstract string ValidatorKind { get; }

        protected CopyLinksValidatorBase(IInstanceSettings instanceSettings, IUserContextConfiguration userContext, ISyncToggles syncToggles, IUserService userService, IAPILog logger)
        {
            _instanceSettings = instanceSettings;
            _userContext = userContext;
            _syncToggles = syncToggles;
            _userService = userService;
            _logger = logger;
        }

        private const string _COPY_NATIVE_FILES_BY_LINKS_LACK_OF_PERMISSION =
            "You do not have permission to run this import because it uses referential links to files. " +
            "You must either log in as a system administrator or change the settings to upload files to run this import.";

        public async Task<ValidationResult> ValidateAsync(IValidationConfiguration configuration, CancellationToken token)
        {
            _logger.LogInformation("Validating {validatorKind} links copy Restriction", ValidatorKind);

            var validationResult = new ValidationResult();

            try
            {
                if (ShouldNotValidateReferentialFileLinksRestriction(configuration))
                {
                    return validationResult;
                }

                bool isRestrictReferentialFileLinksOnImport = await _instanceSettings.GetRestrictReferentialFileLinksOnImportAsync().ConfigureAwait(false);
                bool executingUserIsAdmin = await _userService.ExecutingUserIsAdminAsync(_userContext.ExecutingUserId).ConfigureAwait(false);
                bool nonAdminCanSyncUsingLinks = _syncToggles.IsEnabled<EnableNonAdminSyncLinksToggle>();

                _logger.LogInformation(
                    "Restrict Referential File Links on Import : {isRestricted}, User is Admin : {isAdmin}, EnableNonAdminSyncLinksToggle: {toggleValue}",
                    isRestrictReferentialFileLinksOnImport, executingUserIsAdmin, nonAdminCanSyncUsingLinks);

                if (isRestrictReferentialFileLinksOnImport && !executingUserIsAdmin && !nonAdminCanSyncUsingLinks)
                {
                    validationResult.Add(_COPY_NATIVE_FILES_BY_LINKS_LACK_OF_PERMISSION);
                }
            }
            catch (Exception ex)
            {
                string message = $"Exception occurred during {ValidatorKind} copy by links validation.";
                _logger.LogError(ex, message);
                throw;
            }

            return validationResult;
        }

        public abstract bool ShouldValidate(ISyncPipeline pipeline);

        protected abstract bool ShouldNotValidateReferentialFileLinksRestriction(IValidationConfiguration configuration);
    }
}
