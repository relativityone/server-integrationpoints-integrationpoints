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
	internal sealed class SingleChoiceFieldSanitizerTests
	{
		private Mock<ISerializer> _serializer;

		[SetUp]
		public void SetUp()
		{
			_serializer = new Mock<ISerializer>();
			var jsonSerializer = new JSONSerializer();
			_serializer
				.Setup(x => x.Deserialize<Choice>(It.IsAny<string>()))
				.Returns((string serializedString) => jsonSerializer.Deserialize<Choice>(serializedString));
		}

		[Test]
		public void ItShouldSupportSingleChoice()
		{
			// Arrange
			var sut = new SingleChoiceFieldSanitizer(_serializer.Object);

			// Act
			string supportedType = sut.SupportedType;

			// Assert
			supportedType.Should().Be(FieldTypeHelper.FieldType.Code.ToString());
		}

		[Test]
		public async Task ItShouldReturnNullValueUnchanged()
		{
			// Arrange
			var sut = new SingleChoiceFieldSanitizer(_serializer.Object);

			// Act
			object result = await sut.SanitizeAsync(0, "foo", "bar", "bang", initialValue: null).ConfigureAwait(false);

			// Assert
			result.Should().BeNull();
		}

		[TestCaseSource(nameof(ThrowInvalidExportFieldValueExceptionWhenDeserializationFailsTestCases))]
		public void ItShouldThrowInvalidExportFieldValueExceptionWithTypesNamesWhenDeserializationFails(object initialValue)
		{
			// Arrange
			var sut = new SingleChoiceFieldSanitizer(_serializer.Object);

			// Act
			Func<Task> action = async () => await sut.SanitizeAsync(0, "foo", "bar", "bang", initialValue).ConfigureAwait(false);

			// Assert
			action.ShouldThrow<InvalidExportFieldValueException>()
				.Which.Message.Should()
				.Contain(typeof(Choice).Name).And
				.Contain(initialValue.GetType().Name);
		}

		[TestCaseSource(nameof(ThrowInvalidExportFieldValueExceptionWhenDeserializationFailsTestCases))]
		public void ItShouldThrowInvalidExportFieldValueExceptionWithInnerExceptionWhenDeserializationFails(object initialValue)
		{
			// Arrange
			var sut = new SingleChoiceFieldSanitizer(_serializer.Object);

			// Act
			Func<Task> action = async () => await sut.SanitizeAsync(0, "foo", "bar", "bang", initialValue).ConfigureAwait(false);

			// Assert
			action.ShouldThrow<InvalidExportFieldValueException>()
				.Which.InnerException.Should()
				.Match(ex => ex is JsonReaderException || ex is JsonSerializationException);	
		}

		[Test]
		public void ItShouldThrowInvalidExportFieldValueExceptionWhenChoiceNameIsNull()
		{
			// Arrange
			var sut = new SingleChoiceFieldSanitizer(_serializer.Object);

			// Act
			object initialValue = SanitizationTestUtils.DeserializeJson("{ \"ArtifactID\": 10123, \"Foo\": \"Bar\" }");
			Func<Task> action = async () => await sut.SanitizeAsync(0, "foo", "bar", "bang", initialValue).ConfigureAwait(false);

			// Assert
			action.ShouldThrow<InvalidExportFieldValueException>()
				.Which.Message.Should()
					.Contain(typeof(Choice).Name);
		}

		[Test]
		public async Task ItShouldReturnChoiceName()
		{
			// Arrange
			var sut = new SingleChoiceFieldSanitizer(_serializer.Object);
			const string expectedName = "Noice Choice";

			// Act
			object initialValue = SanitizationTestUtils.ToJToken<JObject>(new Choice { Name = expectedName });
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
