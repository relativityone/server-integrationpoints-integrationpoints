using System;
using System.Collections.Generic;
using FluentAssertions;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Services.Exporter.Sanitization;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Core.Tests.Services.Exporter.Sanitization
{
    [TestFixture, Category("Unit")]
    internal class SanitizationHelperTests
    {
        private Mock<ISerializer> _serializerMock;
        private JSONSerializer _jsonSerializer;
        private ISanitizationDeserializer _sut;

        [SetUp]
        public void SetUp()
        {
            _serializerMock = new Mock<ISerializer>();
            _jsonSerializer = new JSONSerializer();
            SetupSerializerMock<Choice[]>();
            SetupSerializerMock<RelativityObjectValue[]>();
            SetupSerializerMock<Choice>();
            SetupSerializerMock<RelativityObjectValue>();

            _sut = new SanitizationDeserializer(_serializerMock.Object);
        }

        private void SetupSerializerMock<T>()
        {
            _serializerMock
                .Setup(x => x.Deserialize<T>(It.IsAny<string>()))
                .Returns((string serializedString) =>
                    _jsonSerializer.Deserialize<T>(serializedString));
        }

// public void ItShouldReturnProperResultWhenDeserializationSucceedsForChoiceArray()

        [TestCaseSource(nameof(ThrowExceptionWhenDeserializationFailsTestCasesForArray))]
        public void ItShouldThrowInvalidExportFieldValueExceptionWithTypeNamesWhenDeserializationFailsForChoiceArray(object initialValue)
        {
            ItShouldThrowInvalidExportFieldValueExceptionWithTypeNamesWhenDeserializationFails<Choice[]>(initialValue);
        }

        [TestCaseSource(nameof(ThrowExceptionWhenDeserializationFailsTestCasesForArray))]
        public void ItShouldThrowInvalidExportFieldValueExceptionWithInnerExceptionWhenDeserializationFailsForChoiceArray(object initialValue)
        {
            ItShouldThrowInvalidExportFieldValueExceptionWithInnerExceptionWhenDeserializationFails<Choice[]>(initialValue);
        }

        [TestCaseSource(nameof(ThrowExceptionWhenDeserializationFailsTestCasesForArray))]
        public void ItShouldThrowInvalidExportFieldValueExceptionWithTypeNamesWhenDeserializationFailsForRelativityObjectValueArray(object initialValue)
        {
            ItShouldThrowInvalidExportFieldValueExceptionWithTypeNamesWhenDeserializationFails<RelativityObjectValue[]>(initialValue);
        }

        [TestCaseSource(nameof(ThrowExceptionWhenDeserializationFailsTestCasesForArray))]
        public void ItShouldThrowInvalidExportFieldValueExceptionWithInnerExceptionWhenDeserializationFailsForRelativityObjectValueArray(object initialValue)
        {
            ItShouldThrowInvalidExportFieldValueExceptionWithInnerExceptionWhenDeserializationFails<RelativityObjectValue[]>(initialValue);
        }

        [TestCaseSource(nameof(ThrowExceptionWhenDeserializationFailsTestCasesForObject))]
        public void ItShouldThrowInvalidExportFieldValueExceptionWithTypeNamesWhenDeserializationFailsForChoice(object initialValue)
        {
            ItShouldThrowInvalidExportFieldValueExceptionWithTypeNamesWhenDeserializationFails<Choice>(initialValue);
        }

        [TestCaseSource(nameof(ThrowExceptionWhenDeserializationFailsTestCasesForObject))]
        public void ItShouldThrowInvalidExportFieldValueExceptionWithInnerExceptionWhenDeserializationFailsForChoice(object initialValue)
        {
            ItShouldThrowInvalidExportFieldValueExceptionWithInnerExceptionWhenDeserializationFails<Choice>(initialValue);
        }

        [TestCaseSource(nameof(ThrowExceptionWhenDeserializationFailsTestCasesForObject))]
        public void ItShouldThrowInvalidExportFieldValueExceptionWithTypeNamesWhenDeserializationFailsForRelativityObjectValue(object initialValue)
        {
            ItShouldThrowInvalidExportFieldValueExceptionWithTypeNamesWhenDeserializationFails<Choice>(initialValue);
        }

        [TestCaseSource(nameof(ThrowExceptionWhenDeserializationFailsTestCasesForObject))]
        public void ItShouldThrowInvalidExportFieldValueExceptionWithInnerExceptionWhenDeserializationFailsForRelativityObjectValue(object initialValue)
        {
            ItShouldThrowInvalidExportFieldValueExceptionWithInnerExceptionWhenDeserializationFails<Choice>(initialValue);
        }

        private void ItShouldReturnProperResultWhenDeserializationSucceeds<T>(object initialValue, T expectedResult)
        {
            // Act
            T result = _sut.DeserializeAndValidateExportFieldValue<T>(initialValue);

            // Assert
            result.Should().Be(expectedResult);
            _serializerMock.Verify(x => x.Deserialize<T>(initialValue.ToString()), Times.Once);
        }

        private void ItShouldThrowInvalidExportFieldValueExceptionWithTypeNamesWhenDeserializationFails<T>(object initialValue)
        {
            // Act
            Action action = () => _sut.DeserializeAndValidateExportFieldValue<T>(initialValue);

            // Assert
            action.ShouldThrow<InvalidExportFieldValueException>()
                .Which.Message.Should()
                .Contain(typeof(T).Name).And
                .Contain(initialValue.GetType().Name);
        }

        private void ItShouldThrowInvalidExportFieldValueExceptionWithInnerExceptionWhenDeserializationFails<T>(object initialValue)
        {
            // Act
            Action action = () => _sut.DeserializeAndValidateExportFieldValue<T>(initialValue);

            // Assert
            action.ShouldThrow<InvalidExportFieldValueException>()
                .Which.InnerException.Should()
                .Match(ex => ex is JsonReaderException || ex is JsonSerializationException);
        }

        private static IEnumerable<TestCaseData> ThrowExceptionWhenDeserializationFailsTestCasesForArray()
        {
            yield return new TestCaseData(1);
            yield return new TestCaseData("foo");
            yield return new TestCaseData(new object());
            yield return new TestCaseData(SanitizationTestUtils.DeserializeJson("{ \"not\": \"an array\" }"));
        }

        private static IEnumerable<TestCaseData> ThrowExceptionWhenDeserializationFailsTestCasesForObject()
        {
            yield return new TestCaseData(1);
            yield return new TestCaseData("foo");
            yield return new TestCaseData(new object());
            yield return new TestCaseData(SanitizationTestUtils.DeserializeJson("[ \"not\", \"an object\" ]"));
        }
    }
}
