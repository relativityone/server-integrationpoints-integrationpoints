using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Services.Exporter.Sanitization;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Relativity;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Core.Tests.Services.Exporter.Sanitization
{
	[TestFixture]
	internal class SingleObjectFieldSanitizerTests
	{
		private Mock<ISerializer> _serializer;

		[SetUp]
		public void SetUp()
		{
			_serializer = new Mock<ISerializer>();
			var jsonSerializer = new JSONSerializer();
			_serializer
				.Setup(x => x.Deserialize<RelativityObjectValue>(It.IsAny<string>()))
				.Returns((string serializedString) => jsonSerializer.Deserialize<RelativityObjectValue>(serializedString));
		}

		[Test]
		public void ItShouldSupportSingleObject()
		{
			// Arrange
			var sut = new SingleObjectFieldSanitizer(_serializer.Object);

			// Act
			string supportedType = sut.SupportedType;

			// Assert
			supportedType.Should().Be(FieldTypeHelper.FieldType.Object.ToString());
		}

		[Test]
		public async Task ItShouldReturnNullValueUnchanged()
		{
			// Arrange
			var sut = new SingleObjectFieldSanitizer(_serializer.Object);

			// Act
			object result = await sut.SanitizeAsync(0, "foo", "bar", "bang", initialValue: null).ConfigureAwait(false);

			// Assert
			result.Should().BeNull();
		}

		[TestCaseSource(nameof(ThrowInvalidExportFieldValueExceptionWhenDeserializationFailsTestCases))]
		public void ItShouldThrowInvalidExportFieldValueExceptionWithTypesNamesWhenDeserializationFails(object initialValue)
		{
			// Arrange
			var sut = new SingleObjectFieldSanitizer(_serializer.Object);

			// Act
			Func<Task> action = async () => await sut.SanitizeAsync(0, "foo", "bar", "bang", initialValue).ConfigureAwait(false);

			// Assert
			action.ShouldThrow<InvalidExportFieldValueException>()
				.Which.Message.Should()
					.Contain(typeof(RelativityObjectValue).Name).And
					.Contain(initialValue.GetType().Name);
		}

		[TestCaseSource(nameof(ThrowInvalidExportFieldValueExceptionWhenDeserializationFailsTestCases))]
		public void ItShouldThrowInvalidExportFieldValueExceptionWithInnerExceptionWhenDeserializationFails(object initialValue)
		{
			// Arrange
			var sut = new SingleObjectFieldSanitizer(_serializer.Object);

			// Act
			Func<Task> action = async () => await sut.SanitizeAsync(0, "foo", "bar", "bang", initialValue).ConfigureAwait(false);

			// Assert
			action.ShouldThrow<InvalidExportFieldValueException>()
				.Which.InnerException.Should()
					.Match(ex => ex is JsonReaderException || ex is JsonSerializationException);
		}

		[TestCase("")]
		[TestCase("\"ArtifactID\": 0")]
		public async Task ItShouldReturnNullWhenArtifactIdIsZero(string jsonArtifactIdProperty)
		{
			// Arrange
			var sut = new SingleObjectFieldSanitizer(_serializer.Object);

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
			var sut = new SingleObjectFieldSanitizer(_serializer.Object);

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
			var sut = new SingleObjectFieldSanitizer(_serializer.Object);
			const string expectedName = "Awesome Object";

			// Act
			object initialValue = SanitizationTestUtils.ToJToken<JObject>(new RelativityObjectValue { ArtifactID = 1, Name = expectedName });
			object result = await sut.SanitizeAsync(0, "foo", "bar", "bang", initialValue).ConfigureAwait(false);

			// Assert
			result.Should().Be(expectedName);
		}

		private static IEnumerable<TestCaseData> ThrowInvalidExportFieldValueExceptionWhenDeserializationFailsTestCases()
		{
			yield return new TestCaseData(1);
			yield return new TestCaseData("foo");
			yield return new TestCaseData(new object());
			yield return new TestCaseData(SanitizationTestUtils.DeserializeJson("[ \"not\", \"an object\" ]"));
		}
	}
}
