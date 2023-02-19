using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.IntegrationPoints.FieldsMapping.FieldClassifiers;
using Relativity.IntegrationPoints.FieldsMapping.Helpers;
using Relativity.Services.Objects.DataContracts;
using Field = Relativity.Services.Objects.DataContracts.Field;

namespace Relativity.IntegrationPoints.FieldsMapping.Tests
{
    [TestFixture, Category("Unit")]
    public class FieldsClassifierRunnerTests
    {
        private const int _ARTIFACT_TYPE_ID = 111;
        private Mock<IFieldsRepository> _fieldsRepositoryFake;

        [SetUp]
        public void SetUp()
        {
            _fieldsRepositoryFake = new Mock<IFieldsRepository>();
        }

        [Test]
        public async Task GetFilteredFieldsAsync_ShouldFilterOutFieldsNotVisibleToUser()
        {
            // Arrange
            const string autoMappedFieldName = "Auto mapped field";
            const string autoMappedFieldType = "Fixed length text";

            const string excludedFromAutoMapFieldName = "Excluded from auto-map field";
            const string notVisibleToUserFieldName = "Field not visible to user";

            var workspaceFields = new List<FieldInfo>()
            {
                CreateField(autoMappedFieldName, type: autoMappedFieldType, isIdentifier: true),
                CreateField(excludedFromAutoMapFieldName),
                CreateField(notVisibleToUserFieldName)
            };

            SetupWorkspaceFields(workspaceFields);

            var classificationResult = new List<FieldClassificationResult>()
            {
                new FieldClassificationResult(workspaceFields[0]) { ClassificationLevel = ClassificationLevel.AutoMap },
                new FieldClassificationResult(workspaceFields[1]) { ClassificationLevel = ClassificationLevel.ShowToUser },
                new FieldClassificationResult(workspaceFields[2]) { ClassificationLevel = ClassificationLevel.HideFromUser }
            };

            var classifierFake = new Mock<IFieldsClassifier>();
            classifierFake
                .Setup(x => x.ClassifyAsync(It.IsAny<ICollection<FieldInfo>>(), It.IsAny<int>()))
                .ReturnsAsync(classificationResult);
            var fieldsClassifiers = new List<IFieldsClassifier>()
            {
                classifierFake.Object
            };

            FieldsClassifierRunner sut = new FieldsClassifierRunner(_fieldsRepositoryFake.Object, fieldsClassifiers);

            // Act
            IList<FieldClassificationResult> filteredFields = await sut.GetFilteredFieldsAsync(It.IsAny<int>(), _ARTIFACT_TYPE_ID).ConfigureAwait(false);

            // Assert
            filteredFields.Any(x => x.FieldInfo.Name == autoMappedFieldName && x.FieldInfo.Type == autoMappedFieldType && x.FieldInfo.IsIdentifier).Should().BeTrue();
            filteredFields.Any(x => x.FieldInfo.Name == excludedFromAutoMapFieldName).Should().BeTrue();
            filteredFields.Any(x => x.FieldInfo.Name == notVisibleToUserFieldName).Should().BeFalse();
        }

        [Test]
        public async Task GetFilteredFieldsAsync_ShouldReturnAllFields_WhenNoClassifiers()
        {
            // Arrange
            const int count = 3;
            SetupWorkspaceFields(Enumerable.Range(1, count).Select(x => CreateField(x.ToString())).ToList());

            FieldsClassifierRunner sut = new FieldsClassifierRunner(_fieldsRepositoryFake.Object);

            // Act
            IList<FieldClassificationResult> filteredFields = await sut.GetFilteredFieldsAsync(It.IsAny<int>(), _ARTIFACT_TYPE_ID).ConfigureAwait(false);

            // Assert
            filteredFields.Count.Should().Be(count);
            filteredFields.All(x => x.ClassificationLevel == ClassificationLevel.AutoMap).Should().BeTrue();
        }

