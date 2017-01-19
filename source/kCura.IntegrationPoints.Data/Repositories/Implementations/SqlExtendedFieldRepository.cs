using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.API;
using Relativity.Core;
using Relativity.Core.Service;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class SqlExtendedFieldRepository : IExtendedFieldRepository
	{
		private readonly IHelper _helper;
		private readonly BaseServiceContext _serviceContext;
		private readonly BaseContext _baseContext;
		private readonly int _workspaceArtifactId;
		private readonly Lazy<IFieldManagerImplementation> _fieldManager;
		private IFieldManagerImplementation FieldManager => _fieldManager.Value;

		public SqlExtendedFieldRepository(IHelper helper, BaseServiceContext serviceContext, BaseContext baseContext, int workspaceArtifactId)
		{
			_helper = helper;
			_serviceContext = serviceContext;
			_baseContext = baseContext;
			_workspaceArtifactId = workspaceArtifactId;
			_fieldManager = new Lazy<IFieldManagerImplementation>(() => new FieldManagerImplementation());
		}

		public ArtifactFieldDTO[] RetrieveBeginBatesFields()
		{
			kCura.Data.DataView batesFields = FieldQuery.RetrievePotentialBeginBatesFields(_baseContext);
			DataTable table = batesFields.Table;

			List<ArtifactFieldDTO> artifactFields = new List<ArtifactFieldDTO>();
			foreach (DataRow row in table.Rows)
			{
				ArtifactFieldDTO dto = new ArtifactFieldDTO();
				dto.ArtifactId = int.Parse(row["ArtifactID"].ToString());
				dto.Name = row["DisplayName"].ToString();
				dto.FieldType = row["FieldTypeID"].ToString();

				artifactFields.Add(dto);
			}

			return artifactFields.ToArray();
		}

		public int? RetrieveField(string displayName, int fieldArtifactTypeId, int fieldTypeId)
		{
			string sql = @"
				SELECT [ArtifactID]
				FROM [eddsdbo].[Field]
				WHERE [FieldArtifactTypeID] = @fieldArtifactTypeId
					AND [FieldTypeID] = @fieldTypeId
					AND [DisplayName] = @displayName";

			SqlParameter displayNameParameter = new SqlParameter("@displayName", SqlDbType.NVarChar) { Value = displayName };
			SqlParameter fieldArtifactTypeIdParameter = new SqlParameter("@fieldArtifactTypeId", SqlDbType.Int) { Value = fieldArtifactTypeId };
			SqlParameter fieldTypeIdParameter = new SqlParameter("@fieldTypeId", SqlDbType.Int) { Value = fieldTypeId };
			SqlParameter[] sqlParameters = { displayNameParameter, fieldArtifactTypeIdParameter, fieldTypeIdParameter };

			IDBContext workspaceContext = _helper.GetDBContext(_workspaceArtifactId);
			int? fieldArtifactId = workspaceContext.ExecuteSqlStatementAsScalar<int>(sql, sqlParameters);

			return fieldArtifactId > 0 ? fieldArtifactId : null;
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

			_baseContext.DBContext.ExecuteNonQuerySQLStatement(sql, new[] {filterTypeParam, artifactViewFieldIdParam});
		}

		public void SetOverlayBehavior(int fieldArtifactId, bool value)
		{
			global::Relativity.Core.DTO.Field fieldDto = FieldManager.Read(_serviceContext, fieldArtifactId);
			fieldDto.OverlayBehavior = value;
			FieldManager.Update(_serviceContext, fieldDto, fieldDto.DisplayName, fieldDto.IsArtifactBaseField);
		}
	}
}