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
	internal class MultipleObjectFieldSanitizerTests
	{
		private const char _MUTLI_DELIM = (char) 30;

		[Test]
		public void ItShouldSupportMultipleObject()
		{
			// Arrange
			ISynchronizationConfiguration configuration = CreateConfiguration();
			var instance = new MultipleObjectFieldSanitizer(configuration);

			// Act
			RelativityDataType supportedType = instance.SupportedType;

			// Assert
			supportedType.Should().Be(RelativityDataType.MultipleObject);
		}

		private static IEnumerable<TestCaseData> ThrowInvalidExportFieldValueExceptionWhenDeserializationFailsTestCases()
		{
			yield return new TestCaseData(1);
			yield return new TestCaseData("foo");
			yield return new TestCaseData(new object());
			yield return new TestCaseData(JsonHelpers.DeserializeJson("{ \"not\": \"an array\" }"));
		}

		[TestCaseSource(nameof(ThrowInvalidExportFieldValueExceptionWhenDeserializationFailsTestCases))]
		public async Task ItShouldThrowInvalidExportFieldValueExceptionWithTypeNamesWhenDeserializationFails(object initialValue)
		{
			// Arrange
			ISynchronizationConfiguration configuration = CreateConfiguration();
			var instance = new MultipleObjectFieldSanitizer(configuration);

			// Act
			Func<Task> action = async () =>
				await instance.SanitizeAsync(0, "foo", "bar", "baz", initialValue).ConfigureAwait(false);

			// Assert
			(await action.Should().ThrowAsync<InvalidExportFieldValueException>().ConfigureAwait(false))
				.Which.Message.Should()
					.Contain(typeof(RelativityObjectValue[]).Name).And
					.Contain(initialValue.GetType().Name);
		}

		[TestCaseSource(nameof(ThrowInvalidExportFieldValueExceptionWhenDeserializationFailsTestCases))]
		public async Task ItShouldThrowInvalidExportFieldValueExceptionWithInnerExceptionWhenDeserializationFails(object initialValue)
		{
			// Arrange
			ISynchronizationConfiguration configuration = CreateConfiguration();
			var instance = new MultipleObjectFieldSanitizer(configuration);

			// Act
			Func<Task> action = async () =>
				await instance.SanitizeAsync(0, "foo", "bar", "baz", initialValue).ConfigureAwait(false);

			// Assert
			(await action.Should().ThrowAsync<InvalidExportFieldValueException>().ConfigureAwait(false))
				.Which.InnerException.Should()
					.Match(ex => ex is JsonReaderException || ex is JsonSerializationException);
		}

		private static IEnumerable<TestCaseData> ThrowInvalidExportFieldValueExceptionWhenAnyElementsAreInvalidTestCases()
		{
			yield return new TestCaseData(JsonHelpers.DeserializeJson("[ { \"test\": 1 } ]"));
			yield return new TestCaseData(JsonHelpers.DeserializeJson("[ { \"ArtifactID\": 101, \"Name\": \"Cool Object\" }, { \"test\": 1 } ]"));
			yield return new TestCaseData(JsonHelpers.DeserializeJson("[ { \"ArtifactID\": 101, \"Name\": \"Cool Object\" }, { \"test\": 1 }, { \"ArtifactID\": 102, \"Name\": \"Cool Object 2\" } ]"));
		}

		[TestCaseSource(nameof(ThrowInvalidExportFieldValueExceptionWhenAnyElementsAreInvalidTestCases))]
		public async Task ItShouldThrowInvalidExportFieldValueExceptionWhenAnyElementsAreInvalid(object initialValue)
		{
			// Arrange
			ISynchronizationConfiguration configuration = CreateConfiguration();
			var instance = new MultipleObjectFieldSanitizer(configuration);

			// Act
			Func<Task> action = async () =>
				await instance.SanitizeAsync(0, "foo", "bar", "baz", initialValue).ConfigureAwait(false);

			// Assert
			(await action.Should().ThrowAsync<InvalidExportFieldValueException>().ConfigureAwait(false))
				.Which.Message.Should()
				.Contain(typeof(RelativityObjectValue).Name);
		}

		private static IEnumerable<TestCaseData> ThrowSyncExceptionWhenNameContainsMultiValueDelimiterTestCases()
		{
			yield return new TestCaseData(
				ObjectValueJArrayFromNames($"{_MUTLI_DELIM} Sick Name"),
				$"'{_MUTLI_DELIM} Sick Name'")
			{
				TestName = "Singleton violating name"
			};
			yield return new TestCaseData(
				ObjectValueJArrayFromNames("Okay Name", $"Cool{_MUTLI_DELIM} Name", "Awesome Name"),
				$"'Cool{_MUTLI_DELIM} Name'")
			{
				TestName = "Single violating name in larger collection"
			};
			yield return new TestCaseData(
				ObjectValueJArrayFromNames("Okay Name", $"Cool{_MUTLI_DELIM} Name", $"Awesome{_MUTLI_DELIM} Name"),
				$"'Cool{_MUTLI_DELIM} Name', 'Awesome{_MUTLI_DELIM} Name'")
			{
				TestName = "Many violating names in larger collection"
			};
		}

		[TestCaseSource(nameof(ThrowSyncExceptionWhenNameContainsMultiValueDelimiterTestCases))]
		public async Task ItShouldThrowSyncExceptionWhenNameContainsMultiValueDelimiter(object initialValue, string expectedViolators)
		{
			// Arrange
			ISynchronizationConfiguration configuration = CreateConfiguration();
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
			yield return new TestCaseData(ObjectValueJArrayFromNames("Sick Name", "Cool Name", "Awesome Name"),
				$"Sick Name{_MUTLI_DELIM}Cool Name{_MUTLI_DELIM}Awesome Name")
			{
				TestName = "Multiple"
			};
		}

		[TestCaseSource(nameof(CombineNamesIntoReturnValueTestCases))]
		public async Task ItShouldCombineNamesIntoReturnValue(object initialValue, string expectedResult)
		{
			// Arrange
			ISynchronizationConfiguration configuration = CreateConfiguration();
			var instance = new MultipleObjectFieldSanitizer(configuration);

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

		private static JArray ObjectValueJArrayFromNames(params string[] names)
		{
			RelativityObjectValue[] values = names.Select(x => new RelativityObjectValue { Name = x }).ToArray();
			return JsonHelpers.ToJToken<JArray>(values);
		}
	}
}
