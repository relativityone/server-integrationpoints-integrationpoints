using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.RDO;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.Relativity.Client;
using Relativity.API;
using Relativity.Core;
using Relativity.Core.Service;
using Relativity.Services.ObjectQuery;
using Field = Relativity.Core.DTO.Field;
using Query = Relativity.Services.ObjectQuery.Query;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class FieldRepository : IFieldRepository
	{
		private readonly IHelper _helper;
		private readonly IObjectQueryManagerAdaptor _objectQueryManagerAdaptor;
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
			int workspaceArtifactId)
		{
			_helper = helper;
			_objectQueryManagerAdaptor = objectQueryManagerAdaptor;
			_serviceContext = serviceContext;
			_baseContext = baseContext;
			_workspaceArtifactId = workspaceArtifactId;
			_fieldManager = new Lazy<IFieldManagerImplementation>(() => new FieldManagerImplementation());
		}

		public async Task<ArtifactFieldDTO[]> RetrieveLongTextFieldsAsync(int rdoTypeId)
		{
			const string longTextFieldName = "Long Text";

			var longTextFieldsQuery = new Query()
			{
				Condition = String.Format("'Object Type Artifact Type ID' == {0} AND 'Field Type' == '{1}'", rdoTypeId, longTextFieldName),
			};

			ObjectQueryResultSet result = await _objectQueryManagerAdaptor.RetrieveAsync(longTextFieldsQuery, String.Empty);

			if (!result.Success)
			{
				throw new Exception(result.Message);
			}

			ArtifactFieldDTO[] fieldDtos = result.Data.DataResults.Select(x => new ArtifactFieldDTO()
			{
				ArtifactId = x.ArtifactId,
				FieldType = longTextFieldName,
				Name = x.TextIdentifier,
				Value = null // Field RDO's don't have values...setting this to NULL to be explicit
			}).ToArray();

			return fieldDtos;
		}

		public async Task<ArtifactDTO[]> RetrieveFieldsAsync(int rdoTypeId, HashSet<string> fieldFieldsNames)
		{
			var fieldQuery = new Query()
			{
				Fields = fieldFieldsNames.ToArray(),
				Condition = $"'Object Type Artifact Type ID' == {rdoTypeId}"
			};

			ObjectQueryResultSet result = await _objectQueryManagerAdaptor.RetrieveAsync(fieldQuery, String.Empty);

			if (!result.Success)
			{
				throw new Exception(result.Message);
			}

			ArtifactDTO[] fieldArtifacts = result.Data.DataResults.Select(x =>
				new ArtifactDTO(
					x.ArtifactId,
					x.ArtifactTypeId,
					x.Fields.Select(
						y => new ArtifactFieldDTO() { ArtifactId = y.ArtifactId, FieldType = y.FieldType, Name = y.Name, Value = y.Value }))
			).ToArray();

			return fieldArtifacts;
		}

		public void SetOverlayBehavior(int fieldArtifactId, bool value)
		{
			Field fieldDto = FieldManager.Read(_serviceContext, fieldArtifactId);
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