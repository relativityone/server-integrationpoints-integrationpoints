using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Transfer
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    internal class MultipleObjectFieldSanitizerTests
    {
        private const char _NESTED_DELIM = (char)29;
        private const char _MUTLI_DELIM = (char)30;

        [Test]
        public void ItShouldSupportMultipleObject()
        {
            // Arrange
            IDocumentSynchronizationConfiguration configuration = CreateConfiguration();
            var instance = new MultipleObjectFieldSanitizer(configuration);

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
            IDocumentSynchronizationConfiguration configuration = CreateConfiguration();
            var instance = new MultipleObjectFieldSanitizer(configuration);

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
            IDocumentSynchronizationConfiguration configuration = CreateConfiguration();
            var instance = new MultipleObjectFieldSanitizer(configuration);

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
            IDocumentSynchronizationConfiguration configuration = CreateConfiguration();
            var instance = new MultipleObjectFieldSanitizer(configuration);

            // Act
            Func<Task> action = () => instance.SanitizeAsync(0, "foo", "bar", "baz", initialValue);

            // Assert
            await action.Should().ThrowAsync<InvalidExportFieldValueException>().ConfigureAwait(false);
        }

        private static IEnumerable<TestCaseData> ThrowSyncExceptionWhenNameContainsMultiValueDelimiterTestCases()
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

        [TestCaseSource(nameof(ThrowSyncExceptionWhenNameContainsMultiValueDelimiterTestCases))]
        public async Task ItShouldThrowSyncExceptionWhenNameContainsMultiValueDelimiter(object initialValue)
        {
            // Arrange
            IDocumentSynchronizationConfiguration configuration = CreateConfiguration();
            var instance = new MultipleObjectFieldSanitizer(configuration);

            // Act
            Func<Task> action = () => instance.SanitizeAsync(0, "foo", "bar", "baz", initialValue);

            // Assert
            (await action.Should().ThrowAsync<InvalidExportFieldValueException>().ConfigureAwait(false))
                .Which.Message.Should()
                .Be("Unable to parse data from Relativity Export API: " +
                    "The identifiers of the objects in Multiple Object field contain the character specified as the multi-value delimiter ('ASCII 30'). " +
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
                $"Sick Name{_MUTLI_DELIM}Cool Name{_MUTLI_DELIM}Awesome Name")
            {
                TestName = "Multiple"
            };
        }

        [TestCaseSource(nameof(CombineNamesIntoReturnValueTestCases))]
        public async Task ItShouldCombineNamesIntoReturnValue(object initialValue, string expectedResult)
        {
            // Arrange
            IDocumentSynchronizationConfiguration configuration = CreateConfiguration();
            var instance = new MultipleObjectFieldSanitizer(configuration);

            // Act
            object result = await instance.SanitizeAsync(0, "foo", "bar", "baz", initialValue).ConfigureAwait(false);

            // Assert
            result.Should().Be(expectedResult);
        }

        private static IDocumentSynchronizationConfiguration CreateConfiguration()
        {
            var config = new Mock<IDocumentSynchronizationConfiguration>();
            config.SetupGet(x => x.NestedValueDelimiter).Returns(_NESTED_DELIM);
            config.SetupGet(x => x.MultiValueDelimiter).Returns(_MUTLI_DELIM);
            return config.Object;
        }

        private static JArray ObjectValueJArrayFromNames(params string[] names)
        {
            RelativityObjectValue[] values = names.Select(x => new RelativityObjectValue { Name = x }).ToArray();
            return JsonHelpers.ToJToken<JArray>(values);
        }
    }
}
