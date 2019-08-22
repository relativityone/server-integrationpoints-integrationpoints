using System;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Services.Exporter.Sanitization;
using kCura.IntegrationPoints.Data.Repositories.DTO;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Relativity;

namespace kCura.IntegrationPoints.Core.Tests.Services.Exporter.Sanitization
{
	[TestFixture]
	internal sealed class SingleChoiceFieldSanitizerTests
	{
		private Mock<ISanitizationHelper> _sanitizationHelper;

		[SetUp]
		public void SetUp()
		{
			_sanitizationHelper = new Mock<ISanitizationHelper>();
			var jsonSerializer = new JSONSerializer();
			_sanitizationHelper
				.Setup(x => x.DeserializeAndValidateExportFieldValue<ChoiceDto>(
					It.IsAny<string>(),
					It.IsAny<string>(), 
					It.IsAny<object>()))
				.Returns((string x, string y, object serializedObject) =>
					jsonSerializer.Deserialize<ChoiceDto>(serializedObject.ToString()));
		}

		[Test]
		public void ItShouldSupportSingleChoice()
		{
			// Arrange
			var sut = new SingleChoiceFieldSanitizer(_sanitizationHelper.Object);

			// Act
			FieldTypeHelper.FieldType supportedType = sut.SupportedType;

			// Assert
			supportedType.Should().Be(FieldTypeHelper.FieldType.Code);
		}

		[Test]
		public async Task ItShouldReturnNullValueUnchanged()
		{
			// Arrange
			var sut = new SingleChoiceFieldSanitizer(_sanitizationHelper.Object);

			// Act
			object result = await sut.SanitizeAsync(0, "foo", "bar", "bang", initialValue: null).ConfigureAwait(false);

			// Assert
			result.Should().BeNull();
		}

		[Test]
		public void ItShouldThrowInvalidExportFieldValueExceptionWhenChoiceNameIsNull()
		{
			// Arrange
			var sut = new SingleChoiceFieldSanitizer(_sanitizationHelper.Object);

			// Act
			object initialValue = SanitizationTestUtils.DeserializeJson("{ \"ArtifactID\": 10123, \"Foo\": \"Bar\" }");
			Func<Task> action = async () => await sut.SanitizeAsync(0, "foo", "bar", "bang", initialValue).ConfigureAwait(false);

			// Assert
			action.ShouldThrow<InvalidExportFieldValueException>()
				.Which.Message.Should()
					.Contain(typeof(ChoiceDto).Name);
		}

		[Test]
		public async Task ItShouldReturnChoiceName()
		{
			// Arrange
			var sut = new SingleChoiceFieldSanitizer(_sanitizationHelper.Object);
			const string expectedName = "Noice Choice";

			// Act
			object initialValue = SanitizationTestUtils.ToJToken<JObject>(new ChoiceDto(artifactID: 0, name: expectedName));
			object result = await sut.SanitizeAsync(0, "foo", "bar", "bang", initialValue).ConfigureAwait(false);

			// Assert
			result.Should().Be(expectedName);
		}
	}
}
