using System;
using System.Collections.Generic;
using System.IO;
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
			ISynchronizationConfiguration configuration = CreateConfigurationWithDelimiter(_DEFAULT_MULTI_VALUE_DELIMITER);
			var instance = new MultipleObjectFieldSanitizer(configuration);

			// Act
			Func<Task> action = async () =>
				await instance.SanitizeAsync(0, "foo", "bar", "baz", initialValue).ConfigureAwait(false);

			// Assert
			(await action.Should().ThrowAsync<SyncException>().ConfigureAwait(false))
				.Which.Message.Should()
					.Contain(typeof(RelativityObjectValue[]).Name).And
					.Contain(initialValue.GetType().Name);
		}

		[TestCaseSource(nameof(ThrowSyncExceptionWhenDeserializationFailsTestCases))]
		public async Task ItShouldThrowSyncExceptionWithInnerExceptionWhenDeserializationFails(object initialValue)
		{
			// Arrange
			ISynchronizationConfiguration configuration = CreateConfigurationWithDelimiter(_DEFAULT_MULTI_VALUE_DELIMITER);
			var instance = new MultipleObjectFieldSanitizer(configuration);

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
			yield return new TestCaseData(JsonHelpers.DeserializeJson("[ { \"ArtifactID\": 101, \"Name\": \"Cool Object\" }, { \"test\": 1 } ]"));
			yield return new TestCaseData(JsonHelpers.DeserializeJson("[ { \"ArtifactID\": 101, \"Name\": \"Cool Object\" }, { \"test\": 1 }, { \"ArtifactID\": 102, \"Name\": \"Cool Object 2\" } ]"));
		}

		[TestCaseSource(nameof(ThrowSyncExceptionIfAnyElementsAreInvalidTestCases))]
		public async Task ItShouldThrowSyncExceptionIfAnyElementsAreInvalid(object initialValue)
		{
			// Arrange
			ISynchronizationConfiguration configuration = CreateConfigurationWithDelimiter(_DEFAULT_MULTI_VALUE_DELIMITER);
			var instance = new MultipleObjectFieldSanitizer(configuration);

			// Act
			Func<Task> action = async () =>
				await instance.SanitizeAsync(0, "foo", "bar", "baz", initialValue).ConfigureAwait(false);

			// Assert
			(await action.Should().ThrowAsync<SyncException>().ConfigureAwait(false))
				.Which.Message.Should()
				.Contain(typeof(RelativityObjectValue).Name);
		}

		private static IEnumerable<TestCaseData> ThrowSyncExceptionWhenNameContainsMultiValueDelimiterTestCases()
		{
			yield return new TestCaseData(ObjectValueJArrayFromNames("; Sick Name"), "'; Sick Name'")
			{
				TestName = "Singleton violating name"
			};
			yield return new TestCaseData(ObjectValueJArrayFromNames("Okay Name", "Cool; Name", "Awesome Name"), "'Cool; Name'")
			{
				TestName = "Single violating name in larger collection"
			};
			yield return new TestCaseData(ObjectValueJArrayFromNames("Okay Name", "Cool; Name", "Awesome; Name"), "'Cool; Name', 'Awesome; Name'")
			{
				TestName = "Many violating names in larger collection"
			};
		}

		[TestCaseSource(nameof(ThrowSyncExceptionWhenNameContainsMultiValueDelimiterTestCases))]
		public async Task ItShouldThrowSyncExceptionWhenNameContainsMultiValueDelimiter(object initialValue, string expectedViolators)
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
			yield return new TestCaseData(ObjectValueJArrayFromNames(), string.Empty)
			{
				TestName = "Empty"
			};
			yield return new TestCaseData(ObjectValueJArrayFromNames("Sick Name"), "Sick Name")
			{
				TestName = "Single"
			};
			yield return new TestCaseData(ObjectValueJArrayFromNames("Sick Name", "Cool Name", "Awesome Name"), "Sick Name;Cool Name;Awesome Name")
			{
				TestName = "Multiple"
			};
		}

		[TestCaseSource(nameof(CombineNamesIntoReturnValueTestCases))]
		public async Task ItShouldCombineNamesIntoReturnValue(object initialValue, string expectedResult)
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
			object initialValue = ObjectValueJArrayFromNames("Test Name", "Cool Name");
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

		private static JArray ObjectValueJArrayFromNames(params string[] names)
		{
			RelativityObjectValue[] values = names.Select(x => new RelativityObjectValue { Name = x }).ToArray();
			return JsonHelpers.ToJToken<JArray>(values);
		}
	}
}
