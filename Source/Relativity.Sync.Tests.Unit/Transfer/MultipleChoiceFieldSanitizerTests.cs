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
	internal class MultipleChoiceFieldSanitizerTests
	{
		private const char _NESTED_VALUE = (char) 29;
		private const char _MULTI_VALUE = (char) 30;

		[Test]
		public void ItShouldSupportMultipleChoice()
		{
			// Arrange
			ISynchronizationConfiguration configuration = CreateConfiguration();
			var instance = new MultipleChoiceFieldSanitizer(configuration);

			// Act
			RelativityDataType supportedType = instance.SupportedType;

			// Assert
			supportedType.Should().Be(RelativityDataType.MultipleChoice);
		}

		private static IEnumerable<TestCaseData> ThrowExceptionWhenDeserializationFailsTestCases()
		{
			yield return new TestCaseData(1);
			yield return new TestCaseData("foo");
			yield return new TestCaseData(new object());
			yield return new TestCaseData(JsonHelpers.DeserializeJson("{ \"not\": \"an array\" }"));
		}

		[TestCaseSource(nameof(ThrowExceptionWhenDeserializationFailsTestCases))]
		public async Task ItShouldThrowInvalidExportFieldValueExceptionWithTypeNamesWhenDeserializationFails(object initialValue)
		{
			// Arrange
			ISynchronizationConfiguration configuration = CreateConfiguration();
			var instance = new MultipleChoiceFieldSanitizer(configuration);

			// Act
			Func<Task> action = async () =>
				await instance.SanitizeAsync(0, "foo", "bar", "baz", initialValue).ConfigureAwait(false);

			// Assert
			(await action.Should().ThrowAsync<InvalidExportFieldValueException>().ConfigureAwait(false))
				.Which.Message.Should()
					.Contain(typeof(Choice[]).Name).And
					.Contain(initialValue.GetType().Name);
		}

		[TestCaseSource(nameof(ThrowExceptionWhenDeserializationFailsTestCases))]
		public async Task ItShouldThrowInvalidExportFieldValueExceptionWithInnerExceptionWhenDeserializationFails(object initialValue)
		{
			// Arrange
			ISynchronizationConfiguration configuration = CreateConfiguration();
			var instance = new MultipleChoiceFieldSanitizer(configuration);

			// Act
			Func<Task> action = async () =>
				await instance.SanitizeAsync(0, "foo", "bar", "baz", initialValue).ConfigureAwait(false);

			// Assert
			(await action.Should().ThrowAsync<InvalidExportFieldValueException>().ConfigureAwait(false))
				.Which.InnerException.Should()
					.Match(ex => ex is JsonReaderException || ex is JsonSerializationException);
		}

		private static IEnumerable<TestCaseData> ThrowExceptionWhenAnyElementsAreInvalidTestCases()
		{
			yield return new TestCaseData(JsonHelpers.DeserializeJson("[ { \"test\": 1 } ]"));
			yield return new TestCaseData(JsonHelpers.DeserializeJson("[ { \"ArtifactID\": 101, \"Name\": \"Cool Choice\" }, { \"test\": 1 } ]"));
			yield return new TestCaseData(JsonHelpers.DeserializeJson("[ { \"ArtifactID\": 101, \"Name\": \"Cool Choice\" }, { \"test\": 1 }, { \"ArtifactID\": 102, \"Name\": \"Cool Choice 2\" } ]"));
		}

		[TestCaseSource(nameof(ThrowExceptionWhenAnyElementsAreInvalidTestCases))]
		public async Task ItShouldThrowInvalidExportFieldValueExceptionWhenAnyElementsAreInvalid(object initialValue)
		{
			// Arrange
			ISynchronizationConfiguration configuration = CreateConfiguration();
			var instance = new MultipleChoiceFieldSanitizer(configuration);

			// Act
			Func<Task> action = async () =>
				await instance.SanitizeAsync(0, "foo", "bar", "baz", initialValue).ConfigureAwait(false);

			// Assert
			(await action.Should().ThrowAsync<InvalidExportFieldValueException>().ConfigureAwait(false))
				.Which.Message.Should()
				.Contain(typeof(Choice).Name);
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

		[TestCaseSource(nameof(ThrowSyncExceptionWhenNameContainsDelimiterTestCases))]
		public async Task ItShouldThrowSyncExceptionWhenNameContainsDelimiter(object initialValue, string expectedViolators)
		{
			// Arrange
			ISynchronizationConfiguration configuration = CreateConfiguration();
			var instance = new MultipleChoiceFieldSanitizer(configuration);

			// Act
			Func<Task> action = async () =>
				await instance.SanitizeAsync(0, "foo", "bar", "baz", initialValue).ConfigureAwait(false);

			// Assert
			(await action.Should().ThrowAsync<SyncException>().ConfigureAwait(false))
				.Which.Message.Should().MatchRegex($" {expectedViolators}$");
		}

		private static IEnumerable<TestCaseData> CombineNamesIntoReturnValueTestCases()
		{
			yield return new TestCaseData(null, null)
			{
				TestName = "Null"
			};
			yield return new TestCaseData(ChoiceJArrayFromNames(), string.Empty)
			{
				TestName = "Empty"
			};
			yield return new TestCaseData(ChoiceJArrayFromNames("Sick Name"), "Sick Name")
			{
				TestName = "Single"
			};
			yield return new TestCaseData(
				ChoiceJArrayFromNames("Sick Name", "Cool Name", "Awesome Name"),
				$"Sick Name{_MULTI_VALUE}Cool Name{_MULTI_VALUE}Awesome Name")
			{
				TestName = "Multiple"
			};

			// TODO: Add test cases for nested values
		}

		[TestCaseSource(nameof(CombineNamesIntoReturnValueTestCases))]
		public async Task ItShouldCombineNamesIntoReturnValue(object initialValue, object expectedResult)
		{
			// Arrange
			ISynchronizationConfiguration configuration = CreateConfiguration();
			var instance = new MultipleChoiceFieldSanitizer(configuration);

			// Act
			object result = await instance.SanitizeAsync(0, "foo", "bar", "baz", initialValue).ConfigureAwait(false);

			// Assert
			result.Should().Be(expectedResult);
		}

		private static ISynchronizationConfiguration CreateConfiguration()
		{
			var config = new Mock<ISynchronizationConfiguration>();
			config.SetupGet(x => x.ImportSettings).Returns(new ImportSettingsDto());
			return config.Object;
		}

		private static JArray ChoiceJArrayFromNames(params string[] names)
		{
			Choice[] choices = names.Select(x => new Choice {Name = x}).ToArray();
			return JsonHelpers.ToJToken<JArray>(choices);
		}
	}
}