using System.Data;
using System.Data.SqlClient;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Contracts.RDO;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.Core;

namespace kCura.IntegrationPoints.Data.Tests.Repositories
{
	[TestFixture]
	public class ExtendedFieldRepositoryTests : TestBase
	{
		private const int _WORKSPACE_ARTIFACT_ID = 1024165;

		private IHelper _helper;
		private IObjectQueryManagerAdaptor _objectQueryManagerAdaptor;
		private BaseServiceContext _baseServiceContext;
		private BaseContext _baseContext;

		private IExtendedFieldRepository _instance;

		[SetUp]
		public override void SetUp()
		{
			_helper = Substitute.For<IHelper>();
			_objectQueryManagerAdaptor = Substitute.For<IObjectQueryManagerAdaptor>();
			_baseServiceContext = Substitute.For<BaseServiceContext>();
			_baseContext = Substitute.For<BaseContext>();

			_instance = new SqlExtendedFieldRepository(_helper, _objectQueryManagerAdaptor, _baseServiceContext, _baseContext, _WORKSPACE_ARTIFACT_ID);
		}

		[Test]
		public void RetrieveField_ReturnsValidArtifactId_Test()
		{
			// Arrange
			int expectedFieldArtifactId = 456;
			string fieldDisplayName = "Relativity Source Case Test Field";
			int fieldArtifactTypeId = 104165;
			int fieldTypeId = (int) Relativity.Client.FieldType.FixedLengthText;

			IDBContext workspaceContext = Substitute.For<IDBContext>();
			_helper.GetDBContext(_WORKSPACE_ARTIFACT_ID).Returns(workspaceContext);

			SqlParameter displayNameParameter = new SqlParameter("@displayName", SqlDbType.NVarChar) { Value = fieldDisplayName };
			SqlParameter fieldArtifactTypeIdParameter = new SqlParameter("@fieldArtifactTypeId", SqlDbType.Int) { Value = fieldArtifactTypeId };
			SqlParameter fieldTypeIdParameter = new SqlParameter("@fieldTypeId", SqlDbType.Int) { Value = fieldTypeId };
			SqlParameter[] sqlParameters = { displayNameParameter, fieldArtifactTypeIdParameter, fieldTypeIdParameter };

			workspaceContext.ExecuteSqlStatementAsScalar<int>(
				Arg.Is<string>(x => x.Trim() == _RETRIEVE_FIELD_SQL.Trim()),
				Arg.Is<SqlParameter[]>(p => CompareSqlParameters(sqlParameters, p)))
				.Returns(expectedFieldArtifactId);

			// Act
			int? actualArtifactId = _instance.RetrieveField(fieldDisplayName, fieldArtifactTypeId, fieldTypeId);

			// Assert
			Assert.IsNotNull(actualArtifactId);
			Assert.AreEqual(expectedFieldArtifactId, actualArtifactId);

			_helper.Received(1).GetDBContext(_WORKSPACE_ARTIFACT_ID);
			workspaceContext.Received(1).ExecuteSqlStatementAsScalar<int>(Arg.Any<string>(), Arg.Any<SqlParameter[]>());
		}

		[Test]
		public void RetrieveField_ReturnsZeroArtifactId_ReturnsNull_Test()
		{
			// Arrange
			int expectedFieldArtifactId = 0;
			string fieldDisplayName = "Relativity Source Case Test Field";
			int fieldArtifactTypeId = 104165;
			int fieldTypeId = (int)Relativity.Client.FieldType.FixedLengthText;

			IDBContext workspaceContext = Substitute.For<IDBContext>();
			_helper.GetDBContext(_WORKSPACE_ARTIFACT_ID).Returns(workspaceContext);

			SqlParameter displayNameParameter = new SqlParameter("@displayName", SqlDbType.NVarChar) { Value = fieldDisplayName };
			SqlParameter fieldArtifactTypeIdParameter = new SqlParameter("@fieldArtifactTypeId", SqlDbType.Int) { Value = fieldArtifactTypeId };
			SqlParameter fieldTypeIdParameter = new SqlParameter("@fieldTypeId", SqlDbType.Int) { Value = fieldTypeId };
			SqlParameter[] sqlParameters = { displayNameParameter, fieldArtifactTypeIdParameter, fieldTypeIdParameter };

			workspaceContext.ExecuteSqlStatementAsScalar<int>(
				Arg.Is<string>(x => x.Trim() == _RETRIEVE_FIELD_SQL.Trim()),
				Arg.Is<SqlParameter[]>(p => CompareSqlParameters(sqlParameters, p)))
				.Returns(expectedFieldArtifactId);

			// Act
			int? actualArtifactId = _instance.RetrieveField(fieldDisplayName, fieldArtifactTypeId, fieldTypeId);

			// Assert
			Assert.IsNull(actualArtifactId);

			_helper.Received(1).GetDBContext(_WORKSPACE_ARTIFACT_ID);
			workspaceContext.Received(1).ExecuteSqlStatementAsScalar<int>(Arg.Any<string>(), Arg.Any<SqlParameter[]>());
		}

		private bool CompareSqlParameters(SqlParameter[] expectedParameters, SqlParameter[] actualParameters)
		{
			if (expectedParameters == null && actualParameters == null)
			{
				return true;
			}
			if (expectedParameters == null || actualParameters == null)
			{
				return false;
			}
			if (expectedParameters.Length != actualParameters.Length)
			{
				return false;
			}
			foreach (SqlParameter expectedParameter in expectedParameters)
			{
				SqlParameter actualParameter = actualParameters.FirstOrDefault(x => x.ParameterName == expectedParameter.ParameterName);
				if (actualParameter == null)
				{
					return false;
				}
				if (!expectedParameter.Value.Equals(actualParameter.Value))
				{
					return false;
				}
				if (expectedParameter.DbType != actualParameter.DbType)
				{
					return false;
				}
			}
			return true;
		}

		#region SQL Queries

		private const string _RETRIEVE_FIELD_SQL = @"SELECT [ArtifactID] FROM [eddsdbo].[Field] WHERE [FieldArtifactTypeID] = @fieldArtifactTypeId AND [FieldTypeID] = @fieldTypeId AND [DisplayName] = @displayName";

		#endregion
	}
}
