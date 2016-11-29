﻿using System;
using System.Linq;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Validation.RelativityProviderValidator
{
	[TestFixture]
	public class SavedSearchValidatorTests
	{
		[Test]
		public void ItShouldValidateSavedSearch()
		{
			// arrange
			var savedSearch = new SavedSearchDTO();

			var savedSearchRepositoryMock = Substitute.For<ISavedSearchRepository>();
			savedSearchRepositoryMock.RetrieveSavedSearch()
				.Returns(savedSearch);

			var validator = new SavedSearchValidator(savedSearchRepositoryMock);

			// act
			var actual = validator.Validate(0);

			// assert
			Assert.IsTrue(actual.IsValid);
			Assert.That(actual.Messages.Count(), Is.EqualTo(0));
		}

		[Test]
		public void ItShouldFailForNotFoundSavedSearch()
		{
			// arrange
			var savedSearch = default(SavedSearchDTO);

			var savedSearchRepositoryMock = Substitute.For<ISavedSearchRepository>();
			savedSearchRepositoryMock.RetrieveSavedSearch()
				.Returns(savedSearch);

			var validator = new SavedSearchValidator(savedSearchRepositoryMock);

			// act
			var actual = validator.Validate(0);

			// assert
			Assert.IsFalse(actual.IsValid);
			Assert.IsTrue(actual.Messages.Contains(RelativityProviderValidationMessages.SAVED_SEARCH_NOT_EXIST));
		}
	}
}