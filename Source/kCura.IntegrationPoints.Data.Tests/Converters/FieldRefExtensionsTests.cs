using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using kCura.IntegrationPoints.Data.Converters;
using kCura.IntegrationPoints.Domain.Models;
using NUnit.Framework;
using Relativity.Services.Field;
using Enumerable = System.Linq.Enumerable;

namespace kCura.IntegrationPoints.Data.Tests.Converters
{
    [TestFixture, Category("Unit")]
    public class FieldRefExtensionsTests
    {
        [Test]
        public void ToArtifactFieldDTO_ShouldConvertValidObject()
        {
            // arrange 
            const string name = "name";
            const int artifactID = 414123;

            var input = new FieldRef(name)
            {
                ArtifactID = artifactID
            };

            // act
            ArtifactFieldDTO result = input.ToArtifactFieldDTO();

            // assert
            result.Name.Should().Be(name);
            result.ArtifactId.Should().Be(artifactID);
        }

        [Test]
        public void ToArtifactFieldDTO_ShouldReturnNullWhenInputIsNull()
        {
            // arrange
            FieldRef input = null;

            // act
            ArtifactFieldDTO result = input.ToArtifactFieldDTO();

            // assert
            result.Should().BeNull("because input was null");
        }

        [Test]
        public void ToArtifactFieldDTOs_ShouldWorksForEmptyList()
        {
            // arrange
            IEnumerable<FieldRef> input = Enumerable.Empty<FieldRef>();

            // act
            IEnumerable<ArtifactFieldDTO> result = input.ToArtifactFieldDTOs();

            // assert
            result.Should().BeEmpty("because empty list was empty");
        }

        [Test]
        public void ToArtifactFieldDTOs_ShouldReturnNullWhenInputIsNull()
        {
            // arrange
            IEnumerable<FieldRef> input = null;

            // act
            IEnumerable<ArtifactFieldDTO> result = input.ToArtifactFieldDTOs();

            // assert
            result.Should().BeNull("because input was null");
        }

        [Test]
        public void ToArtifactFieldDTOs_ShouldConvertValidObjects()
        {
            // arrange
            FieldRef[] inputs =
            {
                new FieldRef("firstFieldName")
                {
                    ArtifactID = 12
                },
                new FieldRef("second name")
                {
                    ArtifactID = 872
                }
            };

            // act
            ArtifactFieldDTO[] results = inputs.ToArtifactFieldDTOs().ToArray();

            // assert
            results.Length.Should().Be(inputs.Length);
            for (int i = 0; i < inputs.Length; i++)
            {
                results[i].Name.Should().Be(inputs[i].Name);
                results[i].ArtifactId.Should().Be(inputs[i].ArtifactID);
            }
        }
    }
}
