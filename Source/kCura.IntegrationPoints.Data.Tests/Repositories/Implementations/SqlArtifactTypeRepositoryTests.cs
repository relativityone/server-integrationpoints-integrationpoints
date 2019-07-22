using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Tests.Repositories.Implementations
{
	[TestFixture]
	public class SqlArtifactTypeRepositoryTests
	{
		private IArtifactTypeRepository _sut;
		private Mock<IRelativityObjectManager> _objectManagerMock;

		private const int _ARTIFACT_TYPE_ID_OBJECT_TYPE = 25;
		private const string _ARTIFACT_TYPE_ID_FIELD = "Artifact Type ID";
		private const string _ARTIFACT_TYPE_NAME = "Federated Instance";

		[SetUp]
		public void SetUp()
		{
			_objectManagerMock = new Mock<IRelativityObjectManager>();
			_sut = new SqlArtifactTypeRepository(_objectManagerMock.Object);
		}

		[Test]
		public void GetArtifactTypeIdFromArtifactTypeName_ShouldCallObjectManagerAndReturnProperValue()
		{
			// arrange
			string condition = $"((('Name' LIKE ['{_ARTIFACT_TYPE_NAME}'])))";
			const int expectedArtifactTypeID = 1000123;
			var expectedResult = new List<RelativityObject>
			{
				new RelativityObject
				{
					FieldValues = new List<FieldValuePair>
					{
						new FieldValuePair {Field = new Field {Name = _ARTIFACT_TYPE_ID_FIELD}, Value = expectedArtifactTypeID}
					}
				}
			};
			_objectManagerMock
				.Setup(x => x.Query(It.Is<QueryRequest>(y => IsQueryRequestValid(y, condition)), ExecutionIdentity.CurrentUser))
				.Returns(expectedResult);

			// act
			int actualArtifactTypeID = _sut.GetArtifactTypeIdFromArtifactTypeName(_ARTIFACT_TYPE_NAME);

			// assert
			_objectManagerMock.Verify(
				x => x.Query(It.Is<QueryRequest>(y => IsQueryRequestValid(y, condition)), ExecutionIdentity.CurrentUser),
				Times.Once);
			actualArtifactTypeID.Should().Be(expectedArtifactTypeID);
		}

		private static bool IsQueryRequestValid(QueryRequest queryRequest, string condition)
		{
			return queryRequest.ObjectType.ArtifactTypeID == _ARTIFACT_TYPE_ID_OBJECT_TYPE &&
			       queryRequest.Fields.Count() == 1 &&
			       queryRequest.Fields.First().Name == _ARTIFACT_TYPE_ID_FIELD &&
			       queryRequest.Condition == condition;
		}
	}
}
