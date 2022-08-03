using System;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Services.Exporter.Sanitization;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Relativity;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Core.Tests.Services.Exporter.Sanitization
{
    [TestFixture, Category("Unit")]
    internal class SingleObjectFieldSanitizerTests
    {
        private Mock<ISanitizationDeserializer> _sanitizationHelper;

        [SetUp]
        public void SetUp()
        {
            _sanitizationHelper = new Mock<ISanitizationDeserializer>();
            var jsonSerializer = new JSONSerializer();
            _sanitizationHelper
                .Setup(x => x.DeserializeAndValidateExportFieldValue<RelativityObjectValue>(It.IsAny<object>()))
                .Returns((object serializedObject) =>
                    jsonSerializer.Deserialize<RelativityObjectValue>(serializedObject.ToString()));
        }

        [Test]
        public void ItShouldSupportSingleObject()
        {
            // Arrange
            var sut = new SingleObjectFieldSanitizer(_sanitizationHelper.Object);

            // Act
            FieldTypeHelper.FieldType supportedType = sut.SupportedType;

            // Assert
            supportedType.Should().Be(FieldTypeHelper.FieldType.Object);
        }

        [Test]
        public async Task ItShouldReturnNullValueUnchanged()
        {
            // Arrange
            var sut = new SingleObjectFieldSanitizer(_sanitizationHelper.Object);

            // Act
            object result = await sut.SanitizeAsync(0, "foo", "bar", "bang", initialValue: null).ConfigureAwait(false);

            // Assert
            result.Should().BeNull();
        }

        [TestCase("")]
        [TestCase("\"ArtifactID\": 0")]
        public async Task ItShouldReturnNullWhenArtifactIdIsZero(string jsonArtifactIdProperty)
        {
            // Arrange
            var sut = new SingleObjectFieldSanitizer(_sanitizationHelper.Object);

            // Act
            object initialValue = SanitizationTestUtils.DeserializeJson($"{{ {jsonArtifactIdProperty} }}");
            object result = await sut.SanitizeAsync(0, "foo", "bar", "bang", initialValue).ConfigureAwait(false);

            // Assert
            result.Should().BeNull();
        }

        [TestCase("")]
        [TestCase("\"Name\": \"\"")]
        [TestCase("\"Name\": \"  \"")]
        public void ItShouldThrowInvalidExportFieldValueExceptionWhenObjectNameIsInvalidAndArtifactIDIsValid(string jsonNameProperty)
        {
            // Arrange
            var sut = new SingleObjectFieldSanitizer(_sanitizationHelper.Object);

            // Act
            object initialValue = SanitizationTestUtils.DeserializeJson($"{{ \"ArtifactID\": 10123, {jsonNameProperty} }}");
            Func<Task> action = async () => await sut.SanitizeAsync(0, "foo", "bar", "bang", initialValue).ConfigureAwait(false);

            // Assert
            action.ShouldThrow<InvalidExportFieldValueException>()
                .Which.Message.Should()
                    .Contain(typeof(RelativityObjectValue).Name);
        }

        [Test]
        public async Task ItShouldReturnObjectName()
        {
            // Arrange
            var sut = new SingleObjectFieldSanitizer(_sanitizationHelper.Object);
            const string expectedName = "Awesome Object";

            // Act
            object initialValue = SanitizationTestUtils.ToJToken<JObject>(new RelativityObjectValue { ArtifactID = 1, Name = expectedName });
            object result = await sut.SanitizeAsync(0, "foo", "bar", "bang", initialValue).ConfigureAwait(false);

            // Assert
            result.Should().Be(expectedName);
        }
    }
}
