using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Services.Exporter.Sanitization;
using kCura.IntegrationPoints.Domain.Exceptions;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Relativity;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Core.Tests.Services.Exporter.Sanitization
{
	[TestFixture]
	internal class MultipleChoiceFieldSanitizerTests
	{
		private Mock<IChoiceCache> _choiceCache;
		private Mock<IChoiceTreeToStringConverter> _choiceTreeToStringConverter;
		private Mock<ISerializer> _serializer;
		private MultipleChoiceFieldSanitizer _sut;

		private const char _NESTED_VALUE = IntegrationPoints.Domain.Constants.NESTED_VALUE_DELIMITER;
		private const char _MULTI_VALUE = IntegrationPoints.Domain.Constants.MULTI_VALUE_DELIMITER;

		[SetUp]
		public void SetUp()
		{
			_choiceCache = new Mock<IChoiceCache>();
			_choiceTreeToStringConverter = new Mock<IChoiceTreeToStringConverter>();
			_serializer = new Mock<ISerializer>();
			var jsonSerializer = new JSONSerializer();
			_serializer
				.Setup(x => x.Deserialize<Choice[]>(It.IsAny<string>()))
				.Returns((string serializedString) => jsonSerializer.Deserialize<Choice[]>(serializedString));
			_sut = new MultipleChoiceFieldSanitizer(_choiceCache.Object, _choiceTreeToStringConverter.Object, _serializer.Object);
		}

		[Test]
		public void ItShouldSupportMultipleChoice()
		{
			// Act
			string supportedType = _sut.SupportedType;

			// Assert
			supportedType.Should().Be(FieldTypeHelper.FieldType.MultiCode.ToString());
		}

		[TestCaseSource(nameof(ThrowExceptionWhenDeserializationFailsTestCases))]
		public void ItShouldThrowInvalidExportFieldValueExceptionWithTypeNamesWhenDeserializationFails(object initialValue)
		{
			// Act
			Func<Task> action = async () => await _sut.SanitizeAsync(0, "foo", "bar", "baz", initialValue).ConfigureAwait(false);

			// Assert
			action.ShouldThrow<InvalidExportFieldValueException>()
				.Which.Message.Should()
				.Contain(typeof(Choice[]).Name).And
				.Contain(initialValue.GetType().Name);
		}

		[TestCaseSource(nameof(ThrowExceptionWhenDeserializationFailsTestCases))]
		public void ItShouldThrowInvalidExportFieldValueExceptionWithInnerExceptionWhenDeserializationFails(object initialValue)
		{
			// Act
			Func<Task> action = async () => await _sut.SanitizeAsync(0, "foo", "bar", "baz", initialValue).ConfigureAwait(false);

			// Assert
			action.ShouldThrow<InvalidExportFieldValueException>()
				.Which.InnerException.Should()
				.Match(ex => ex is JsonReaderException || ex is JsonSerializationException);	
		}

		[TestCaseSource(nameof(ThrowExceptionWhenAnyElementsAreInvalidTestCases))]
		public void ItShouldThrowInvalidExportFieldValueExceptionWhenAnyElementsAreInvalid(object initialValue)
		{
			// Act
			Func<Task> action = async () => await _sut.SanitizeAsync(0, "foo", "bar", "baz", initialValue).ConfigureAwait(false);

			// Assert
			action.ShouldThrow<InvalidExportFieldValueException>()
				.Which.Message.Should()
				.Contain(typeof(Choice).Name);
		}

		[TestCaseSource(nameof(ThrowSyncExceptionWhenNameContainsDelimiterTestCases))]
		public void ItShouldThrowSyncExceptionWhenNameContainsDelimiter(object initialValue, string expectedViolators)
		{
			// Act
			Func<Task> action = async () => await _sut.SanitizeAsync(0, "foo", "bar", "baz", initialValue).ConfigureAwait(false);

			// Assert
			action.ShouldThrow<IntegrationPointsException>()
				.Which.Message.Should().MatchRegex($" {expectedViolators}\\.$");
		}

		[Test]
		public async Task ItShouldReturnNull()
		{
			// Act
			object result = await _sut.SanitizeAsync(0, "foo", "bar", "baz", null).ConfigureAwait(false);

			// Assert
			result.Should().BeNull();
		}

		[Test]
		public async Task ItShouldReturnEmptyString()
		{
			_choiceCache.Setup(x => x.GetChoicesWithParentInfoAsync(It.IsAny<ICollection<Choice>>())).ReturnsAsync(new List<ChoiceWithParentInfo>());
			_choiceTreeToStringConverter.Setup(x => x.ConvertTreeToString(It.IsAny<IList<ChoiceWithChildInfo>>())).Returns(string.Empty);

			// Act
			object result = await _sut.SanitizeAsync(0, "foo", "bar", "baz", ChoiceJArrayFromNames().ToString()).ConfigureAwait(false);

			// Assert
			result.Should().Be(string.Empty);
		}

		private static IEnumerable<TestCaseData> ThrowExceptionWhenAnyElementsAreInvalidTestCases()
		{
			yield return new TestCaseData(SanitizationTestUtils.DeserializeJson("[ { \"test\": 1 } ]"));
			yield return new TestCaseData(SanitizationTestUtils.DeserializeJson("[ { \"ArtifactID\": 101, \"Name\": \"Cool Choice\" }, { \"test\": 1 } ]"));
			yield return new TestCaseData(SanitizationTestUtils.DeserializeJson("[ { \"ArtifactID\": 101, \"Name\": \"Cool Choice\" }, { \"test\": 1 }, { \"ArtifactID\": 102, \"Name\": \"Cool Choice 2\" } ]"));
		}

		private static IEnumerable<TestCaseData> ThrowSyncExceptionWhenNameContainsDelimiterTestCases()
		{
			yield return new TestCaseData(
				ChoiceJArrayFromNames($"{_MULTI_VALUE} Sick Choice"),
				$"'{_MULTI_VALUE} Sick Choice'")
			{
				TestName = "MultiValue - Singleton"
			};
			yield return new TestCaseData(
				ChoiceJArrayFromNames("Okay Name", $"Cool{_MULTI_VALUE} Name", "Awesome Name"),
				$"'Cool{_MULTI_VALUE} Name'")
			{
				TestName = "MultiValue - Single violating name in larger collection"
			};
			yield return new TestCaseData(
				ChoiceJArrayFromNames("Okay Name", $"Cool{_MULTI_VALUE} Name", $"Awesome{_MULTI_VALUE} Name"),
				$"'Cool{_MULTI_VALUE} Name', 'Awesome{_MULTI_VALUE} Name'")
			{
				TestName = "MultiValue - Many violating names in larger collection"
			};

			yield return new TestCaseData(
				ChoiceJArrayFromNames($"{_NESTED_VALUE} Sick Choice"),
				$"'{_NESTED_VALUE} Sick Choice'")
			{
				TestName = "NestedValue - Singleton"
			};
			yield return new TestCaseData(
				ChoiceJArrayFromNames("Okay Name", $"Cool{_NESTED_VALUE} Name", "Awesome Name"),
				$"'Cool{_NESTED_VALUE} Name'")
			{
				TestName = "NestedValue - Single violating name in larger collection"
			};
			yield return new TestCaseData(
				ChoiceJArrayFromNames("Okay Name", $"Cool{_NESTED_VALUE} Name", $"Awesome{_NESTED_VALUE} Name"),
				$"'Cool{_NESTED_VALUE} Name', 'Awesome{_NESTED_VALUE} Name'")
			{
				TestName = "NestedValue - Many violating names in larger collection"
			};

			yield return new TestCaseData(
				ChoiceJArrayFromNames("Okay Name", $"Cool{_NESTED_VALUE} Name", $"Awesome{_MULTI_VALUE} Name"),
				$"'Cool{_NESTED_VALUE} Name', 'Awesome{_MULTI_VALUE} Name'")
			{
				TestName = "Combined - Many violating names in larger collection"
			};
		}

		private static IEnumerable<TestCaseData> ThrowExceptionWhenDeserializationFailsTestCases()
		{
			yield return new TestCaseData(1);
			yield return new TestCaseData("foo");
			yield return new TestCaseData(new object());
			yield return new TestCaseData(SanitizationTestUtils.DeserializeJson("{ \"not\": \"an array\" }"));
		}

		private static JArray ChoiceJArrayFromNames(params string[] names)
		{
			Choice[] choices = names.Select(x => new Choice { Name = x }).ToArray();
			return SanitizationTestUtils.ToJToken<JArray>(choices);
		}
	}
}
