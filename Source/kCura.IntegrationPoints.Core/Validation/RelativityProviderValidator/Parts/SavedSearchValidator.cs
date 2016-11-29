using System;
using System.Linq;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts
{
	public class SavedSearchValidator : BasePartsValidator<int>
	{
		private readonly ISavedSearchRepository _savedSearchRepository;

		public SavedSearchValidator(ISavedSearchRepository savedSearchRepository)
		{
			_savedSearchRepository = savedSearchRepository;
		}

		public override ValidationResult Validate(int value)
		{
			var result = new ValidationResult();

			SavedSearchDTO savedSearch = _savedSearchRepository.RetrieveSavedSearch();

			if (savedSearch == null)
			{
				result.Add(RelativityProviderValidationMessages.SAVED_SEARCH_NOT_EXIST);
			}

			return result;
		}
	}
}