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
    internal class SingleObjectFieldSanitizerTests
    {
        [Test]
        public void ItShouldSupportSingleObject()
        {
            // Arrange
            var instance = new SingleObjectFieldSanitizer();

            // Act
            RelativityDataType supportedType = instance.SupportedType;

            // Assert
            supportedType.Should().Be(RelativityDataType.SingleObject);
        }

        [Test]
        public async Task ItShouldReturnNullValueUnchanged()
        {
            // Arrange
            var instance = new SingleObjectFieldSanitizer();

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
            var instance = new SingleObjectFieldSanitizer();

            // Act
            Func<Task> action = async () => await instance.SanitizeAsync(0, "foo", "bar", "bang", initialValue).ConfigureAwait(false);

            // Assert
            (await action.Should().ThrowAsync<InvalidExportFieldValueException>().ConfigureAwait(false))
                .Which.Message.Should()
                    .Contain(typeof(RelativityObjectValue).Name).And
                    .Contain(initialValue.GetType().Name);
        }

        [TestCaseSource(nameof(ThrowInvalidExportFieldValueExceptionWhenDeserializationFailsTestCases))]
        public async Task ItShouldThrowInvalidExportFieldValueExceptionWithInnerExceptionWhenDeserializationFails(object initialValue)
        {
            // Arrange
            var instance = new SingleObjectFieldSanitizer();

            // Act
            Func<Task> action = async () => await instance.SanitizeAsync(0, "foo", "bar", "bang", initialValue).ConfigureAwait(false);

            // Assert
            (await action.Should().ThrowAsync<InvalidExportFieldValueException>().ConfigureAwait(false))
                .Which.InnerException.Should()
                    .Match(ex => ex is JsonReaderException || ex is JsonSerializationException);
        }

        [TestCase("")]
        [TestCase("\"ArtifactID\": 0")]
        public async Task ItShouldReturnNullWhenArtifactIdIsZero(string jsonArtifactIdProperty)
        {
            // Arrange
            var instance = new SingleObjectFieldSanitizer();

            // Act
            object initialValue = JsonHelpers.DeserializeJson($"{{ {jsonArtifactIdProperty} }}");
            object result = await instance.SanitizeAsync(0, "foo", "bar", "bang", initialValue).ConfigureAwait(false);

            // Assert
            result.Should().BeNull();
        }

        [TestCase("")]
        [TestCase("\"Name\": \"\"")]
        [TestCase("\"Name\": \"  \"")]
        public async Task ItShouldThrowInvalidExportFieldValueExceptionWhenObjectNameIsInvalidAndArtifactIDIsValid(string jsonNameProperty)
        {
            // Arrange
            var instance = new SingleObjectFieldSanitizer();

            // Act
            object initialValue = JsonHelpers.DeserializeJson($"{{ \"ArtifactID\": 10123, {jsonNameProperty} }}");
            Func<Task> action = async () => await instance.SanitizeAsync(0, "foo", "bar", "bang", initialValue).ConfigureAwait(false);

            // Assert
            (await action.Should().ThrowAsync<InvalidExportFieldValueException>().ConfigureAwait(false))
                .Which.Message.Should()
                    .Contain(typeof(RelativityObjectValue).Name);
        }

        [Test]
        public async Task ItShouldReturnObjectName()
        {
            // Arrange
            var instance = new SingleObjectFieldSanitizer();
            const string expectedName = "Awesome Object";

            // Act
            object initialValue = JsonHelpers.ToJToken<JObject>(new RelativityObjectValue { ArtifactID = 1, Name = expectedName });
            object result = await instance.SanitizeAsync(0, "foo", "bar", "bang", initialValue).ConfigureAwait(false);

            // Assert
            result.Should().Be(expectedName);
        }
    }
}
