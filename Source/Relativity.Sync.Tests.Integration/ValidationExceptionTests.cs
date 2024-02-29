using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using Relativity.Sync.Executors.Validation;

namespace Relativity.Sync.Tests.Integration
{
    [TestFixture]
    internal sealed class ValidationExceptionTests : ExceptionSerializationTestsBase<ValidationException>
    {
        private Exception _innerException;
        private ValidationMessage _validationMessage;
        private ValidationResult _validationResult;
        private const string _VALIDATION_EXCEPTION_MESSAGE = "message";
        private const string _ERROR_CODE = "error code";
        private const string _VALIDATION_MESSAGE = "validation message";
        private const int _BUFFER_SIZE = 4096;

        [SetUp]
        public void SetUp()
        {
            _innerException = new InvalidOperationException("foo");
            _validationMessage = new ValidationMessage(_ERROR_CODE, _VALIDATION_MESSAGE);
            _validationResult = new ValidationResult(_validationMessage);
        }

        [Test]
        public void ItShouldSerializeToXmlWithMessageAndValidationResult()
        {
            ValidationException originalException = new ValidationException(_VALIDATION_EXCEPTION_MESSAGE, _validationResult);
            byte[] buffer = new byte[_BUFFER_SIZE];
            MemoryStream ms = new MemoryStream(buffer);
            MemoryStream ms2 = new MemoryStream(buffer);
            BinaryFormatter formatter = new BinaryFormatter();

            // ACT
            formatter.Serialize(ms, originalException);
            ValidationException deserializedException = (ValidationException)formatter.Deserialize(ms2);

            // ASSERT
            deserializedException.Message.Should().Be(originalException.Message);
            deserializedException.ValidationResult.IsValid.Should().Be(originalException.ValidationResult.IsValid);
            List<ValidationMessage> validationMessages = deserializedException.ValidationResult.Messages.ToList();
            validationMessages.Should().HaveCount(_validationResult.Messages.Count());
            validationMessages.Should().Contain(_validationMessage);
        }

        [Test]
        public void ItShouldSerializeToJsonWithMessageAndValidationResult()
        {
            ValidationException originalException = new ValidationException(_VALIDATION_EXCEPTION_MESSAGE, _validationResult);

            // ACT
            string json = JsonConvert.SerializeObject(originalException);
            ValidationException deserializedException = JsonConvert.DeserializeObject<ValidationException>(json);

            // ASSERT
            deserializedException.Message.Should().Be(originalException.Message);
            deserializedException.ValidationResult.IsValid.Should().Be(originalException.ValidationResult.IsValid);
            List<ValidationMessage> validationMessages = deserializedException.ValidationResult.Messages.ToList();
            validationMessages.Should().HaveCount(_validationResult.Messages.Count());
            validationMessages.Should().Contain(_validationMessage);
        }

        [Test]
        public void ItShouldSerializeToXmlWithMessageResult()
        {
            ValidationException originalException = new ValidationException(_VALIDATION_EXCEPTION_MESSAGE);
            byte[] buffer = new byte[_BUFFER_SIZE];
            MemoryStream ms = new MemoryStream(buffer);
            MemoryStream ms2 = new MemoryStream(buffer);
            BinaryFormatter formatter = new BinaryFormatter();

            // ACT
            formatter.Serialize(ms, originalException);
            ValidationException deserializedException = (ValidationException)formatter.Deserialize(ms2);

            // ASSERT
            deserializedException.Message.Should().Be(originalException.Message);
            deserializedException.ValidationResult.IsValid.Should().Be(originalException.ValidationResult.IsValid);
        }

        [Test]
        public void ItShouldSerializeToJsonWithMessageResult()
        {
            ValidationException originalException = new ValidationException(_VALIDATION_EXCEPTION_MESSAGE);

            // ACT
            string json = JsonConvert.SerializeObject(originalException);
            ValidationException deserializedException = JsonConvert.DeserializeObject<ValidationException>(json);

            // ASSERT
            deserializedException.Message.Should().Be(originalException.Message);
            deserializedException.ValidationResult.IsValid.Should().Be(originalException.ValidationResult.IsValid);
        }

        [Test]
        public void ItShouldSerializeToXmlWithValidationResult()
        {
            ValidationException originalException = new ValidationException(_validationResult);
            byte[] buffer = new byte[_BUFFER_SIZE];
            MemoryStream ms = new MemoryStream(buffer);
            MemoryStream ms2 = new MemoryStream(buffer);
            BinaryFormatter formatter = new BinaryFormatter();

            // ACT
            formatter.Serialize(ms, originalException);
            ValidationException deserializedException = (ValidationException)formatter.Deserialize(ms2);

            // ASSERT
            deserializedException.ValidationResult.IsValid.Should().Be(originalException.ValidationResult.IsValid);
            List<ValidationMessage> validationMessages = deserializedException.ValidationResult.Messages.ToList();
            validationMessages.Should().HaveCount(_validationResult.Messages.Count());
            validationMessages.Should().Contain(_validationMessage);
        }

