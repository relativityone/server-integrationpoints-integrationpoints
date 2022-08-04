using System;
using FluentAssertions;
using kCura.IntegrationPoints.Data.Repositories.DTO;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Tests.Repositories.DTO
{
    [TestFixture, Category("Unit")]
    public class SingleChoiceReferenceDtoTests
    {
        [Test]
        public void Value_ShouldReturnChoiceRefWithCorrectGuid()
        {
            // arrange
            Guid expectedGuid = Guid.NewGuid();
            var sut = new SingleChoiceReferenceDto(expectedGuid);

            // act
            var choiceRef = sut.Value as ChoiceRef;

            // assert
            choiceRef
                .Should()
                .NotBeNull("because value should be {0}", nameof(ChoiceRef));
            choiceRef.Guid.Should().Be(expectedGuid);
        }
    }
}