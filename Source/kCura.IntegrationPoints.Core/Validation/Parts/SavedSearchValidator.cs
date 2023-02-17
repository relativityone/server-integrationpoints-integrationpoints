using System;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Validation.Parts
{
    public class SavedSearchValidator : BasePartsValidator<int>
    {
        private readonly IAPILog _logger;
        private readonly ISavedSearchQueryRepository _savedSearchQueryRepository;

        public SavedSearchValidator(IAPILog logger, ISavedSearchQueryRepository savedSearchQueryRepository)
        {
            _logger = logger.ForContext<SavedSearchValidator>();
            _savedSearchQueryRepository = savedSearchQueryRepository;
        }

        public override ValidationResult Validate(int savedSearchId)
        {
            var result = new ValidationResult();

            SavedSearchDTO savedSearch = RetrieveSavedSearch(savedSearchId);

            if (savedSearch == null)
            {
                // Important Note: If the saved search is null, that means it either doesn't exist or the current user does not have permissions to it.
                // Make sure to never give information the user is not privy to
                // (i.e. if they don't have access to the saved search, don't tell them that it is also not public
                result.Add(ValidationMessages.SavedSearchNoAccess);
            }
            else
            {
                bool savedSearchIsPublic = string.IsNullOrEmpty(savedSearch.Owner);
                if (!savedSearchIsPublic)
                {
                    result.Add(Constants.IntegrationPoints.PermissionErrors.SAVED_SEARCH_NOT_PUBLIC);
                }
            }

            return result;
        }

        private SavedSearchDTO RetrieveSavedSearch(int savedSearchId)
        {
            try
            {
                return _savedSearchQueryRepository.RetrieveSavedSearch(savedSearchId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred retrieving saved search in {validator}", nameof(SavedSearchValidator));
                string message = IntegrationPointsExceptionMessages.CreateErrorMessageRetryOrContactAdministrator("retrieving saved search");
                throw new IntegrationPointsException(message, ex);
            }
        }
    }
}
