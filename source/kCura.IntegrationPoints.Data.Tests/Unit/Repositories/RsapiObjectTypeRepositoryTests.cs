using System.Data;
using System.Data.SqlClient;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Tests.Unit.Repositories
{
	[TestFixture]
	public class RsapiObjectTypeRepositoryTests
	{
		private const int _WORKSPACE_ARTIFACT_ID = 1024165;

		private IHelper _helper;

		private RsapiObjectTypeRepository _instance;

		[SetUp]
		public void SetUp()
		{
			_helper = Substitute.For<IHelper>();

			_instance = new RsapiObjectTypeRepository(_helper, _WORKSPACE_ARTIFACT_ID);
		}

		[Test]
		public void RetrieveObjectTypeArtifactId_ReturnsValidArtifactId_Test()
		{
			// Arrange
			int expectedArtifactId = 456;
			string objectTypeName = "Relativity Source Case Object Type Example";

			IDBContext workspaceContext = Substitute.For<IDBContext>();
			_helper.GetDBContext(_WORKSPACE_ARTIFACT_ID).Returns(workspaceContext);

			SqlParameter nameParameter = new SqlParameter("@objectTypeName", SqlDbType.NVarChar) { Value = objectTypeName };
			workspaceContext.ExecuteSqlStatementAsScalar<int>(
				Arg.Is<string>(x => x.Trim() == _OBJECT_TYPE_ARTIFACT_ID_SQL.Trim()),
				Arg.Is<SqlParameter>(
					x =>
						x.ParameterName == nameParameter.ParameterName && x.Value.Equals(nameParameter.Value) &&
						x.DbType == nameParameter.DbType)).Returns(expectedArtifactId);

			// Act
			int? actualArtifactId = _instance.RetrieveObjectTypeArtifactId(objectTypeName);

			// Assert
			Assert.IsNotNull(actualArtifactId);
			Assert.AreEqual(expectedArtifactId, actualArtifactId);

			_helper.Received(1).GetDBContext(_WORKSPACE_ARTIFACT_ID);
			workspaceContext.Received(1).ExecuteSqlStatementAsScalar<int>(Arg.Any<string>(), Arg.Any<SqlParameter>());
		}

		[Test]
		public void RetrieveObjectTypeArtifactId_ReturnsZeroArtifactId_ReturnsNull_Test()
		{
			// Arrange
			int expectedArtifactId = 0;
			string objectTypeName = "Relativity Source Case Object Type Example";

			IDBContext workspaceContext = Substitute.For<IDBContext>();
			_helper.GetDBContext(_WORKSPACE_ARTIFACT_ID).Returns(workspaceContext);

			SqlParameter nameParameter = new SqlParameter("@objectTypeName", SqlDbType.NVarChar) { Value = objectTypeName };
			workspaceContext.ExecuteSqlStatementAsScalar<int>(
				Arg.Is<string>(x => x.Trim() == _OBJECT_TYPE_ARTIFACT_ID_SQL.Trim()),
				Arg.Is<SqlParameter>(
					x =>
						x.ParameterName == nameParameter.ParameterName && x.Value.Equals(nameParameter.Value) &&
						x.DbType == nameParameter.DbType)).Returns(expectedArtifactId);

			// Act
			int? actualArtifactId = _instance.RetrieveObjectTypeArtifactId(objectTypeName);

			// Assert
			Assert.IsNull(actualArtifactId);

			_helper.Received(1).GetDBContext(_WORKSPACE_ARTIFACT_ID);
			workspaceContext.Received(1).ExecuteSqlStatementAsScalar<int>(Arg.Any<string>(), Arg.Any<SqlParameter>());
		}

		#region SQL Queries

		private const string _OBJECT_TYPE_ARTIFACT_ID_SQL = @"
				SELECT [ArtifactID]
				FROM [eddsdbo].[ObjectType]
				WHERE [Name] = @objectTypeName";

		#endregion
	}
}