        [Test]
        public void ItShouldSerializeToJsonWithValidationResult()
        {
            ValidationException originalException = new ValidationException(_validationResult);

            // ACT
            string json = JsonConvert.SerializeObject(originalException);
            ValidationException deserializedException = JsonConvert.DeserializeObject<ValidationException>(json);

            // ASSERT
            deserializedException.ValidationResult.IsValid.Should().Be(originalException.ValidationResult.IsValid);
            List<ValidationMessage> validationMessages = deserializedException.ValidationResult.Messages.ToList();
            validationMessages.Should().HaveCount(_validationResult.Messages.Count());
            validationMessages.Should().Contain(_validationMessage);
        }

        [Test]
        public void ItShouldSerializeToXmlWithMessageAndInnerExceptionAndValidationResult()
        {
            ValidationResult validationResult = new ValidationResult();
            validationResult.Add(new ValidationMessage(_ERROR_CODE, _VALIDATION_MESSAGE));

            ValidationException originalException = new ValidationException(_VALIDATION_EXCEPTION_MESSAGE, _innerException, validationResult);
            byte[] buffer = new byte[_BUFFER_SIZE];
            MemoryStream ms = new MemoryStream(buffer);
            MemoryStream ms2 = new MemoryStream(buffer);
            BinaryFormatter formatter = new BinaryFormatter();

            // ACT
            formatter.Serialize(ms, originalException);
            ValidationException deserializedException = (ValidationException)formatter.Deserialize(ms2);

            // ASSERT
            deserializedException.InnerException.Should().NotBeNull();
            deserializedException.InnerException.Message.Should().Be(originalException.InnerException.Message);
            deserializedException.Message.Should().Be(originalException.Message);
            deserializedException.ValidationResult.IsValid.Should().Be(originalException.ValidationResult.IsValid);
            List<ValidationMessage> validationMessages = deserializedException.ValidationResult.Messages.ToList();
            validationMessages.Should().HaveCount(validationResult.Messages.Count());
            validationMessages.Should().Contain(_validationMessage);
        }

        [Test]
        public void ItShouldSerializeToJsonWithMessageAndInnerExceptionAndValidationResult()
        {
            ValidationResult validationResult = new ValidationResult();
            validationResult.Add(new ValidationMessage(_ERROR_CODE, _VALIDATION_MESSAGE));

            ValidationException originalException = new ValidationException(_VALIDATION_EXCEPTION_MESSAGE, _innerException, validationResult);

            // ACT
            string json = JsonConvert.SerializeObject(originalException);
            ValidationException deserializedException = JsonConvert.DeserializeObject<ValidationException>(json);

            // ASSERT
            deserializedException.InnerException.Should().NotBeNull();
            deserializedException.InnerException.Message.Should().Be(originalException.InnerException.Message);
            deserializedException.Message.Should().Be(originalException.Message);
            deserializedException.ValidationResult.IsValid.Should().Be(originalException.ValidationResult.IsValid);
            List<ValidationMessage> validationMessages = deserializedException.ValidationResult.Messages.ToList();
            validationMessages.Should().HaveCount(validationResult.Messages.Count());
            validationMessages.Should().Contain(_validationMessage);
        }

        [Test]
        public void ItShouldFormatToStringWithValidationMessage()
        {
            ValidationResult validationResult = new ValidationResult();
            validationResult.Add(new ValidationMessage(_VALIDATION_MESSAGE));

            ValidationException exception = new ValidationException(validationResult);

            // act
            string formattedException = exception.ToString();

            // assert
            string expected = $"Is valid: False{System.Environment.NewLine}1. {_VALIDATION_MESSAGE}{System.Environment.NewLine}";
            Assert.AreEqual(expected, formattedException);
        }

        [Test]
        public void ItShouldFormatToStringWithValidationMessageAndErrorCode()
        {
            ValidationResult validationResult = new ValidationResult();
            validationResult.Add(new ValidationMessage(_ERROR_CODE, _VALIDATION_MESSAGE));

            ValidationException exception = new ValidationException(validationResult);

            // act
            string formattedException = exception.ToString();

            // assert
            string expected = $"Is valid: False{System.Environment.NewLine}1. (Error code: {_ERROR_CODE}) {_VALIDATION_MESSAGE}{System.Environment.NewLine}";
            Assert.AreEqual(expected, formattedException);
        }

        [Test]
        public void ItShouldFormatToStringWithoutValidationMessage()
        {
            ValidationResult validationResult = new ValidationResult()
            {
                IsValid = false
            };

            ValidationException exception = new ValidationException(validationResult);

            // act
            string formattedException = exception.ToString();

            // assert
            string expected = $"Is valid: False{System.Environment.NewLine}";
            Assert.AreEqual(expected, formattedException);
        }
    }
}
