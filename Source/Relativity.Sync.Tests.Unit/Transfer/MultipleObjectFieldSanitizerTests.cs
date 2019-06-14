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
	internal class MultipleObjectFieldSanitizerTests
	{
		private const char _DEFAULT_MULTI_VALUE_DELIMITER = ';';

		[Test]
		public void ItShouldSupportMultipleObject()
		{
			// Arrange
			ISynchronizationConfiguration configuration = CreateConfigurationWithDelimiter(_DEFAULT_MULTI_VALUE_DELIMITER);
			var instance = new MultipleObjectFieldSanitizer(configuration);

			// Act
			RelativityDataType supportedType = instance.SupportedType;

			// Assert
			supportedType.Should().Be(RelativityDataType.MultipleObject);
		}

		[Test]
		public async Task ItShouldThrowSyncExceptionWhenEncounteringUnexpectedType()
		{
			// Arrange
			ISynchronizationConfiguration configuration = CreateConfigurationWithDelimiter(_DEFAULT_MULTI_VALUE_DELIMITER);
			var instance = new MultipleObjectFieldSanitizer(configuration);

			// Act
			const int initialValue = 1012323;
			Func<Task> action = async () => await instance.SanitizeAsync(0, "foo", "bar", "baz", initialValue).ConfigureAwait(false);

			// Assert
			(await action.Should().ThrowAsync<SyncException>().ConfigureAwait(false))
				.Which.Message.Should()
					.Contain(typeof(RelativityObjectValue[]).Name).And
					.Contain(typeof(int).Name);
		}

		private static IEnumerable<TestCaseData> ThrowSyncExceptionWhenNameContainsMultiValueDelimiterTestCases()
		{
			yield return new TestCaseData(ObjectValuesFromNames("; Sick Name"), "'; Sick Name'")
			{
				TestName = "Singleton violating name"
			};
			yield return new TestCaseData(ObjectValuesFromNames("Okay Name", "Cool; Name", "Awesome Name"), "'Cool; Name'")
			{
				TestName = "Single violating name in larger collection"
			};
			yield return new TestCaseData(ObjectValuesFromNames("Okay Name", "Cool; Name", "Awesome; Name"), "'Cool; Name', 'Awesome; Name'")
			{
				TestName = "Many violating names in larger collection"
			};
		}

		[TestCaseSource(nameof(ThrowSyncExceptionWhenNameContainsMultiValueDelimiterTestCases))]
		public async Task ItShouldThrowSyncExceptionWhenNameContainsMultiValueDelimiter(RelativityObjectValue[] initialValue, string expectedViolators)
		{
			// Arrange
			ISynchronizationConfiguration configuration = CreateConfigurationWithDelimiter(_DEFAULT_MULTI_VALUE_DELIMITER);
			var instance = new MultipleObjectFieldSanitizer(configuration);

			// Act
			Func<Task> action = async () => await instance.SanitizeAsync(0, "foo", "bar", "baz", initialValue).ConfigureAwait(false);

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
			yield return new TestCaseData(ObjectValuesFromNames(), string.Empty)
			{
				TestName = "Empty"
			};
			yield return new TestCaseData(ObjectValuesFromNames("Sick Name"), "Sick Name")
			{
				TestName = "Single"
			};
			yield return new TestCaseData(ObjectValuesFromNames("Sick Name", "Cool Name", "Awesome Name"), "Sick Name;Cool Name;Awesome Name")
			{
				TestName = "Multiple"
			};
		}

		[TestCaseSource(nameof(CombineNamesIntoReturnValueTestCases))]
		public async Task ItShouldCombineNamesIntoReturnValue(RelativityObjectValue[] initialValue, string expectedResult)
		{
			// Arrange
			ISynchronizationConfiguration configuration = CreateConfigurationWithDelimiter(_DEFAULT_MULTI_VALUE_DELIMITER);
			var instance = new MultipleObjectFieldSanitizer(configuration);

			// Act
			object result = await instance.SanitizeAsync(0, "foo", "bar", "baz", initialValue).ConfigureAwait(false);

			// Assert
			result.Should().Be(expectedResult);
		}

		private static IEnumerable<TestCaseData> ReadMultiValueDelimiterFromConfigurationTestCases()
		{
			const int numCases = 10;
			
			// We avoid 0 (null) and 32 (space).
			const int minChar = 1;
			const int maxChar = 31;
			Random rng = new Random();

			return Enumerable.Range(1, numCases)
				.Select(_ => new TestCaseData(
					(char) rng.Next(minChar, maxChar + 1)));
		}

		[TestCaseSource(nameof(ReadMultiValueDelimiterFromConfigurationTestCases))]
		public async Task ItShouldReadMultiValueDelimiterFromConfiguration(char delimiter)
		{
			// Arrange
			ISynchronizationConfiguration configuration = CreateConfigurationWithDelimiter(delimiter);
			var instance = new MultipleObjectFieldSanitizer(configuration);

			// Act
			RelativityObjectValue[] initialValue = ObjectValuesFromNames("Test Name", "Cool Name");
			object result = await instance.SanitizeAsync(0, "foo", "bar", "baz", initialValue).ConfigureAwait(false);

			// Assert
			result.Should().Be($"Test Name{delimiter}Cool Name");
		}

		private static ISynchronizationConfiguration CreateConfigurationWithDelimiter(char multiValueDelimiter)
		{
			var config = new Mock<ISynchronizationConfiguration>();
			var importSettings = new ImportSettingsDto { MultiValueDelimiter = multiValueDelimiter };
			config.SetupGet(x => x.ImportSettings).Returns(importSettings);
			return config.Object;
		}

		private static RelativityObjectValue[] ObjectValuesFromNames(params string[] names)
		{
			return names.Select(x => new RelativityObjectValue { Name = x }).ToArray();
		}
	}
}
