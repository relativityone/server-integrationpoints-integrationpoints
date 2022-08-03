using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Relativity.IntegrationPoints.FieldsMapping.FieldClassifiers;

namespace Relativity.IntegrationPoints.FieldsMapping.Tests.FieldsClassifiers
{
    [TestFixture, Category("Unit")]
    public class RipFieldsClassifierTests
    {
        private RipFieldsClassifier _sut;

        private readonly IEnumerable<string> _ripFields = new[]
        {
            "Relativity Source Case",
            "Relativity Source Job",
            "Relativity Destination Case",
            "Job History"
        };

        private readonly IEnumerable<string> _nonRipFields = new[]
        {
            "Relativity Source Case222",
            "Relativity fdagfdSource Job",
            "Relativifdafty Destsaination Case",
            "Job Hgafdsghnhdistory"
        };

        [SetUp]
        public void SetUp()
        {
            _sut = new RipFieldsClassifier();
        }

        [Test]
        public async Task ClassifyAsync_ShouldClassifyRipFieldsAsHideFromUser()
        {
            // Arrange
            ICollection<FieldInfo> fields = CreateFieldInfos(_ripFields).ToList();

            // Act
            List<FieldClassificationResult> classified = (await _sut.ClassifyAsync(fields, 0).ConfigureAwait(false)).ToList();

            // Assert
            CollectionAssert.AreEquivalent(_ripFields, classified.Select(x => x.FieldInfo.Name));
            classified.Should().OnlyContain(x => x.ClassificationLevel == ClassificationLevel.HideFromUser);
        }

        [Test]
        public async Task ClassifyAsync_ShouldNotClassifyOnlyRipFields()
        {
            // Arrange
            ICollection<FieldInfo> fields = CreateFieldInfos(_nonRipFields).ToList();

            // Act
            IEnumerable<FieldClassificationResult> classified = await _sut.ClassifyAsync(fields, 0).ConfigureAwait(false);

            // Assert
            classified.Should().BeEmpty();
        }

        private IEnumerable<FieldInfo> CreateFieldInfos(IEnumerable<string> fieldNames)
        {
            return fieldNames.Select(x => new FieldInfo(fieldIdentifier: x, name: x, type: "Some Type"));
        }
    }
}