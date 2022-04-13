using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Logging;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Unit.Storage;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Tests.Unit
{
    internal sealed class FieldMappingsTests : ConfigurationTestBase
    {
        private FieldMappings _instance;

        private Mock<ISerializer> _serializer;

        private const string _FIELD_MAP = "field map";
      
        [SetUp]
        public void Setup()
        {
            _serializer = new Mock<ISerializer>();

            _instance = new FieldMappings(_configuration, _serializer.Object, new EmptyLogger());
           
            _configurationRdo.FieldsMapping = _FIELD_MAP;
        }

        [Test]
        public void ItShouldDeserializeFieldMappings()
        {
            List<FieldMap> fieldMappings = new List<FieldMap>();

            _serializer.Setup(x => x.Deserialize<List<FieldMap>>(_FIELD_MAP)).Returns(fieldMappings);

            // ACT
            IList<FieldMap> actualResult = _instance.GetFieldMappings();

            // ASSERT
            actualResult.Should().BeSameAs(fieldMappings);
        }

        [Test]
        public void ItShouldCacheFieldMappings()
        {
            List<FieldMap> fieldMappings = new List<FieldMap>();

            _serializer.Setup(x => x.Deserialize<List<FieldMap>>(_FIELD_MAP)).Returns(fieldMappings);

            // ACT
            _instance.GetFieldMappings();
            _instance.GetFieldMappings();

            // ASSERT
            _serializer.Verify(x => x.Deserialize<List<FieldMap>>(_FIELD_MAP), Times.Once);
        }

        [Test]
        public void ItShouldNotHideException()
        {
            _serializer.Setup(x => x.Deserialize<List<FieldMap>>(_FIELD_MAP)).Throws<InvalidOperationException>();

            // ACT
            Action action = () => _instance.GetFieldMappings();

            // ASSERT
            action.Should().Throw<InvalidOperationException>();
        }
    }
}