using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Transfer;
using Relativity.Sync.Transfer.ImportAPI;

namespace Relativity.Sync.Tests.Unit.Transfer
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    internal class MultipleObjectFieldSanitizerTests
    {
        [Test]
        public void ItShouldSupportMultipleObject()
        {
            // Arrange
            var instance = new MultipleObjectFieldSanitizer();

            // Act
            RelativityDataType supportedType = instance.SupportedType;

            // Assert
            supportedType.Should().Be(RelativityDataType.MultipleObject);
        }

        private static IEnumerable<TestCaseData> ThrowInvalidExportFieldValueExceptionWhenDeserializationFailsTestCases()
        {
            yield return new TestCaseData(1);
            yield return new TestCaseData("foo");
            yield return new TestCaseData(new object());
            yield return new TestCaseData(JsonHelpers.DeserializeJson("{ \"not\": \"an array\" }"));
        }

        [TestCaseSource(nameof(ThrowInvalidExportFieldValueExceptionWhenDeserializationFailsTestCases))]
        public async Task ItShouldThrowInvalidExportFieldValueExceptionWithTypeNamesWhenDeserializationFails(object initialValue)
        {
            // Arrange
            var instance = new MultipleObjectFieldSanitizer();

            // Act
            Func<Task> action = () => instance.SanitizeAsync(0, "foo", "bar", "baz", initialValue);

            // Assert
            (await action.Should().ThrowAsync<InvalidExportFieldValueException>().ConfigureAwait(false))
                .Which.Message.Should()
                    .Contain(typeof(RelativityObjectValue[]).Name).And
                    .Contain(initialValue.GetType().Name);
        }

        [TestCaseSource(nameof(ThrowInvalidExportFieldValueExceptionWhenDeserializationFailsTestCases))]
        public async Task ItShouldThrowInvalidExportFieldValueExceptionWithInnerExceptionWhenDeserializationFails(object initialValue)
        {
            // Arrange
            var instance = new MultipleObjectFieldSanitizer();

            // Act
            Func<Task> action = () => instance.SanitizeAsync(0, "foo", "bar", "baz", initialValue);

            // Assert
            (await action.Should().ThrowAsync<InvalidExportFieldValueException>().ConfigureAwait(false))
                .Which.InnerException.Should()
                    .Match(ex => ex is JsonReaderException || ex is JsonSerializationException);
        }

        private static IEnumerable<TestCaseData> ThrowSyncItemLevelErrorExceptionWhenAnyElementsAreInvalidTestCases()
        {
            yield return new TestCaseData(JsonHelpers.DeserializeJson("[ { \"test\": 1 } ]"));
            yield return new TestCaseData(JsonHelpers.DeserializeJson("[ { \"ArtifactID\": 101, \"Name\": \"Cool Object\" }, { \"test\": 1 } ]"));
            yield return new TestCaseData(JsonHelpers.DeserializeJson("[ { \"ArtifactID\": 101, \"Name\": \"Cool Object\" }, { \"test\": 1 }, { \"ArtifactID\": 102, \"Name\": \"Cool Object 2\" } ]"));
        }

        [TestCaseSource(nameof(ThrowSyncItemLevelErrorExceptionWhenAnyElementsAreInvalidTestCases))]
        public async Task SanitizeAsync_ShouldThrowSyncItemLevelErrorException_WhenAnyElementsAreInvalid(object initialValue)
        {
            // Arrange
            var instance = new MultipleObjectFieldSanitizer();

            // Act
            Func<Task> action = () => instance.SanitizeAsync(0, "foo", "bar", "baz", initialValue);

            // Assert
            await action.Should().ThrowAsync<InvalidExportFieldValueException>().ConfigureAwait(false);
        }

        private static IEnumerable<TestCaseData> ThrowSyncExceptionWhenNameContainsMultiValueDelimiterTestCases()
        {
            yield return new TestCaseData(ObjectValueJArrayFromNames($"{LoadFileOptions._DEFAULT_MULTI_VALUE_ASCII} Sick Name"))
            {
                TestName = "Singleton violating name"
            };
            yield return new TestCaseData(ObjectValueJArrayFromNames("Okay Name", $"Cool{LoadFileOptions._DEFAULT_MULTI_VALUE_ASCII} Name", "Awesome Name"))
            {
                TestName = "Single violating name in larger collection"
            };
            yield return new TestCaseData(ObjectValueJArrayFromNames("Okay Name", $"Cool{LoadFileOptions._DEFAULT_MULTI_VALUE_ASCII} Name", $"Awesome{LoadFileOptions._DEFAULT_MULTI_VALUE_ASCII} Name"))
            {
                TestName = "Many violating names in larger collection"
            };
        }

        [TestCaseSource(nameof(ThrowSyncExceptionWhenNameContainsMultiValueDelimiterTestCases))]
        public async Task ItShouldThrowSyncExceptionWhenNameContainsMultiValueDelimiter(object initialValue)
        {
            // Arrange
            var instance = new MultipleObjectFieldSanitizer();

            // Act
            Func<Task> action = () => instance.SanitizeAsync(0, "foo", "bar", "baz", initialValue);

            // Assert
            (await action.Should().ThrowAsync<InvalidExportFieldValueException>().ConfigureAwait(false))
                .Which.Message.Should()
                .Be("Unable to parse data from Relativity Export API: " +
                    $"The identifiers of the objects in Multiple Object field contain the character specified as the multi-value delimiter ('ASCII {(int)LoadFileOptions._DEFAULT_MULTI_VALUE_ASCII}'). " +
                    "Rename these objects to not contain delimiter.");
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
            yield return new TestCaseData(
                ObjectValueJArrayFromNames("Sick Name", "Cool Name", "Awesome Name"),
                $"Sick Name{LoadFileOptions._DEFAULT_MULTI_VALUE_ASCII}Cool Name{LoadFileOptions._DEFAULT_MULTI_VALUE_ASCII}Awesome Name")
            {
                TestName = "Multiple"
            };
        }

        [TestCaseSource(nameof(CombineNamesIntoReturnValueTestCases))]
        public async Task ItShouldCombineNamesIntoReturnValue(object initialValue, string expectedResult)
        {
            // Arrange
            var instance = new MultipleObjectFieldSanitizer();

            // Act
            object result = await instance.SanitizeAsync(0, "foo", "bar", "baz", initialValue).ConfigureAwait(false);

            // Assert
            result.Should().Be(expectedResult);
        }

        private static JArray ObjectValueJArrayFromNames(params string[] names)
        {
            RelativityObjectValue[] values = names.Select(x => new RelativityObjectValue { Name = x }).ToArray();
            return JsonHelpers.ToJToken<JArray>(values);
        }
    }
}