        [Test]
        public void GetFilteredFieldsAsync_ShouldRetryOnce()
        {
            // Arrange
            SetupWorkspaceFields(Enumerable.Range(1, 2).Select(x => CreateField(x.ToString())).ToList());

            Mock<IFieldsClassifier> classifierMock = new Mock<IFieldsClassifier>();
            classifierMock.Setup(x => x.ClassifyAsync(It.IsAny<ICollection<FieldInfo>>(), It.IsAny<int>())).Throws<InvalidOperationException>();

            var fieldsClassifiers = new List<IFieldsClassifier>()
            {
                classifierMock.Object
            };

            FieldsClassifierRunner sut = new FieldsClassifierRunner(_fieldsRepositoryFake.Object, fieldsClassifiers);

            // Act
            Func<Task> action = () => sut.GetFilteredFieldsAsync(0, _ARTIFACT_TYPE_ID);

            // Assert
            action.ShouldThrow<InvalidOperationException>();
            classifierMock.Verify(x => x.ClassifyAsync(It.IsAny<ICollection<FieldInfo>>(), It.IsAny<int>()), Times.Exactly(2));
        }

        [Test]
        public async Task GetFilteredFieldsAsync_ShouldUpdateOnlyClassificationLevelAndReasonFromClassifier()
        {
            // Arrange
            const int count = 3;
            SetupWorkspaceFields(Enumerable.Range(1, count).Select(x => CreateField(x.ToString(), x, "Some type")).ToList());

            var classificationResult = new List<FieldClassificationResult>()
            {
                new FieldClassificationResult(CreateField(name: "1", type: "Changed type", isIdentifier: true))
                {
                    ClassificationLevel = ClassificationLevel.AutoMap,
                    ClassificationReason = "Reason"
                },
                new FieldClassificationResult(CreateField(name: "2", type: "Changed type", isIdentifier: true))
                {
                    ClassificationLevel = ClassificationLevel.ShowToUser,
                    ClassificationReason = "Reason"
                },
                new FieldClassificationResult(CreateField(name: "3", type: "Changed type", isIdentifier: true))
                {
                    ClassificationLevel = ClassificationLevel.ShowToUser,
                    ClassificationReason = "Reason",
                }
            };

            var classifierFake = new Mock<IFieldsClassifier>();
            classifierFake
                .Setup(x => x.ClassifyAsync(It.IsAny<ICollection<FieldInfo>>(), It.IsAny<int>()))
                .ReturnsAsync(classificationResult);

            var fieldsClassifiers = new List<IFieldsClassifier>()
            {
                classifierFake.Object
            };

            FieldsClassifierRunner sut = new FieldsClassifierRunner(_fieldsRepositoryFake.Object, fieldsClassifiers);

            // Act
            IList<FieldClassificationResult> filteredFields = await sut.GetFilteredFieldsAsync(It.IsAny<int>(), _ARTIFACT_TYPE_ID).ConfigureAwait(false);

            // Assert
            filteredFields.Count.Should().Be(count);

            filteredFields.Select(x => x.FieldInfo.IsIdentifier).Any(x => x).Should().BeFalse();
            filteredFields.Select(x => x.FieldInfo.IsRequired).Any(x => x).Should().BeFalse();
            filteredFields.Select(x => x.FieldInfo.Type).All(x => x == "Some type").Should().BeTrue();

            filteredFields[0].FieldInfo.FieldIdentifier.Should().Be("1");
            filteredFields[1].FieldInfo.FieldIdentifier.Should().Be("2");
            filteredFields[2].FieldInfo.FieldIdentifier.Should().Be("3");

            filteredFields[0].ClassificationReason.Should().Be(null);
            filteredFields[1].ClassificationReason.Should().Be("Reason");
            filteredFields[2].ClassificationReason.Should().Be("Reason");
        }

