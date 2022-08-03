using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Relativity.IntegrationPoints.FieldsMapping.FieldClassifiers;

namespace Relativity.IntegrationPoints.FieldsMapping.Tests.FieldsClassifiers
{
    [TestFixture, Category("Unit")]
    public class SystemFieldsClassifierTests
    {
        private SystemFieldsClassifier _sut;

        private readonly List<string> _systemFields = new List<string>()
        {
            "Is System Artifact",
            "System Created By",
            "System Created On",
            "System Last Modified By",
            "System Last Modified On",
            "Artifact ID"
        };

        private readonly List<string> _nonSystemFields = new List<string>()
        {
            "Some cutom field",
            "Another custom field"
        };

        [SetUp]
        public void SetUp()
        {
            _sut = new SystemFieldsClassifier();
        }

        [Test]
        public async Task ClassifyAsync_ShouldClassifySystemFieldsAsHideFromUser()
        {
            // Arrange
            ICollection<FieldInfo> fields = CreateDocumentFieldInfos(_systemFields).ToList();

            // Act
            List<FieldClassificationResult> classified = (await _sut.ClassifyAsync(fields, 0).ConfigureAwait(false)).ToList();

            // Assert
            CollectionAssert.AreEquivalent(_systemFields, classified.Select(x => x.FieldInfo.Name));
            classified.Should().OnlyContain(x => x.ClassificationLevel == ClassificationLevel.HideFromUser);
        }

        [Test]
        public async Task ClassifyAsync_ShouldNotClassifyNonSystemFields()
        {
            // Arrange
            ICollection<FieldInfo> fields = CreateDocumentFieldInfos(_nonSystemFields).ToList();

            // Act
            IEnumerable<FieldClassificationResult> classified = await _sut.ClassifyAsync(fields, 0).ConfigureAwait(false);

            // Assert
            classified.Should().BeEmpty();
        }

        private IEnumerable<FieldInfo> CreateDocumentFieldInfos(IEnumerable<string> fieldNames)
        {
            return fieldNames.Select(x => new FieldInfo(fieldIdentifier: x, name: x, type: "Some Type"));
        }
    }
}