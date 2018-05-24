using System.Linq;
using kCura.IntegrationPoints.Core.Validation.Parts;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Tests.Validation.RelativityProviderValidator
{
	[TestFixture]
	public class SavedSearchValidatorTests
	{
		private const int _SAVED_SEARCH_ID = 1;

		[Test]
		public void ItShouldValidateSavedSearch()
		{
			// arrange
			var savedSearch = new SavedSearchDTO();
			
			var savedSearchRepositoryMock = Substitute.For<ISavedSearchQueryRepository>();
			savedSearchRepositoryMock.RetrieveSavedSearch(_SAVED_SEARCH_ID)
				.Returns(savedSearch);
			IAPILog logger = Substitute.For<IAPILog>();
			var validator = new SavedSearchValidator(logger, savedSearchRepositoryMock, _SAVED_SEARCH_ID);

			// act
			var actual = validator.Validate(0);

			// assert
			Assert.IsTrue(actual.IsValid);
			Assert.That(actual.MessageTexts.Count(), Is.EqualTo(0));
		}

		[Test]
		public void ItShouldFailForNotFoundSavedSearch()
		{
			// arrange
			var savedSearch = default(SavedSearchDTO);

			var savedSearchRepositoryMock = Substitute.For<ISavedSearchQueryRepository>();
			savedSearchRepositoryMock.RetrieveSavedSearch(_SAVED_SEARCH_ID)
				.Returns(savedSearch);

			IAPILog logger = Substitute.For<IAPILog>();
			var validator = new SavedSearchValidator(logger, savedSearchRepositoryMock, _SAVED_SEARCH_ID);

			// act
			var actual = validator.Validate(0);

			// assert
			Assert.IsFalse(actual.IsValid);
			Assert.IsTrue(actual.MessageTexts.Contains(Constants.IntegrationPoints.PermissionErrors.SAVED_SEARCH_NO_ACCESS));
		}

		[Test]
		public void ItShouldFailForNotPublicSavedSearch()
		{
			// arrange
			var savedSearch = new SavedSearchDTO() { Owner = "owner" };

			var savedSearchRepositoryMock = Substitute.For<ISavedSearchQueryRepository>();
			savedSearchRepositoryMock.RetrieveSavedSearch(_SAVED_SEARCH_ID)
				.Returns(savedSearch);

			IAPILog logger = Substitute.For<IAPILog>();
			var validator = new SavedSearchValidator(logger, savedSearchRepositoryMock, _SAVED_SEARCH_ID);

			// act
			var actual = validator.Validate(0);

			// assert
			Assert.IsFalse(actual.IsValid);
			Assert.IsTrue(actual.MessageTexts.Contains(Constants.IntegrationPoints.PermissionErrors.SAVED_SEARCH_NOT_PUBLIC));
		}
	}
}