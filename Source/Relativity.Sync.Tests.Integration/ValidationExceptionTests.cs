using System;
using System.IO;
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
	}
}