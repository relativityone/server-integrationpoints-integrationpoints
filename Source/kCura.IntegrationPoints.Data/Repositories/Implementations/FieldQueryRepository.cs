using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Contracts.RDO;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.API;
using Relativity.Services.FieldManager;
using Field = kCura.Relativity.Client.DTOs.Field;
using Query = Relativity.Services.ObjectQuery.Query;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class FieldQueryRepository : KeplerServiceBase, IFieldQueryRepository
	{
		private readonly IHelper _helper;
		private readonly IServicesMgr _servicesMgr;
		private readonly int _workspaceArtifactId;

		public FieldQueryRepository(
			IHelper helper, IServicesMgr servicesMgr,
			IObjectQueryManagerAdaptor objectQueryManagerAdaptor,
			int workspaceArtifactId) : base(objectQueryManagerAdaptor)
		{
			_helper = helper;
			_servicesMgr = servicesMgr;
			_workspaceArtifactId = workspaceArtifactId;
		}

		public async Task<ArtifactFieldDTO[]> RetrieveLongTextFieldsAsync(int rdoTypeId)
		{
			const string longTextFieldName = "Long Text";

			var longTextFieldsQuery = new Query
			{
				Condition = string.Format("'Object Type Artifact Type ID' == {0} AND 'Field Type' == '{1}'", rdoTypeId, longTextFieldName)
			};

			ArtifactDTO[] artifactDtos = null;
			try
			{
				artifactDtos = await RetrieveAllArtifactsAsync(longTextFieldsQuery);
			}
			catch (Exception e)
			{
				throw new Exception("Unable to retrieve long text fields", e);
			}

			ArtifactFieldDTO[] fieldDtos =
				artifactDtos.Select(x => new ArtifactFieldDTO
					{
						ArtifactId = x.ArtifactId,
						FieldType = longTextFieldName,
						Name = x.TextIdentifier,
						Value = null // Field RDO's don't have values...setting this to NULL to be explicit
					})
					.ToArray();

			return fieldDtos;
		}

		public async Task<ArtifactDTO[]> RetrieveFieldsAsync(int rdoTypeId, HashSet<string> fieldNames)
		{
			var fieldQuery = new Query
			{
				Fields = fieldNames.ToArray(),
				Condition = $"'Object Type Artifact Type ID' == {rdoTypeId}"
			};

			ArtifactDTO[] fieldArtifactDtos = null;
			try
			{
				fieldArtifactDtos = await RetrieveAllArtifactsAsync(fieldQuery);
			}
			catch (Exception e)
			{
				throw new Exception("Unable to retrieve fields", e);
			}

			return fieldArtifactDtos;
		}

		public async Task<ArtifactDTO[]> RetrieveFieldsAsync(int rdoTypeId, string displayName, string fieldType, HashSet<string> fieldNames)
		{
			var fieldQuery = new Query
			{
				Fields = fieldNames.ToArray(),
				Condition = $"'Object Type Artifact Type ID' == {rdoTypeId} AND 'DisplayName' == '{EscapeSingleQuote(displayName)}' AND 'Field Type' == '{fieldType}'"
			};

			ArtifactDTO[] fieldArtifactDtos = null;
			try
			{
				fieldArtifactDtos = await RetrieveAllArtifactsAsync(fieldQuery);
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

		public ArtifactDTO RetrieveField(int rdoTypeId, string displayName, string fieldType, HashSet<string> fieldNames)
		{
			ArtifactDTO[] fieldsDtos = Task.Run(() => RetrieveFieldsAsync(rdoTypeId, displayName, fieldType, fieldNames)).GetResultsWithoutContextSync();

			return fieldsDtos.FirstOrDefault();
		}

		public ResultSet<Field> Read(Field dto)
		{
			ResultSet<Field> resultSet = null;
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

		public ArtifactDTO RetrieveTheIdentifierField(int rdoTypeId)
		{
			HashSet<string> fieldsToRetrieveWhenQueryFields = new HashSet<string> {"Name", "Is Identifier"};
			ArtifactDTO[] fieldsDtos = RetrieveFieldsAsync(rdoTypeId, fieldsToRetrieveWhenQueryFields).GetResultsWithoutContextSync();
			ArtifactDTO identifierField = fieldsDtos.First(field => Convert.ToBoolean(field.Fields[1].Value));
			return identifierField;
		}

		public ArtifactFieldDTO[] RetrieveBeginBatesFields()
		{
			IEnumerable<ArtifactFieldDTO> artifactFieldDTOs;
			using (IFieldManager fieldManagerProxy =
				_servicesMgr.CreateProxy<IFieldManager>(ExecutionIdentity.System))
			{
				var result = fieldManagerProxy.RetrieveBeginBatesFieldsAsync(_workspaceArtifactId).Result;
				artifactFieldDTOs = result.Select(x => new ArtifactFieldDTO
				{
					ArtifactId = x.ArtifactID,
					Name = x.Name
				});
			}

			return artifactFieldDTOs.ToArray();
		}

		public int? RetrieveArtifactViewFieldId(int fieldArtifactId)
		{
			using (IFieldManager fieldManagerProxy =
				_servicesMgr.CreateProxy<IFieldManager>(ExecutionIdentity.System))
			{
				return fieldManagerProxy.RetrieveArtifactViewFieldIdAsync(_workspaceArtifactId, fieldArtifactId).Result;
			}
		}
	}
}