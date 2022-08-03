using System;
using FluentAssertions;
using kCura.IntegrationPoints.Data.Converters;
using kCura.IntegrationPoints.Data.Repositories.DTO;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Tests.Converters
{
    [TestFixture, Category("Unit")]
    public class FieldUpdateRequestDtoExtensionsTests
    {
        [Test]
        public void ToFieldRefValuePair_ShouldReturnNullWhenInputIsNull()
        {
            // arrange
            FieldUpdateRequestDto input = null;

            // act
            FieldRefValuePair result = input.ToFieldRefValuePair();

            // assert
            result.Should().BeNull("because input was null");
        }

        [Test]
        public void ToFieldRefValuePair_ShouldConvertValidObject()
        {
            // arrange
            Guid fieldGuid = Guid.NewGuid();
            var expectedValue = new object();

            var fieldValueDtoMock = new Mock<IFieldValueDto>();
            fieldValueDtoMock
                .Setup(x => x.Value)
                .Returns(expectedValue);

            FieldUpdateRequestDto input = new FieldUpdateRequestDto(
                fieldGuid,
                fieldValueDtoMock.Object);

            // act
            FieldRefValuePair result = input.ToFieldRefValuePair();

            // assert
            result.Field.Guid.Should().Be(fieldGuid);
            result.Value.Should().Be(expectedValue);
        }
    }
}
