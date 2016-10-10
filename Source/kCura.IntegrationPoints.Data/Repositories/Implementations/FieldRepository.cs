using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Contracts.RDO;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.API;
using Relativity.Core;
using Relativity.Core.Service;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class FieldRepository : KeplerServiceBase, IFieldRepository
	{
		private readonly IHelper _helper;
		private readonly BaseServiceContext _serviceContext;
		private readonly BaseContext _baseContext;
		private readonly int _workspaceArtifactId;
		private readonly Lazy<IFieldManagerImplementation> _fieldManager;
		private IFieldManagerImplementation FieldManager => _fieldManager.Value;

		public FieldRepository(
			IHelper helper,
			IObjectQueryManagerAdaptor objectQueryManagerAdaptor,
			BaseServiceContext serviceContext,
			BaseContext baseContext,
			int workspaceArtifactId) : base(objectQueryManagerAdaptor)
		{
			_helper = helper;
			_serviceContext = serviceContext;
			_baseContext = baseContext;
			_workspaceArtifactId = workspaceArtifactId;
			_fieldManager = new Lazy<IFieldManagerImplementation>(() => new FieldManagerImplementation());
		}

		public async Task<ArtifactFieldDTO[]> RetrieveLongTextFieldsAsync(int rdoTypeId)
		{
			const string longTextFieldName = "Long Text";

			var longTextFieldsQuery = new global::Relativity.Services.ObjectQuery.Query()
			{
				Condition = String.Format("'Object Type Artifact Type ID' == {0} AND 'Field Type' == '{1}'", rdoTypeId, longTextFieldName),
			};

			ArtifactDTO[] artifactDtos = null;
			try
			{
				artifactDtos = await this.RetrieveAllArtifactsAsync(longTextFieldsQuery);
			}
			catch (Exception e)
			{
				throw new Exception("Unable to retrieve long text fields", e);
			}

			ArtifactFieldDTO[] fieldDtos =
				artifactDtos.Select(x => new ArtifactFieldDTO()
				{
					ArtifactId = x.ArtifactId,
					FieldType = longTextFieldName,
					Name = x.TextIdentifier,
					Value = null // Field RDO's don't have values...setting this to NULL to be explicit
				}).ToArray();

			return fieldDtos;
		}

		public async Task<ArtifactDTO[]> RetrieveFieldsAsync(int rdoTypeId, HashSet<string> fieldNames)
		{
			var fieldQuery = new global::Relativity.Services.ObjectQuery.Query()
			{
				Fields = fieldNames.ToArray(),
				Condition = $"'Object Type Artifact Type ID' == {rdoTypeId}"
			};

			ArtifactDTO[] fieldArtifactDtos = null;
			try
			{
				fieldArtifactDtos = await this.RetrieveAllArtifactsAsync(fieldQuery);
			}
			catch (Exception e)
			{
				throw new Exception("Unable to retrieve fields", e);
			}

			return fieldArtifactDtos;
		}

		public ArtifactDTO[] RetrieveFields(int rdoTypeId, HashSet<string> fieldNames)
		{
			return Task.Run(() => RetrieveFieldsAsync(rdoTypeId, fieldNames)).GetResultsWithoutContextSync();
		}


		public int? RetrieveField(string displayName, int fieldArtifactTypeId, int fieldTypeId)
		{
			string sql = @"SELECT [ArtifactID] FROM [eddsdbo].[Field] WHERE [FieldArtifactTypeID] = @fieldArtifactTypeId AND [FieldTypeID] = @fieldTypeId AND [DisplayName] = @displayName";

			SqlParameter displayNameParameter = new SqlParameter("@displayName", SqlDbType.NVarChar) { Value = displayName };
			SqlParameter fieldArtifactTypeIdParameter = new SqlParameter("@fieldArtifactTypeId", SqlDbType.Int) { Value = fieldArtifactTypeId };
			SqlParameter fieldTypeIdParameter = new SqlParameter("@fieldTypeId", SqlDbType.Int) { Value = fieldTypeId };
			SqlParameter[] sqlParameters = { displayNameParameter, fieldArtifactTypeIdParameter, fieldTypeIdParameter };

			IDBContext workspaceContext = _helper.GetDBContext(_workspaceArtifactId);
			int? fieldArtifactId = workspaceContext.ExecuteSqlStatementAsScalar<int>(sql, sqlParameters);

			return fieldArtifactId > 0 ? fieldArtifactId : null;
		}

		public void SetOverlayBehavior(int fieldArtifactId, bool value)
		{
			global::Relativity.Core.DTO.Field fieldDto = FieldManager.Read(_serviceContext, fieldArtifactId);
			fieldDto.OverlayBehavior = value;
			FieldManager.Update(_serviceContext, fieldDto, fieldDto.DisplayName, fieldDto.IsArtifactBaseField);
		}

		public void Delete(IEnumerable<int> artifactIds)
		{
			using (IRSAPIClient rsapiClient = _helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser))
			{
				rsapiClient.APIOptions.WorkspaceID = _workspaceArtifactId;

				rsapiClient.Repositories.Field.Delete(artifactIds.ToArray());
			}
		}

		public ResultSet<Relativity.Client.DTOs.Field> Read(Relativity.Client.DTOs.Field dto)
		{
			ResultSet<Relativity.Client.DTOs.Field> resultSet = null;
			using (IRSAPIClient rsapiClient = _helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser))
			{
				rsapiClient.APIOptions.WorkspaceID = _workspaceArtifactId;

				try
				{
					resultSet = rsapiClient.Repositories.Field.Read(dto);
				}
				catch (Exception e)
				{
					throw new Exception("Unable to read Field dto", e);
				}

				return resultSet;
			}
		}

		public int? RetrieveArtifactViewFieldId(int fieldArtifactId)
		{
			const string artifactIdParamName = "@ArtifactId";
			string sql = $@"
			SELECT TOP 1 AVF.ArtifactViewFieldID
			FROM EDDSDBO.Field as F
			JOIN EDDSDBO.ArtifactViewField as AVF on F.ArtifactViewFieldID = AVF.ArtifactViewFieldID
			WHERE F.ArtifactID = {artifactIdParamName}";

			var artifactIdParam = new SqlParameter(artifactIdParamName, SqlDbType.Int) { Value = fieldArtifactId };

			int? artifactViewFieldId = null;
			using (SqlDataReader reader = _baseContext.DBContext.ExecuteSQLStatementAsReader(sql, new[] { artifactIdParam }))
			{
				if (reader.Read())
				{
					artifactViewFieldId = reader.GetInt32(0);
				}
			}

			return artifactViewFieldId;
		}

		public ArtifactDTO RetrieveTheIdentifierField(int rdoTypeId)
		{
			HashSet<string> fieldsToRetrieveWhenQueryFields = new HashSet<string>() { "Name", "Is Identifier" };
			ArtifactDTO[] fieldsDtos = RetrieveFieldsAsync(rdoTypeId, fieldsToRetrieveWhenQueryFields).GetResultsWithoutContextSync();
			ArtifactDTO identifierField = fieldsDtos.First(field => Convert.ToBoolean(field.Fields[1].Value));
			return identifierField;
		}

		public void UpdateFilterType(int artifactViewFieldId, string filterType)
		{
			const string artifactViewFieldIdParamName = "@ArtifactViewFieldID";
			const string filterTypeParamName = "@FilterType";
			string sql = $@"
			UPDATE EDDSDBO.ArtifactViewField
			SET FilterType = {filterTypeParamName}
			WHERE ArtifactViewFieldID = {artifactViewFieldId}";

			var filterTypeParam = new SqlParameter(filterTypeParamName, SqlDbType.Text) { Value = filterType };
			var artifactViewFieldIdParam = new SqlParameter(artifactViewFieldIdParamName, SqlDbType.Int) { Value = artifactViewFieldId };

			_baseContext.DBContext.ExecuteNonQuerySQLStatement(sql, new[] { filterTypeParam, artifactViewFieldIdParam });
		}
	}
}