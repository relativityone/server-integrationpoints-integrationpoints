using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
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

		[Test]
		public async Task ItShouldThrowSyncExceptionWhenEncounteringUnexpectedType()
		{
			// Arrange
			ISynchronizationConfiguration configuration = CreateConfigurationWithDefaultDelimiters();
			var instance = new MultipleChoiceFieldSanitizer(configuration);

			// Act
			const int initialValue = 10123232;
			Func<Task> action = async () =>
				await instance.SanitizeAsync(0, "foo", "bar", "baz", initialValue).ConfigureAwait(false);

			// Assert
			(await action.Should().ThrowAsync<SyncException>().ConfigureAwait(false))
				.Which.Message.Should()
				.Contain(typeof(Choice[]).Name).And
				.Contain(typeof(int).Name);
		}

		private static IEnumerable<TestCaseData> ThrowSyncExceptionWhenNameContainsDelimiterTestCases()
		{
			yield return new TestCaseData(ChoicesFromNames("; Sick Choice"), "'; Sick Choice'")
			{
				TestName = "MultiValue - Singleton"
			};
			yield return new TestCaseData(ChoicesFromNames("Okay Name", "Cool; Name", "Awesome Name"), "'Cool; Name'")
			{
				TestName = "MultiValue - Single violating name in larger collection"
			};
			yield return new TestCaseData(ChoicesFromNames("Okay Name", "Cool; Name", "Awesome; Name"), "'Cool; Name', 'Awesome; Name'")
			{
				TestName = "MultiValue - Many violating names in larger collection"
			};

			yield return new TestCaseData(ChoicesFromNames("/ Sick Choice"), "'/ Sick Choice'")
			{
				TestName = "NestedValue - Singleton"
			};
			yield return new TestCaseData(ChoicesFromNames("Okay Name", "Cool/ Name", "Awesome Name"), "'Cool/ Name'")
			{
				TestName = "NestedValue - Single violating name in larger collection"
			};
			yield return new TestCaseData(ChoicesFromNames("Okay Name", "Cool/ Name", "Awesome/ Name"), "'Cool/ Name', 'Awesome/ Name'")
			{
				TestName = "NestedValue - Many violating names in larger collection"
			};
			
			yield return new TestCaseData(ChoicesFromNames("Okay Name", "Cool/ Name", "Awesome; Name"), "'Cool/ Name', 'Awesome; Name'")
			{
				TestName = "Combined - Many violating names in larger collection"
			};
		}

		[TestCaseSource(nameof(ThrowSyncExceptionWhenNameContainsDelimiterTestCases))]
		public async Task ItShouldThrowSyncExceptionWhenNameContainsDelimiter(Choice[] initialValue,
			string expectedViolators)
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
			yield return new TestCaseData(ChoicesFromNames(), string.Empty)
			{
				TestName = "Empty"
			};
			yield return new TestCaseData(ChoicesFromNames("Sick Name"), "Sick Name")
			{
				TestName = "Single"
			};
			yield return new TestCaseData(ChoicesFromNames("Sick Name", "Cool Name", "Awesome Name"),
				"Sick Name;Cool Name;Awesome Name")
			{
				TestName = "Multiple"
			};

			// TODO: Add test cases for nested values
		}

		[TestCaseSource(nameof(CombineNamesIntoReturnValueTestCases))]
		public async Task ItShouldCombineNamesIntoReturnValue(Choice[] initialValue, string expectedResult)
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
			Choice[] initialValue = ChoicesFromNames("Test Name", "Cool Name");
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

		private static Choice[] ChoicesFromNames(params string[] names)
		{
			return names.Select(x => new Choice { Name = x }).ToArray();
		}
	}
}