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
    internal class MultipleChoiceFieldSanitizerTests
    {
        private Mock<IDocumentSynchronizationConfiguration> _config;
        private Mock<IChoiceCache> _choiceCache;
        private Mock<IChoiceTreeToStringConverter> _choiceTreeToStringConverter;
        private MultipleChoiceFieldSanitizer _instance;

        private const char _NESTED_VALUE = (char)29;
        private const char _MULTI_VALUE = (char)30;

        [SetUp]
        public void SetUp()
        {
            _config = new Mock<IDocumentSynchronizationConfiguration>();
            _config.SetupGet(x => x.NestedValueDelimiter).Returns(_NESTED_VALUE);
            _config.SetupGet(x => x.MultiValueDelimiter).Returns(_MULTI_VALUE);
            _choiceCache = new Mock<IChoiceCache>();
            _choiceTreeToStringConverter = new Mock<IChoiceTreeToStringConverter>();
            _instance = new MultipleChoiceFieldSanitizer(_config.Object, _choiceCache.Object, _choiceTreeToStringConverter.Object);
        }

        [Test]
        public void ItShouldSupportMultipleChoice()
        {
            // Act
            RelativityDataType supportedType = _instance.SupportedType;

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
            // Act
            Func<Task> action = () => _instance.SanitizeAsync(0, "foo", "bar", "baz", initialValue);

            // Assert
            (await action.Should().ThrowAsync<InvalidExportFieldValueException>().ConfigureAwait(false))
                .Which.Message.Should()
                    .Contain(typeof(Choice[]).Name).And
                    .Contain(initialValue.GetType().Name);
        }

        [TestCaseSource(nameof(ThrowExceptionWhenDeserializationFailsTestCases))]
        public async Task ItShouldThrowInvalidExportFieldValueExceptionWithInnerExceptionWhenDeserializationFails(object initialValue)
        {
            // Act
            Func<Task> action = () => _instance.SanitizeAsync(0, "foo", "bar", "baz", initialValue);

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
            // Act
            Func<Task> action = () => _instance.SanitizeAsync(0, "foo", "bar", "baz", initialValue);

            // Assert
            await action.Should().ThrowAsync<InvalidExportFieldValueException>().ConfigureAwait(false);
        }

        private static IEnumerable<TestCaseData> ThrowSyncExceptionWhenNameContainsDelimiterTestCases()
        {
            yield return new TestCaseData(ChoiceJArrayFromNames($"{_MULTI_VALUE} Sick Choice"))
            {
                TestName = "MultiValue - Singleton"
            };
            yield return new TestCaseData(ChoiceJArrayFromNames("Okay Name", $"Cool{_MULTI_VALUE} Name", "Awesome Name"))
            {
                TestName = "MultiValue - Single violating name in larger collection"
            };
            yield return new TestCaseData(ChoiceJArrayFromNames("Okay Name", $"Cool{_MULTI_VALUE} Name", $"Awesome{_MULTI_VALUE} Name"))
            {
                TestName = "MultiValue - Many violating names in larger collection"
            };

            yield return new TestCaseData(ChoiceJArrayFromNames($"{_NESTED_VALUE} Sick Choice"))
            {
                TestName = "NestedValue - Singleton"
            };
            yield return new TestCaseData(ChoiceJArrayFromNames("Okay Name", $"Cool{_NESTED_VALUE} Name", "Awesome Name"))
            {
                TestName = "NestedValue - Single violating name in larger collection"
            };
            yield return new TestCaseData(ChoiceJArrayFromNames("Okay Name", $"Cool{_NESTED_VALUE} Name", $"Awesome{_NESTED_VALUE} Name"))
            {
                TestName = "NestedValue - Many violating names in larger collection"
            };

            yield return new TestCaseData(ChoiceJArrayFromNames("Okay Name", $"Cool{_NESTED_VALUE} Name", $"Awesome{_MULTI_VALUE} Name"))
            {
                TestName = "Combined - Many violating names in larger collection"
            };
        }

        [TestCaseSource(nameof(ThrowSyncExceptionWhenNameContainsDelimiterTestCases))]
        public async Task ItShouldThrowSyncExceptionWhenNameContainsDelimiter(object initialValue)
        {
            // Act
            Func<Task> action = () => _instance.SanitizeAsync(0, "foo", "bar", "baz", initialValue);

            // Assert
            (await action.Should().ThrowAsync<InvalidExportFieldValueException>().ConfigureAwait(false))
                .Which.Message.Should().Be("Unable to parse data from Relativity Export API: " +
                                           "The identifiers of the choices contain the character specified as the" +
                                           " multi-value delimiter ('ASCII 30') or nested value delimiter ('ASCII 29'). " +
                                           "Rename choices to not contain delimiters.");
        }

        [Test]
        public async Task ItShouldReturnNull()
        {
            // Act
            object result = await _instance.SanitizeAsync(0, "foo", "bar", "baz", null).ConfigureAwait(false);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task ItShouldReturnEmptyString()
        {
            _choiceCache.Setup(x => x.GetChoicesWithParentInfoAsync(It.IsAny<ICollection<Choice>>())).ReturnsAsync(new List<ChoiceWithParentInfo>());
            _choiceTreeToStringConverter.Setup(x => x.ConvertTreeToString(It.IsAny<IList<ChoiceWithChildInfo>>())).Returns(string.Empty);

            // Act
            object result = await _instance.SanitizeAsync(0, "foo", "bar", "baz", ChoiceJArrayFromNames().ToString()).ConfigureAwait(false);

            // Assert
            result.Should().Be(string.Empty);
        }

        private static JArray ChoiceJArrayFromNames(params string[] names)
        {
            Choice[] choices = names.Select(x => new Choice { Name = x }).ToArray();
            return JsonHelpers.ToJToken<JArray>(choices);
        }
    }
}
