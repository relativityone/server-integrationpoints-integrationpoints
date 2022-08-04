using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Attributes;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.EventHandlers.Tests.IntegrationPoints.Helpers
{
    [TestFixture, Category("Unit")]
    public class ObjectArtifactIdsByStringFieldValueQueryTests
    {
        private IObjectArtifactIdsByStringFieldValueQuery _query;
        private Mock<IRelativityObjectManager> _relativityObjectManager;
        private string _expectedCondition;

        private const string _FIELD_GUID = "085CB84B-4DAA-400F-B28F-18DE267BD7EA";
        private const string _FIELD_VALUE = "valid value";
        private const int _WORKSPACE_ID = 100000;
        private const int _OBJECT_COUNT = 5;
        private const int _RDO_STUB_ARTIFACT_ID = 0;

        [SetUp]
        public void SetUp()
        {
            _relativityObjectManager = new Mock<IRelativityObjectManager>();
            _query = new ObjectArtifactIdsByStringFieldValueQuery(workspaceId => _relativityObjectManager.Object);

            List<RdoStub> rdoStubs = Enumerable
                .Repeat(new RdoStub() { ArtifactId = _RDO_STUB_ARTIFACT_ID }, _OBJECT_COUNT)
                .ToList();

            _relativityObjectManager
                .Setup(x => x.QueryAsync<RdoStub>(It.IsAny<QueryRequest>(), true, It.IsAny<ExecutionIdentity>()))
                .ReturnsAsync(rdoStubs);

            Condition condition = new TextCondition(_FIELD_GUID, TextConditionEnum.EqualTo, _FIELD_VALUE);
            _expectedCondition = condition.ToQueryString();
        }

        [Test]
        public async Task ItShouldConstructProperConditionBasedOnParameters()
        {
            // Act
            List<int> artifactIds = (await _query
                .QueryForObjectArtifactIdsByStringFieldValueAsync<RdoStub>(_WORKSPACE_ID,
                    stub => stub.Property, _FIELD_VALUE)
                .ConfigureAwait(false)).ToList();

            // Assert
            artifactIds.ShouldAllBeEquivalentTo(_RDO_STUB_ARTIFACT_ID);
            artifactIds.Should().HaveCount(_OBJECT_COUNT);

            VerifyQueryCall(Times.Once);
        }

        private void VerifyQueryCall(Func<Times> times)
        {
            _relativityObjectManager
                .Verify(x => x.QueryAsync<RdoStub>(
                    It.Is<QueryRequest>(request => request.Condition.Equals(_expectedCondition, StringComparison.OrdinalIgnoreCase)),
                    true, It.IsAny<ExecutionIdentity>()), times);
        }

        [DynamicObject(@"Rdo Stub", "Whatever", "", @"d014f00d-f2c0-4e7a-b335-84fcb6eae980")]
        private class RdoStub : BaseRdo
        {
            [DynamicField(@"Property", _FIELD_GUID, "Fixed Length Text", 255)]
            public string Property
            {
                get => GetField<string>(new Guid(_FIELD_GUID));
                set => SetField<string>(new Guid(_FIELD_GUID), value);
            }

            private static Dictionary<Guid, DynamicFieldAttribute> _fieldMetadata;
            public override Dictionary<Guid, DynamicFieldAttribute> FieldMetadata =>
                _fieldMetadata ?? (_fieldMetadata = GetFieldMetadata(typeof(RdoStub)));
        }
    }
}
