using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.IntegrationPoints.FieldsMapping.FieldClassifiers;
using Relativity.IntegrationPoints.FieldsMapping.ImportApi;

namespace Relativity.IntegrationPoints.FieldsMapping.Tests.FieldsClassifiers
{
    [TestFixture, Category("Unit")]
    public class NotSupportedByIAPIFieldsClassifierTests
    {
        private const int _ARTIFACT_TYPE_ID = 123;
        private Mock<IImportApiFacade> _importApiFacadeFake;
        private NotSupportedByIAPIFieldsClassifier _sut;

        [SetUp]
        public void SetUp()
        {
            _importApiFacadeFake = new Mock<IImportApiFacade>();
            _sut = new NotSupportedByIAPIFieldsClassifier(_importApiFacadeFake.Object, _ARTIFACT_TYPE_ID);
        }

        [Test]
        public async Task ClassifyAsync_ShouldProperlyClassifyFieldsNotSupportedByIAPI()
        {
            // Arrange
            List<FieldInfo> allFields = Enumerable.Range(1, 2).Select(x => new FieldInfo(
                fieldIdentifier: x.ToString(),
                name: $"Field {x}",
                type: "Fixed-Length Text(250)")).ToList();

            Dictionary<int, string> iapiFields = allFields.Where(x => x.FieldIdentifier == "1").ToDictionary(x => int.Parse(x.FieldIdentifier), x => x.Name);
            _importApiFacadeFake.Setup(x => x.GetWorkspaceFieldsNames(It.IsAny<int>(), _ARTIFACT_TYPE_ID)).Returns(iapiFields);

            // Act
            List<FieldClassificationResult> fields = (await _sut.ClassifyAsync(allFields, 0).ConfigureAwait(false)).ToList();

            // Assert
            fields.Count().Should().Be(1);
            FieldClassificationResult fieldClassificationResult = fields.First();
            fieldClassificationResult.FieldInfo.FieldIdentifier.Should().Be("2");
            fieldClassificationResult.ClassificationLevel.Should().Be(ClassificationLevel.HideFromUser);
            fieldClassificationResult.ClassificationReason.Should().Be("Field not supported by IAPI.");
        }

        [Test]
        public void ClassifyAsync_ShouldRethrowExceptionWhenGettingFieldsFromIapiFails()
        {
            // Arrange
            _importApiFacadeFake.Setup(x => x.GetWorkspaceFieldsNames(It.IsAny<int>(), It.IsAny<int>())).Throws<InvalidOperationException>();

            // Act
            Func<Task> action = () => _sut.ClassifyAsync(Mock.Of<ICollection<FieldInfo>>(), 0);

            // Assert
            action.ShouldThrow<InvalidOperationException>();
        }
    }
}
