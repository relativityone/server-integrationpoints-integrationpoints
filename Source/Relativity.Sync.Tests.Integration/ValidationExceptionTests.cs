using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using Relativity.Sync.Executors;
using Relativity.Sync.Executors.Validation;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	public class ValidationExceptionTests
	{
		[Test]
		public void ItShouldSerializeToXml()
		{
			const int bufferSize = 4096;

			Exception innerEx = new Exception("foo");
			ValidationException originalException = new ValidationException("message", innerEx);
			byte[] buffer = new byte[bufferSize];
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
		}

		[Test]
		public void ItShouldSerializeToJson()
		{
			Exception innerEx = new Exception("foo");
			ValidationException originalException = new ValidationException("message", innerEx);

			// ACT
			string json = JsonConvert.SerializeObject(originalException);
			ValidationException deserializedException = JsonConvert.DeserializeObject<ValidationException>(json);

			// ASSERT
			deserializedException.InnerException.Should().NotBeNull();
			deserializedException.InnerException.Message.Should().Be(originalException.InnerException.Message);
			deserializedException.Message.Should().Be(originalException.Message);
		}

		[Test]
		public void ItShouldSerializeToXmlWithMessageAndValidationResult()
		{
			const int bufferSize = 4096;
			const string message = "message";
			ValidationResult validationResult = new ValidationResult();
			const string errorCode = "errorcode";
			const string validationMessage = "validation message";
			validationResult.Add(errorCode, validationMessage);

			ValidationException originalException = new ValidationException(message, validationResult);
			byte[] buffer = new byte[bufferSize];
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
			validationMessages.Count.Should().Be(validationResult.Messages.Count());
			validationMessages[0].ErrorCode.Should().Be(errorCode);
			validationMessages[0].ShortMessage.Should().Be(validationMessage);
		}

		[Test]
		public void ItShouldSerializeToJsonWithMessageAndValidationResult()
		{
			const string message = "message";
			ValidationResult validationResult = new ValidationResult();
			const string errorCode = "errorcode";
			const string validationMessage = "validation message";
			validationResult.Add(errorCode, validationMessage);

			ValidationException originalException = new ValidationException(message, validationResult);

			// ACT
			string json = JsonConvert.SerializeObject(originalException);
			ValidationException deserializedException = JsonConvert.DeserializeObject<ValidationException>(json);

			// ASSERT
			deserializedException.Message.Should().Be(originalException.Message);
			deserializedException.ValidationResult.IsValid.Should().Be(originalException.ValidationResult.IsValid);
			List<ValidationMessage> validationMessages = deserializedException.ValidationResult.Messages.ToList();
			validationMessages.Count.Should().Be(validationResult.Messages.Count());
			validationMessages[0].ErrorCode.Should().Be(errorCode);
			validationMessages[0].ShortMessage.Should().Be(validationMessage);
		}

		[Test]
		public void ItShouldSerializeToXmlWithMessageResult()
		{
			const int bufferSize = 4096;
			const string message = "message";

			ValidationException originalException = new ValidationException(message);
			byte[] buffer = new byte[bufferSize];
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
			const string message = "message";

			ValidationException originalException = new ValidationException(message);

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
			const int bufferSize = 4096;
			ValidationResult validationResult = new ValidationResult();
			const string errorCode = "errorcode";
			const string validationMessage = "validation message";
			validationResult.Add(errorCode, validationMessage);

			ValidationException originalException = new ValidationException(validationResult);
			byte[] buffer = new byte[bufferSize];
			MemoryStream ms = new MemoryStream(buffer);
			MemoryStream ms2 = new MemoryStream(buffer);
			BinaryFormatter formatter = new BinaryFormatter();

			// ACT
			formatter.Serialize(ms, originalException);
			ValidationException deserializedException = (ValidationException)formatter.Deserialize(ms2);

			// ASSERT
			deserializedException.ValidationResult.IsValid.Should().Be(originalException.ValidationResult.IsValid);
			List<ValidationMessage> validationMessages = deserializedException.ValidationResult.Messages.ToList();
			validationMessages.Count.Should().Be(validationResult.Messages.Count());
			validationMessages[0].ErrorCode.Should().Be(errorCode);
			validationMessages[0].ShortMessage.Should().Be(validationMessage);
		}

		[Test]
		public void ItShouldSerializeToJsonWithValidationResult()
		{
			ValidationResult validationResult = new ValidationResult();
			const string errorCode = "errorcode";
			const string validationMessage = "validation message";
			validationResult.Add(errorCode, validationMessage);

			ValidationException originalException = new ValidationException(validationResult);

			// ACT
			string json = JsonConvert.SerializeObject(originalException);
			ValidationException deserializedException = JsonConvert.DeserializeObject<ValidationException>(json);

			// ASSERT
			deserializedException.ValidationResult.IsValid.Should().Be(originalException.ValidationResult.IsValid);
			List<ValidationMessage> validationMessages = deserializedException.ValidationResult.Messages.ToList();
			validationMessages.Count.Should().Be(validationResult.Messages.Count());
			validationMessages[0].ErrorCode.Should().Be(errorCode);
			validationMessages[0].ShortMessage.Should().Be(validationMessage);
		}

		[Test]
		public void ItShouldSerializeToXmlWithMessageAndInnerExceptionAndValidationResult()
		{
			const int bufferSize = 4096;
			const string message = "message";
			ValidationResult validationResult = new ValidationResult();
			const string errorCode = "errorcode";
			const string validationMessage = "validation message";
			validationResult.Add(errorCode, validationMessage);
			Exception innerEx = new Exception("foo");

			ValidationException originalException = new ValidationException(message, innerEx, validationResult);
			byte[] buffer = new byte[bufferSize];
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
			validationMessages.Count.Should().Be(validationResult.Messages.Count());
			validationMessages[0].ErrorCode.Should().Be(errorCode);
			validationMessages[0].ShortMessage.Should().Be(validationMessage);
		}

		[Test]
		public void ItShouldSerializeToJsonWithMessageAndInnerExceptionAndValidationResult()
		{
			const string message = "message";
			ValidationResult validationResult = new ValidationResult();
			const string errorCode = "errorcode";
			const string validationMessage = "validation message";
			validationResult.Add(errorCode, validationMessage);
			Exception innerEx = new Exception("foo");

			ValidationException originalException = new ValidationException(message, innerEx, validationResult);

			// ACT
			string json = JsonConvert.SerializeObject(originalException);
			ValidationException deserializedException = JsonConvert.DeserializeObject<ValidationException>(json);

			// ASSERT
			deserializedException.InnerException.Should().NotBeNull();
			deserializedException.InnerException.Message.Should().Be(originalException.InnerException.Message);
			deserializedException.Message.Should().Be(originalException.Message);
			deserializedException.ValidationResult.IsValid.Should().Be(originalException.ValidationResult.IsValid);
			List<ValidationMessage> validationMessages = deserializedException.ValidationResult.Messages.ToList();
			validationMessages.Count.Should().Be(validationResult.Messages.Count());
			validationMessages[0].ErrorCode.Should().Be(errorCode);
			validationMessages[0].ShortMessage.Should().Be(validationMessage);
		}
	}
}