﻿using System;
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
	[TestFixture]
	public class IntegrationPointProfilesQueryTests
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
			List<int> artifactIds = await _query
				.QueryForObjectArtifactIdsByStringFieldValueAsync<RdoStub>(_WORKSPACE_ID,
					stub => stub.Property, _FIELD_VALUE)
				.ConfigureAwait(false);

			// Assert
			artifactIds.ShouldAllBeEquivalentTo(_RDO_STUB_ARTIFACT_ID);
			artifactIds.Should().HaveCount(_OBJECT_COUNT);

			VerifyQueryCall(Times.Once);
		}

		[Test]
		public void ItShouldFailOnMethodExpression()
		{
			// Act
			Action run = () => _query
				.QueryForObjectArtifactIdsByStringFieldValueAsync<RdoStub>(_WORKSPACE_ID,
					stub => stub.GetString(), _FIELD_VALUE)
				.GetAwaiter().GetResult();

			// Assert
			run
				.ShouldThrowExactly<ArgumentException>()
				.And
				.Message.Should().EndWith("refers to a method, not a property.");

			VerifyQueryCall(Times.Never);
		}

		[Test]
		public void ItShouldFailOnFieldExpression()
		{
			// Act
			Action run = () => _query
				.QueryForObjectArtifactIdsByStringFieldValueAsync<RdoStub>(_WORKSPACE_ID,
					stub => stub.field, _FIELD_VALUE)
				.GetAwaiter().GetResult();

			// Assert
			run
				.ShouldThrowExactly<ArgumentException>()
				.And
				.Message.Should().EndWith("refers to a field, not a property.");

			VerifyQueryCall(Times.Never);
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
			public string field = "field";

			public string GetString()
			{
				return @"string";
			}

			[DynamicField(@"Property", _FIELD_GUID, "Fixed Length Text", 255)]
			public string Property
			{
				get
				{
					return GetField<string>(new System.Guid(_FIELD_GUID));
				}
				set
				{
					SetField<string>(new System.Guid(_FIELD_GUID), value);
				}
			}
			private static System.Collections.Generic.Dictionary<Guid, DynamicFieldAttribute> _fieldMetadata;
			public override System.Collections.Generic.Dictionary<Guid, DynamicFieldAttribute> FieldMetadata
			{
				get
				{
					if (!(_fieldMetadata == null))
						return _fieldMetadata;
					_fieldMetadata = GetFieldMetadata(typeof(RdoStub));
					return _fieldMetadata;
				}
			}
			private static DynamicObjectAttribute _objectMetadata;
			public override DynamicObjectAttribute ObjectMetadata
			{
				get
				{
					if (!(_objectMetadata == null))
						return _objectMetadata;
					_objectMetadata = GetObjectMetadata(typeof(RdoStub));
					return _objectMetadata;
				}
			}
		}
	}
}