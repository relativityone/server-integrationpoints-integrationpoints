using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Relativity.Sync.Tests.Integration
{
    [TestFixture]
    public sealed class SyncExceptionTests
    {
        [Test]
        [TestCase("correlation id")]
        [TestCase("")]
        [TestCase(null)]
        public void ItShouldSerializeToXml(string correlationId)
        {
            const int bufferSize = 4096;

            Exception innerEx = new Exception("foo");
            SyncException originalException = new SyncException("message", innerEx, correlationId);
            byte[] buffer = new byte[bufferSize];
            MemoryStream ms = new MemoryStream(buffer);
            MemoryStream ms2 = new MemoryStream(buffer);
            BinaryFormatter formatter = new BinaryFormatter();

            // ACT
            formatter.Serialize(ms, originalException);
            SyncException deserializedException = (SyncException) formatter.Deserialize(ms2);

            // ASSERT
            deserializedException.WorkflowId.Should().Be(originalException.WorkflowId);
            deserializedException.InnerException.Should().NotBeNull();
            deserializedException.InnerException.Message.Should().Be(originalException.InnerException.Message);
            deserializedException.Message.Should().Be(originalException.Message);
        }

        [Test]
        [TestCase("correlation id")]
        [TestCase("")]
        [TestCase(null)]
        public void ItShouldSerializeToJson(string correlationId)
        {
            Exception innerEx = new Exception("foo");
            SyncException originalException = new SyncException("message", innerEx, correlationId);

            // ACT
            string json = JsonConvert.SerializeObject(originalException);
            SyncException deserializedException = JsonConvert.DeserializeObject<SyncException>(json);

            // ASSERT
            deserializedException.WorkflowId.Should().Be(originalException.WorkflowId);
            deserializedException.InnerException.Should().NotBeNull();
            deserializedException.InnerException.Message.Should().Be(originalException.InnerException.Message);
            deserializedException.Message.Should().Be(originalException.Message);
        }
    }
}