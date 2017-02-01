using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Validation.Parts
{
	public class SavedSearchValidator : BasePartsValidator<int>
	{
		private readonly ISavedSearchQueryRepository _savedSearchQueryRepository;
		private readonly int _savedSearchId;

		public SavedSearchValidator(ISavedSearchQueryRepository savedSearchQueryRepository, int savedSearchId)
		{
			_savedSearchQueryRepository = savedSearchQueryRepository;
			_savedSearchId = savedSearchId;
		}

		public override ValidationResult Validate(int value)
		{
			var result = new ValidationResult();

			SavedSearchDTO savedSearch = _savedSearchQueryRepository.RetrieveSavedSearch(_savedSearchId);

			if (savedSearch == null)
			{
				// Important Note: If the saved search is null, that means it either doesn't exist or the current user does not have permissions to it.
				// Make sure to never give information the user is not privy to
				// (i.e. if they don't have access to the saved search, don't tell them that it is also not public
				result.Add(Constants.IntegrationPoints.PermissionErrors.SAVED_SEARCH_NO_ACCESS);
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
	}
}