        [Test]
        public async Task GetFilteredFieldsAsync_ShouldSortByName()
        {
            // Arrange
            List<FieldInfo> unsortedFields = new List<FieldInfo>()
            {
                CreateField("Field C"),
                CreateField("Field A"),
                CreateField("Field B")
            };

            List<FieldInfo> sortedFields = unsortedFields.OrderBy(x => x.Name).ToList();

            SetupWorkspaceFields(unsortedFields);

            Mock<IFieldsClassifier> classifier = new Mock<IFieldsClassifier>();
            IEnumerable<FieldClassificationResult> classificationResult = unsortedFields.Select(x => new FieldClassificationResult(x));

            classifier.Setup(x => x.ClassifyAsync(It.IsAny<ICollection<FieldInfo>>(), It.IsAny<int>())).ReturnsAsync(classificationResult);

            var fieldsClassifiers = new List<IFieldsClassifier>()
            {
                classifier.Object
            };

            FieldsClassifierRunner sut = new FieldsClassifierRunner(_fieldsRepositoryFake.Object, fieldsClassifiers);

            // Act
            IList<FieldClassificationResult> filteredFields = await sut.GetFilteredFieldsAsync(0, _ARTIFACT_TYPE_ID).ConfigureAwait(false);

            // Assert
            filteredFields.Select(x => x.FieldInfo.Name).ShouldAllBeEquivalentTo(sortedFields.Select(x => x.Name));
        }

        [Test]
        public async Task ClassifyAsync_ShouldCallClassifier()
        {
            // Arrange
            List<FieldInfo> fields = new List<FieldInfo>()
            {
                CreateField("Field C"),
                CreateField("Field A"),
                CreateField("Field B")
            };

            List<FieldInfo> sortedFields = fields.OrderBy(x => x.Name).ToList();
            string[] artifactIDs = fields.Select(x => x.FieldIdentifier).ToArray();

            _fieldsRepositoryFake
                .Setup(x => x.GetFieldsByArtifactsIdAsync(artifactIDs, 0))
                .ReturnsAsync(fields);

            Mock<IFieldsClassifier> classifier = new Mock<IFieldsClassifier>();
            IEnumerable<FieldClassificationResult> fieldClassificationResults = fields.Select(x => new FieldClassificationResult(x)).ToArray();

            classifier.Setup(x => x.ClassifyAsync(It.IsAny<ICollection<FieldInfo>>(), It.IsAny<int>())).ReturnsAsync(fieldClassificationResults);

            var fieldsClassifiers = new List<IFieldsClassifier>()
            {
                classifier.Object
            };

            FieldsClassifierRunner sut = new FieldsClassifierRunner(_fieldsRepositoryFake.Object, fieldsClassifiers);

            // Act
            IEnumerable<FieldClassificationResult> filteredFields = await sut.ClassifyFieldsAsync(artifactIDs, 0).ConfigureAwait(false);

            // Assert
            classifier.Verify(x => x.ClassifyAsync(fields, 0), Times.Once);
            filteredFields.ShouldAllBeEquivalentTo(fieldClassificationResults);
        }

        private void SetupWorkspaceFields(IEnumerable<FieldInfo> fields)
        {
            _fieldsRepositoryFake
                .Setup(x => x.GetAllFieldsAsync(It.IsAny<int>(), _ARTIFACT_TYPE_ID))
                .ReturnsAsync(fields);
        }

        private FieldInfo CreateField(string name, int artifactID = 0, string type = "", bool isIdentifier = false)
        {
            var fieldObject = new RelativityObject()
            {
                ArtifactID = artifactID,
                Name = name,
                FieldValues = new List<FieldValuePair>()
                {
                    new FieldValuePair()
                    {
                        Field = new Field()
                        {
                            Name = "Is Identifier"
                        },
                        Value = isIdentifier
                    },
                    new FieldValuePair()
                    {
                        Field = new Field()
                        {
                            Name = "Field Type"
                        },
                        Value = type
                    }
                }
            };

            return FieldConvert.ToDocumentFieldInfo(fieldObject);
        }
    }
}
