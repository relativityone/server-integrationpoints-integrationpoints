using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Services.Exporter.Sanitization;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.DTO;
using kCura.IntegrationPoints.Domain.Exceptions;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Relativity;

namespace kCura.IntegrationPoints.Core.Tests.Services.Exporter.Sanitization
{
    [TestFixture, Category("Unit")]
    internal class MultipleChoiceFieldSanitizerTests
    {
        private Mock<IChoiceRepository> _choiceCache;
        private Mock<IChoiceTreeToStringConverter> _choiceTreeToStringConverter;
        private Mock<ISanitizationDeserializer> _sanitizationHelper;
        private MultipleChoiceFieldSanitizer _sut;

        private const char _NESTED_VALUE = IntegrationPoints.Domain.Constants.NESTED_VALUE_DELIMITER;
        private const char _MULTI_VALUE = IntegrationPoints.Domain.Constants.MULTI_VALUE_DELIMITER;

        [SetUp]
        public void SetUp()
        {
            _choiceCache = new Mock<IChoiceRepository>();
            _choiceTreeToStringConverter = new Mock<IChoiceTreeToStringConverter>();
            _sanitizationHelper = new Mock<ISanitizationDeserializer>();
            var jsonSerializer = new JSONSerializer();
            _sanitizationHelper
                .Setup(x => x.DeserializeAndValidateExportFieldValue<ChoiceDto[]>(It.IsAny<object>()))
                .Returns((object serializedObject) =>
                    jsonSerializer.Deserialize<ChoiceDto[]>(serializedObject.ToString()));
            _sut = new MultipleChoiceFieldSanitizer(
                _choiceCache.Object, 
                _choiceTreeToStringConverter.Object,
                _sanitizationHelper.Object);
        }

        [Test]
        public void ItShouldSupportMultipleChoice()
        {
            // Act
            FieldTypeHelper.FieldType supportedType = _sut.SupportedType;

            // Assert
            supportedType.Should().Be(FieldTypeHelper.FieldType.MultiCode);
        }

        [TestCaseSource(nameof(ThrowExceptionWhenAnyElementsAreInvalidTestCases))]
        public void ItShouldThrowInvalidExportFieldValueExceptionWhenAnyElementsAreInvalid(object initialValue)
        {
            // Act
            Func<Task> action = () => _sut.SanitizeAsync(0, "foo", "bar", "baz", initialValue);

            // Assert
            action.ShouldThrow<InvalidExportFieldValueException>();
        }

        [TestCaseSource(nameof(ThrowIPExceptionWhenNameContainsDelimiterTestCases))]
        public void ItShouldThrowIPExceptionWhenNameContainsDelimiter(object initialValue)
        {
            // Act
            Func<Task> action = () => _sut.SanitizeAsync(0, "foo", "bar", "baz", initialValue);

            // Assert
            action.ShouldThrow<IntegrationPointsException>()
                .Which.Message.Should().Be("Unable to parse data from Relativity Export API: " +
                                           "The identifiers of the choices contain the character specified as the" +
                                           " multi-value delimiter ('ASCII 59') or nested value delimiter ('ASCII 47'). " +
                                           "Rename choices to not contain delimiters.");
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
            _choiceCache
                .Setup(
                    x => x.QueryChoiceWithParentInfoAsync(It.IsAny<ICollection<ChoiceDto>>(), It.IsAny<ICollection<ChoiceDto>>()))
                .ReturnsAsync(new List<ChoiceWithParentInfoDto>());
            _choiceTreeToStringConverter.Setup(x => x.ConvertTreeToString(It.IsAny<IList<ChoiceWithChildInfoDto>>())).Returns(string.Empty);

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

        private static IEnumerable<TestCaseData> ThrowIPExceptionWhenNameContainsDelimiterTestCases()
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

        private static JArray ChoiceJArrayFromNames(params string[] names)
        {
            ChoiceDto[] choices = names.Select(x => new ChoiceDto(artifactID: 0, name: x)).ToArray();
            return SanitizationTestUtils.ToJToken<JArray>(choices);
        }
    }
}
