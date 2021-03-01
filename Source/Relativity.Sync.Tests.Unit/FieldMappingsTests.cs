using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Relativity.Sync.Utils;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Logging;
using Relativity.Sync.RDOs;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Unit.Storage;

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

            _instance = new FieldMappings(_configuration.Object, _serializer.Object, new EmptyLogger());
           
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
            _configuration.Verify(x => x.GetFieldValue(It.IsAny<Func<SyncConfigurationRdo, string>>()), Times.Once);
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