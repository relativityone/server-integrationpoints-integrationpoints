using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Transfer
{
    [TestFixture]
    internal sealed class ExportDataSanitizerTests
    {
        [Test]
        public void ItShouldThrowExceptionWhenGivenMultipleSanitizersWithSameDataType()
        {
            // Arrange
            var sanitizer1 = new Mock<IExportFieldSanitizer>();
            sanitizer1.SetupGet(x => x.SupportedType).Returns(RelativityDataType.Currency);
            var sanitizer2 = new Mock<IExportFieldSanitizer>();
            sanitizer2.SetupGet(x => x.SupportedType).Returns(RelativityDataType.Date);
            var sanitizer3 = new Mock<IExportFieldSanitizer>();
            sanitizer3.SetupGet(x => x.SupportedType).Returns(RelativityDataType.Currency);

            // Act
            Func<ExportDataSanitizer> action = () => new ExportDataSanitizer(new[] { sanitizer1.Object, sanitizer2.Object, sanitizer3.Object });

            // Assert
            action.Should().Throw<ArgumentException>()
                .Which.Message.Should().Contain("sanitizers"); // To ensure that Sync (and not an underlying type) is throwing this exception.
        }

        private static IEnumerable<TestCaseData> CorrectlyCheckForDataTypesToSanitizeTestCases()
        {
            var matchingSanitizer1 = new Mock<IExportFieldSanitizer>();
            matchingSanitizer1.SetupGet(x => x.SupportedType).Returns(RelativityDataType.Currency);

            var matchingSanitizer2 = new Mock<IExportFieldSanitizer>();
            matchingSanitizer2.SetupGet(x => x.SupportedType).Returns(RelativityDataType.Currency);

            var nonMatchingSanitizer = new Mock<IExportFieldSanitizer>();
            nonMatchingSanitizer.SetupGet(x => x.SupportedType).Returns(RelativityDataType.Date);

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

        [TestCaseSource(nameof(CorrectlyCheckForDataTypesToSanitizeTestCases))]
        public void ItShouldCorrectlyCheckForDataTypesToSanitize(IEnumerable<IExportFieldSanitizer> sanitizers, bool expectedResult)
        {
            // Arrange
            var instance = new ExportDataSanitizer(sanitizers);

            // Act
            bool result = instance.ShouldSanitize(RelativityDataType.Currency);

            // Assert
            result.Should().Be(expectedResult);
        }

        private static IEnumerable<TestCaseData> ThrowExceptionForUnregisteredDataTypeTestCases()
        {
            var nonMatchingSanitizer = new Mock<IExportFieldSanitizer>();
            nonMatchingSanitizer.SetupGet(x => x.SupportedType).Returns(RelativityDataType.Date);

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

        [TestCaseSource(nameof(ThrowExceptionForUnregisteredDataTypeTestCases))]
        public async Task ItShouldThrowExceptionForUnregisteredDataType(IEnumerable<IExportFieldSanitizer> sanitizers)
        {
            // Arrange
            var instance = new ExportDataSanitizer(sanitizers);

            // Act
            FieldInfoDto field = FieldInfoDto.DocumentField("src", "dst", false);
            field.RelativityDataType = RelativityDataType.Currency;
            Func<Task> action = async () =>
                await instance.SanitizeAsync(0, "foo", "bar", field, "test")
                    .ConfigureAwait(false);

            // Assert
            await action.Should().ThrowAsync<InvalidOperationException>().ConfigureAwait(false);
        }

        [Test]
        public async Task ItShouldInvokeSanitizerForCorrectDataType()
        {
            // Arrange
            Guid matchingResult = Guid.NewGuid();
            var matchingSanitizer = new Mock<IExportFieldSanitizer>();
            matchingSanitizer.Setup(x => x.SupportedType).Returns(RelativityDataType.Currency);
            matchingSanitizer
                .Setup(x => x.SanitizeAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(matchingResult);

            Guid nonMatchingResult = Guid.NewGuid();
            var nonMatchingSanitizer = new Mock<IExportFieldSanitizer>();
            nonMatchingSanitizer.Setup(x => x.SupportedType).Returns(RelativityDataType.Date);
            nonMatchingSanitizer
                .Setup(x => x.SanitizeAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(nonMatchingResult);

            var instance = new ExportDataSanitizer(new[] { matchingSanitizer.Object, nonMatchingSanitizer.Object });

            // Act
            FieldInfoDto field = FieldInfoDto.DocumentField("src", "dst", false);
            field.RelativityDataType = RelativityDataType.Currency;
            object result =
                await instance.SanitizeAsync(0, "foo", "bar", field, "test")
                    .ConfigureAwait(false);

            // Assert
            result.Should().Be(matchingResult);
        }

        [Test]
        public async Task ItShouldPassThroughExceptionFromSanitizer()
        {
            // Arrange
            var sanitizer = new Mock<IExportFieldSanitizer>();
            sanitizer.Setup(x => x.SupportedType).Returns(RelativityDataType.Currency);
            sanitizer
                .Setup(x => x.SanitizeAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()))
                .Throws<SyncException>();

            var instance = new ExportDataSanitizer(new[] { sanitizer.Object });

            // Act
            FieldInfoDto field = FieldInfoDto.DocumentField("src", "dst", false);
            field.RelativityDataType = RelativityDataType.Currency;
            Func<Task> action = async () =>
                await instance.SanitizeAsync(0, "foo", "bar", field, "test")
                    .ConfigureAwait(false);

            // Assert
            await action.Should().ThrowAsync<SyncException>().ConfigureAwait(false);
        }
    }
}
