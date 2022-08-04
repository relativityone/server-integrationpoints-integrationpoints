using System;
using FluentAssertions;
using kCura.IntegrationPoints.Data.Converters;
using kCura.IntegrationPoints.Domain.Models;
using NUnit.Framework;
using Relativity;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Tests.Converters
{
    [TestFixture, Category("Unit")]
    public class FieldValuePairExtensionsTests
    {
        [Test]
        public void ToArtifactFieldDTO_ShouldReturnNullForNullInput()
        {
            // arrange
            FieldValuePair input = null;

            // act
            ArtifactFieldDTO result = input.ToArtifactFieldDTO();

            // assert
            result.Should().BeNull("because input was null");
        }

        [Test]
        public void ToArtifactFieldDTO_ShouldThrowArgumentExceptionWhenFieldPropertyIsMissing()
        {
            // arrange
            var input = new FieldValuePair
            {
                Value = "value"
            };

            // act
            Action convertAction = () => input.ToArtifactFieldDTO();

            // assert
            convertAction.ShouldThrow<ArgumentException>();
        }

        [Test]
        public void ToArtifactFieldDTO_ShouldConvertValidObject()
        {
            // arrange
            const int fieldArtifactID = 432432;
            const string fieldName = "fielName";
            const string value = "field value";

            var input = new FieldValuePair
            {
                Field = new Field
                {
                    ArtifactID = fieldArtifactID,
                    FieldType = FieldType.MultipleChoice,
                    Name = fieldName
                },
                Value = value
            };

            // act
            ArtifactFieldDTO result = input.ToArtifactFieldDTO();

            // assert
            result.ArtifactId.Should().Be(fieldArtifactID);
            result.Name.Should().Be(fieldName);
            result.FieldType.Should().Be(FieldTypeHelper.FieldType.MultiCode);
            result.Value.Should().Be(value);
        }

    }
}
