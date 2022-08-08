using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Services.Exporter.Sanitization;
using kCura.IntegrationPoints.Domain.Exceptions;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Relativity;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Core.Tests.Services.Exporter.Sanitization
{
    [TestFixture, Category("Unit")]
    internal class MultipleObjectFieldSanitizerTests
    {
        private Mock<ISanitizationDeserializer> _sanitizationHelper;
        private const char _MUTLI_DELIM = IntegrationPoints.Domain.Constants.MULTI_VALUE_DELIMITER;

        [SetUp]
        public void SetUp()
        {
            _sanitizationHelper = new Mock<ISanitizationDeserializer>();
            var jsonSerializer = new JSONSerializer();
            _sanitizationHelper
                .Setup(x => x.DeserializeAndValidateExportFieldValue<RelativityObjectValue[]>(It.IsAny<object>()))
                .Returns((object serializedObject) =>
                    jsonSerializer.Deserialize<RelativityObjectValue[]>(serializedObject.ToString()));
        }

        [Test]
        public void ItShouldSupportMultipleObject()
        {
            // Arrange
            var sut = new MultipleObjectFieldSanitizer(_sanitizationHelper.Object);

            // Act
            FieldTypeHelper.FieldType supportedType = sut.SupportedType;

            // Assert
            supportedType.Should().Be(FieldTypeHelper.FieldType.Objects);
        }

        [TestCaseSource(nameof(ThrowInvalidExportFieldValueExceptionWhenAnyElementsAreInvalidTestCases))]
        public void ItShouldThrowInvalidExportFieldValueExceptionWhenAnyElementsAreInvalid(object initialValue)
        {
            // Arrange
            var sut = new MultipleObjectFieldSanitizer(_sanitizationHelper.Object);

            // Act
            Func<Task> action = () => sut.SanitizeAsync(0, "foo", "bar", "baz", initialValue);

            // Assert
            action.ShouldThrow<InvalidExportFieldValueException>();
        }

        [TestCaseSource(nameof(ThrowInvalidExportFieldValueExceptionWhenNameContainsMultiValueDelimiterTestCases))]
        public void ItShouldThrowInvalidExportFieldValueExceptionWhenNameContainsMultiValueDelimiter(object initialValue)
        {
            // Arrange
            var sut = new MultipleObjectFieldSanitizer(_sanitizationHelper.Object);

            // Act
            Func<Task> action = () => sut.SanitizeAsync(0, "foo", "bar", "baz", initialValue);

            // Assert
            action.ShouldThrow<InvalidExportFieldValueException>();
        }

        [TestCaseSource(nameof(CombineNamesIntoReturnValueTestCases))]
        public async Task ItShouldCombineNamesIntoReturnValue(object initialValue, string expectedResult)
        {
            // Arrange
            var sut = new MultipleObjectFieldSanitizer(_sanitizationHelper.Object);

            // Act
            object result = await sut.SanitizeAsync(0, "foo", "bar", "baz", initialValue).ConfigureAwait(false);

            // Assert
            result.Should().Be(expectedResult);
        }

        private static IEnumerable<TestCaseData> ThrowInvalidExportFieldValueExceptionWhenAnyElementsAreInvalidTestCases()
        {
            yield return new TestCaseData(SanitizationTestUtils.DeserializeJson("[ { \"test\": 1 } ]"));
            yield return new TestCaseData(SanitizationTestUtils.DeserializeJson("[ { \"ArtifactID\": 101, \"Name\": \"Cool Object\" }, { \"test\": 1 } ]"));
            yield return new TestCaseData(SanitizationTestUtils.DeserializeJson("[ { \"ArtifactID\": 101, \"Name\": \"Cool Object\" }, { \"test\": 1 }, { \"ArtifactID\": 102, \"Name\": \"Cool Object 2\" } ]"));
        }

        private static IEnumerable<TestCaseData> ThrowInvalidExportFieldValueExceptionWhenNameContainsMultiValueDelimiterTestCases()
        {
            yield return new TestCaseData(ObjectValueJArrayFromNames($"{_MUTLI_DELIM} Sick Name"))
            {
                TestName = "Singleton violating name"
            };
            yield return new TestCaseData(ObjectValueJArrayFromNames("Okay Name", $"Cool{_MUTLI_DELIM} Name", "Awesome Name"))
            {
                TestName = "Single violating name in larger collection"
            };
            yield return new TestCaseData(ObjectValueJArrayFromNames("Okay Name", $"Cool{_MUTLI_DELIM} Name", $"Awesome{_MUTLI_DELIM} Name"))
            {
                TestName = "Many violating names in larger collection"
            };
        }

        private static IEnumerable<TestCaseData> CombineNamesIntoReturnValueTestCases()
        {
            yield return new TestCaseData(null, null)
            {
                TestName = "Null"
            };
            yield return new TestCaseData(ObjectValueJArrayFromNames(), string.Empty)
            {
                TestName = "Empty"
            };
            yield return new TestCaseData(ObjectValueJArrayFromNames("Sick Name"), "Sick Name")
            {
                TestName = "Single"
            };
            yield return new TestCaseData(ObjectValueJArrayFromNames("Sick Name", "Cool Name", "Awesome Name"),
                $"Sick Name{_MUTLI_DELIM}Cool Name{_MUTLI_DELIM}Awesome Name")
            {
                TestName = "Multiple"
            };
        }

        private static JArray ObjectValueJArrayFromNames(params string[] names)
        {
            RelativityObjectValue[] values = names.Select(x => new RelativityObjectValue { Name = x }).ToArray();
            return SanitizationTestUtils.ToJToken<JArray>(values);
        }
    }
}
