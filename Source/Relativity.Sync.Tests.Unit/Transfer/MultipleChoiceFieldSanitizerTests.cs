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
		private const char _DEFAULT_MULTI_VALUE_DELIMITER = ';';
		private const char _DEFAULT_NESTED_VALUE_DELIMITER = '/';

		[Test]
		public void ItShouldSupportMultipleChoice()
		{
			// Arrange
			ISynchronizationConfiguration configuration = CreateConfigurationWithDefaultDelimiters();
			var instance = new MultipleChoiceFieldSanitizer(configuration);

			// Act
			RelativityDataType supportedType = instance.SupportedType;

			// Assert
			supportedType.Should().Be(RelativityDataType.MultipleChoice);
		}

		private static IEnumerable<TestCaseData> ThrowSyncExceptionWhenDeserializationFailsTestCases()
		{
			yield return new TestCaseData(1);
			yield return new TestCaseData("foo");
			yield return new TestCaseData(new object());
			yield return new TestCaseData(JsonHelpers.DeserializeJson("{ \"not\": \"an array\" }"));
		}

		[TestCaseSource(nameof(ThrowSyncExceptionWhenDeserializationFailsTestCases))]
		public async Task ItShouldThrowSyncExceptionWithTypeNamesWhenDeserializationFails(object initialValue)
		{
			// Arrange
			ISynchronizationConfiguration configuration = CreateConfigurationWithDefaultDelimiters();
			var instance = new MultipleChoiceFieldSanitizer(configuration);

			// Act
			Func<Task> action = async () =>
				await instance.SanitizeAsync(0, "foo", "bar", "baz", initialValue).ConfigureAwait(false);

			// Assert
			(await action.Should().ThrowAsync<SyncException>().ConfigureAwait(false))
				.Which.Message.Should()
					.Contain(typeof(Choice[]).Name).And
					.Contain(initialValue.GetType().Name);
		}

		[TestCaseSource(nameof(ThrowSyncExceptionWhenDeserializationFailsTestCases))]
		public async Task ItShouldThrowSyncExceptionWithInnerExceptionWhenDeserializationFails(object initialValue)
		{
			// Arrange
			ISynchronizationConfiguration configuration = CreateConfigurationWithDefaultDelimiters();
			var instance = new MultipleChoiceFieldSanitizer(configuration);

			// Act
			Func<Task> action = async () =>
				await instance.SanitizeAsync(0, "foo", "bar", "baz", initialValue).ConfigureAwait(false);

			// Assert
			(await action.Should().ThrowAsync<SyncException>().ConfigureAwait(false))
				.Which.InnerException.Should()
					.Match(ex => ex is JsonReaderException || ex is JsonSerializationException);
		}

		private static IEnumerable<TestCaseData> ThrowSyncExceptionIfAnyElementsAreInvalidTestCases()
		{
			yield return new TestCaseData(JsonHelpers.DeserializeJson("[ { \"test\": 1 } ]"));
			yield return new TestCaseData(JsonHelpers.DeserializeJson("[ { \"ArtifactID\": 101, \"Name\": \"Cool Choice\" }, { \"test\": 1 } ]"));
			yield return new TestCaseData(JsonHelpers.DeserializeJson("[ { \"ArtifactID\": 101, \"Name\": \"Cool Choice\" }, { \"test\": 1 }, { \"ArtifactID\": 102, \"Name\": \"Cool Choice 2\" } ]"));
		}

		[TestCaseSource(nameof(ThrowSyncExceptionIfAnyElementsAreInvalidTestCases))]
		public async Task ItShouldThrowSyncExceptionIfAnyElementsAreInvalid(object initialValue)
		{
			// Arrange
			ISynchronizationConfiguration configuration = CreateConfigurationWithDefaultDelimiters();
			var instance = new MultipleChoiceFieldSanitizer(configuration);

			// Act
			Func<Task> action = async () =>
				await instance.SanitizeAsync(0, "foo", "bar", "baz", initialValue).ConfigureAwait(false);

			// Assert
			(await action.Should().ThrowAsync<SyncException>().ConfigureAwait(false))
				.Which.Message.Should()
				.Contain(typeof(Choice).Name);
		}

		private static IEnumerable<TestCaseData> ThrowSyncExceptionWhenNameContainsDelimiterTestCases()
		{
			yield return new TestCaseData(ChoiceJArrayFromNames("; Sick Choice"), "'; Sick Choice'")
			{
				TestName = "MultiValue - Singleton"
			};
			yield return new TestCaseData(ChoiceJArrayFromNames("Okay Name", "Cool; Name", "Awesome Name"), "'Cool; Name'")
			{
				TestName = "MultiValue - Single violating name in larger collection"
			};
			yield return new TestCaseData(ChoiceJArrayFromNames("Okay Name", "Cool; Name", "Awesome; Name"), "'Cool; Name', 'Awesome; Name'")
			{
				TestName = "MultiValue - Many violating names in larger collection"
			};

			yield return new TestCaseData(ChoiceJArrayFromNames("/ Sick Choice"), "'/ Sick Choice'")
			{
				TestName = "NestedValue - Singleton"
			};
			yield return new TestCaseData(ChoiceJArrayFromNames("Okay Name", "Cool/ Name", "Awesome Name"), "'Cool/ Name'")
			{
				TestName = "NestedValue - Single violating name in larger collection"
			};
			yield return new TestCaseData(ChoiceJArrayFromNames("Okay Name", "Cool/ Name", "Awesome/ Name"), "'Cool/ Name', 'Awesome/ Name'")
			{
				TestName = "NestedValue - Many violating names in larger collection"
			};
			
			yield return new TestCaseData(ChoiceJArrayFromNames("Okay Name", "Cool/ Name", "Awesome; Name"), "'Cool/ Name', 'Awesome; Name'")
			{
				TestName = "Combined - Many violating names in larger collection"
			};
		}

		[TestCaseSource(nameof(ThrowSyncExceptionWhenNameContainsDelimiterTestCases))]
		public async Task ItShouldThrowSyncExceptionWhenNameContainsDelimiter(object initialValue, string expectedViolators)
		{
			// Arrange
			ISynchronizationConfiguration configuration = CreateConfigurationWithDefaultDelimiters();
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
			yield return new TestCaseData(ChoiceJArrayFromNames("Sick Name", "Cool Name", "Awesome Name"),
				"Sick Name;Cool Name;Awesome Name")
			{
				TestName = "Multiple"
			};

			// TODO: Add test cases for nested values
		}

		[TestCaseSource(nameof(CombineNamesIntoReturnValueTestCases))]
		public async Task ItShouldCombineNamesIntoReturnValue(object initialValue, object expectedResult)
		{
			// Arrange
			ISynchronizationConfiguration configuration = CreateConfigurationWithDefaultDelimiters();
			var instance = new MultipleChoiceFieldSanitizer(configuration);

			// Act
			object result = await instance.SanitizeAsync(0, "foo", "bar", "baz", initialValue).ConfigureAwait(false);

			// Assert
			result.Should().Be(expectedResult);
		}

		private static IEnumerable<TestCaseData> ReadDelimitersFromConfigurationTestCases()
		{
			const int numCases = 10;

			// We avoid 0 (null), 32 (space) and the uppercase Latin alphabet range (starts at 65).
			const int minMultiValueChar = 1;
			const int maxMultiValueChar = 31;
			const int minNestedValueChar = 33;
			const int maxNestedValueChar = 64;
			Random rng = new Random();

			return Enumerable.Range(1, numCases)
				.Select(_ => new TestCaseData(
					(char) rng.Next(minMultiValueChar, maxMultiValueChar + 1),
					(char) rng.Next(minNestedValueChar, maxNestedValueChar + 1)));
		}

		[TestCaseSource(nameof(ReadDelimitersFromConfigurationTestCases))]
		public async Task ItShouldReadDelimitersFromConfiguration(char multiValueDelimiter, char nestedValueDelimiter)
		{
			// Arrange
			ISynchronizationConfiguration configuration =
				CreateConfigurationWithDelimiters(multiValueDelimiter, nestedValueDelimiter);
			var instance = new MultipleChoiceFieldSanitizer(configuration);

			// Act
			JArray initialValue = ChoiceJArrayFromNames("Test Name", "Cool Name");
			object result = await instance.SanitizeAsync(0, "foo", "bar", "baz", initialValue).ConfigureAwait(false);

			// Assert
			// TODO: Test nested value composition
			result.Should().Be($"Test Name{multiValueDelimiter}Cool Name");
		}

		private static ISynchronizationConfiguration CreateConfigurationWithDefaultDelimiters()
		{
			return CreateConfigurationWithDelimiters(_DEFAULT_MULTI_VALUE_DELIMITER, _DEFAULT_NESTED_VALUE_DELIMITER);
		}

		private static ISynchronizationConfiguration CreateConfigurationWithDelimiters(char multiValueDelimiter,
			char nestedValueDelimiter)
		{
			var config = new Mock<ISynchronizationConfiguration>();
			var importSettings = new ImportSettingsDto { MultiValueDelimiter = multiValueDelimiter, NestedValueDelimiter = nestedValueDelimiter };
			config.SetupGet(x => x.ImportSettings).Returns(importSettings);
			return config.Object;
		}

		private static JArray ChoiceJArrayFromNames(params string[] names)
		{
			Choice[] choices = names.Select(x => new Choice {Name = x}).ToArray();
			return JsonHelpers.ToJToken<JArray>(choices);
		}
	}
}