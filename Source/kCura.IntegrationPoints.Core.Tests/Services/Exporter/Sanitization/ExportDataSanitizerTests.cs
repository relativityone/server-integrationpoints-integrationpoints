using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Core.Services.Exporter.Sanitization;
using kCura.IntegrationPoints.Domain.Exceptions;
using Moq;
using NUnit.Framework;
using Relativity;

namespace kCura.IntegrationPoints.Core.Tests.Services.Exporter.Sanitization
{
    [TestFixture, Category("Unit")]
    internal sealed class ExportDataSanitizerTests
    {
        [Test]
        public void ItShouldThrowExceptionWhenGivenMultipleSanitizersWithSameDataType()
        {
            // Arrange
            var sanitizer1 = new Mock<IExportFieldSanitizer>();
            sanitizer1.SetupGet(x => x.SupportedType).Returns(FieldTypeHelper.FieldType.Currency);
            var sanitizer2 = new Mock<IExportFieldSanitizer>();
            sanitizer2.SetupGet(x => x.SupportedType).Returns(FieldTypeHelper.FieldType.Date);
            var sanitizer3 = new Mock<IExportFieldSanitizer>();
            sanitizer3.SetupGet(x => x.SupportedType).Returns(FieldTypeHelper.FieldType.Currency);
            IList<IExportFieldSanitizer> sanitizers = new[] {sanitizer1.Object, sanitizer2.Object, sanitizer3.Object};

            // Act
            Action action = () => new ExportDataSanitizer(sanitizers);

            // Assert
            action.ShouldThrow<IntegrationPointsException>()
                .Which.Message.Should().Contain("sanitizers"); // To ensure that old Sync (and not an underlying type) is throwing this exception.
        }

        [TestCaseSource(nameof(CorrectlyCheckForDataTypesToSanitizeTestCases))]
        public void ItShouldCorrectlyCheckForDataTypesToSanitize(IList<IExportFieldSanitizer> sanitizers, bool expectedResult)
        {
            // Arrange
            var sut = new ExportDataSanitizer(sanitizers);

            // Act
            bool result = sut.ShouldSanitize(FieldTypeHelper.FieldType.Currency);

            // Assert
            result.Should().Be(expectedResult);
        }

        [TestCaseSource(nameof(ThrowExceptionForUnregisteredDataTypeTestCases))]
        public void ItShouldThrowExceptionForUnregisteredDataType(IList<IExportFieldSanitizer> sanitizers)
        {
            // Arrange
            var sut = new ExportDataSanitizer(sanitizers);

            // Act
            FieldTypeHelper.FieldType fieldType = FieldTypeHelper.FieldType.Currency;
            Func<Task> action = async () =>
                await sut.SanitizeAsync(0, "foo", "bar", "src", fieldType, "test")
                    .ConfigureAwait(false);

            // Assert
            action.ShouldThrow<InvalidOperationException>();
        }

        [Test]
        public async Task ItShouldInvokeSanitizerForCorrectDataType()
        {
            // Arrange
            Guid matchingResult = Guid.NewGuid();
            var matchingSanitizer = new Mock<IExportFieldSanitizer>();
            matchingSanitizer.Setup(x => x.SupportedType).Returns(FieldTypeHelper.FieldType.Currency);
            matchingSanitizer
                .Setup(x => x.SanitizeAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(matchingResult);

            Guid nonMatchingResult = Guid.NewGuid();
            var nonMatchingSanitizer = new Mock<IExportFieldSanitizer>();
            nonMatchingSanitizer.Setup(x => x.SupportedType).Returns(FieldTypeHelper.FieldType.Date);
            nonMatchingSanitizer
                .Setup(x => x.SanitizeAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(nonMatchingResult);

            IList<IExportFieldSanitizer> sanitizers = new[] {matchingSanitizer.Object, nonMatchingSanitizer.Object};
            var sut = new ExportDataSanitizer(sanitizers);

            // Act
            FieldTypeHelper.FieldType fieldType = FieldTypeHelper.FieldType.Currency;
            object result =
                await sut.SanitizeAsync(0, "foo", "bar", "src", fieldType, "test")
                    .ConfigureAwait(false);

            // Assert
            result.Should().Be(matchingResult);
        }

        [Test]
        public void ItShouldPassThroughExceptionFromSanitizer()
        {
            // Arrange
            var sanitizer = new Mock<IExportFieldSanitizer>();
            sanitizer.Setup(x => x.SupportedType).Returns(FieldTypeHelper.FieldType.Currency);
            sanitizer
                .Setup(x => x.SanitizeAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()))
                .Throws<IntegrationPointsException>();

            IList<IExportFieldSanitizer> sanitizers = new[] { sanitizer.Object };
            var sut = new ExportDataSanitizer(sanitizers);

            // Act
            FieldTypeHelper.FieldType fieldType = FieldTypeHelper.FieldType.Currency;
            Func<Task> action = async () =>
                await sut.SanitizeAsync(0, "foo", "bar", "src", fieldType, "test")
                    .ConfigureAwait(false);

            // Assert
            action.ShouldThrow<IntegrationPointsException>();
        }

        private static IEnumerable<TestCaseData> CorrectlyCheckForDataTypesToSanitizeTestCases()
        {
            var matchingSanitizer1 = new Mock<IExportFieldSanitizer>();
            matchingSanitizer1.SetupGet(x => x.SupportedType).Returns(FieldTypeHelper.FieldType.Currency);

            var matchingSanitizer2 = new Mock<IExportFieldSanitizer>();
            matchingSanitizer2.SetupGet(x => x.SupportedType).Returns(FieldTypeHelper.FieldType.Currency);

            var nonMatchingSanitizer = new Mock<IExportFieldSanitizer>();
            nonMatchingSanitizer.SetupGet(x => x.SupportedType).Returns(FieldTypeHelper.FieldType.Date);

            yield return new TestCaseData(null, false)
            {
                TestName = "Null sanitizers"
            };
            yield return new TestCaseData(Enumerable.Empty<IExportFieldSanitizer>(), false)
            {
                TestName = "Empty sanitizers"
            };
            yield return new TestCaseData(new[] { nonMatchingSanitizer.Object }, false)
            {
                TestName = "No matching sanitizer"
            };
            yield return new TestCaseData(new[] { matchingSanitizer1.Object }, true)
            {
                TestName = "One matching sanitizer"
            };
            yield return new TestCaseData(new[] { matchingSanitizer1.Object, nonMatchingSanitizer.Object }, true)
            {
                TestName = "Multiple sanitizers, one matching"
            };
        }

        private static IEnumerable<TestCaseData> ThrowExceptionForUnregisteredDataTypeTestCases()
        {
            var nonMatchingSanitizer = new Mock<IExportFieldSanitizer>();
            nonMatchingSanitizer.SetupGet(x => x.SupportedType).Returns(FieldTypeHelper.FieldType.Date);

            yield return new TestCaseData(null)
            {
                TestName = "Null sanitizers"
            };
            yield return new TestCaseData(Enumerable.Empty<IExportFieldSanitizer>())
            {
                TestName = "Empty sanitizers"
            };
            yield return new TestCaseData((IEnumerable<IExportFieldSanitizer>)new[] { nonMatchingSanitizer.Object })
            {
                TestName = "No matching sanitizer"
            };
        }
    }
}
