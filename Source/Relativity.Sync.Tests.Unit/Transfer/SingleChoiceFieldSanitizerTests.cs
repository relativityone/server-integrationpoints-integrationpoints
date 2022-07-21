using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Transfer
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    internal sealed class SingleChoiceFieldSanitizerTests
    {
        [Test]
        public void ItShouldSupportSingleChoice()
        {
            // Arrange
            var instance = new SingleChoiceFieldSanitizer();

            // Act
            RelativityDataType supportedType = instance.SupportedType;

            // Assert
            supportedType.Should().Be(RelativityDataType.SingleChoice);
        }

        [Test]
        public async Task ItShouldReturnNullValueUnchanged()
        {
            // Arrange
            var instance = new SingleChoiceFieldSanitizer();

            // Act
            object initialValue = null;
            object result = await instance.SanitizeAsync(0, "foo", "bar", "bang", initialValue).ConfigureAwait(false);

            // Assert
            result.Should().BeNull();
        }

        private static IEnumerable<TestCaseData> ThrowInvalidExportFieldValueExceptionWhenDeserializationFailsTestCases()
        {
            yield return new TestCaseData(1);
            yield return new TestCaseData("foo");
            yield return new TestCaseData(new object());
            yield return new TestCaseData(JsonHelpers.DeserializeJson("[ \"not\", \"an object\" ]"));
        }

        [TestCaseSource(nameof(ThrowInvalidExportFieldValueExceptionWhenDeserializationFailsTestCases))]
        public async Task ItShouldThrowInvalidExportFieldValueExceptionWithTypesNamesWhenDeserializationFails(object initialValue)
        {
            // Arrange
            var instance = new SingleChoiceFieldSanitizer();

            // Act
            Func<Task> action = async () => await instance.SanitizeAsync(0, "foo", "bar", "bang", initialValue).ConfigureAwait(false);

            // Assert
            (await action.Should().ThrowAsync<InvalidExportFieldValueException>().ConfigureAwait(false))
                .Which.Message.Should()
                    .Contain(typeof(Choice).Name).And
                    .Contain(initialValue.GetType().Name);
        }

        [TestCaseSource(nameof(ThrowInvalidExportFieldValueExceptionWhenDeserializationFailsTestCases))]
        public async Task ItShouldThrowInvalidExportFieldValueExceptionWithInnerExceptionWhenDeserializationFails(object initialValue)
        {
            // Arrange
            var instance = new SingleChoiceFieldSanitizer();

            // Act
            Func<Task> action = async () => await instance.SanitizeAsync(0, "foo", "bar", "bang", initialValue).ConfigureAwait(false);

            // Assert
            (await action.Should().ThrowAsync<InvalidExportFieldValueException>().ConfigureAwait(false))
                .Which.InnerException.Should()
                    .Match(ex => ex is JsonReaderException || ex is JsonSerializationException);
        }

        [Test]
        public async Task ItShouldThrowInvalidExportFieldValueExceptionWhenChoiceNameIsNull()
        {
            // Arrange
            var instance = new SingleChoiceFieldSanitizer();

            // Act
            object initialValue = JsonHelpers.DeserializeJson("{ \"ArtifactID\": 10123, \"Foo\": \"Bar\" }");
            Func<Task> action = async () => await instance.SanitizeAsync(0, "foo", "bar", "bang", initialValue).ConfigureAwait(false);

            // Assert
            (await action.Should().ThrowAsync<InvalidExportFieldValueException>().ConfigureAwait(false))
                .Which.Message.Should()
                    .Contain(typeof(Choice).Name);
        }

        [Test]
        public async Task ItShouldReturnChoiceName()
        {
            // Arrange
            var instance = new SingleChoiceFieldSanitizer();
            const string expectedName = "Noice Choice";

            // Act
            object initialValue = JsonHelpers.ToJToken<JObject>(new Choice { Name = expectedName });
            object result = await instance.SanitizeAsync(0, "foo", "bar", "bang", initialValue).ConfigureAwait(false);

            // Assert
            result.Should().Be(expectedName);
        }
    }
}
