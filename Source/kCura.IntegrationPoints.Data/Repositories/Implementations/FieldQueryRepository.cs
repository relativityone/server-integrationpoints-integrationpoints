using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.RSAPIClient;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.API;
using Relativity.Services.FieldManager;
using Relativity.Services.Objects.DataContracts;
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
			IRelativityObjectManager relativityObjectManager,
			int workspaceArtifactId) : base(relativityObjectManager)
		{
			_helper = helper;
			_servicesMgr = servicesMgr;
			_workspaceArtifactId = workspaceArtifactId;
		}

		public async Task<ArtifactFieldDTO[]> RetrieveLongTextFieldsAsync(int rdoTypeId)
		{
			const string longTextFieldName = "Long Text";

			var longTextFieldsQuery = new QueryRequest()
			{
				ObjectType = new ObjectTypeRef { ArtifactTypeID = (int)ArtifactType.Field },
				Condition = string.Format("'Object Type Artifact Type ID' == OBJECT {0} AND 'Field Type' == '{1}'", rdoTypeId, longTextFieldName)
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
			var fieldQuery = new QueryRequest
			{
				ObjectType = new ObjectTypeRef { ArtifactTypeID = (int)ArtifactType.Field },
				Fields = fieldNames.Select(x => new FieldRef() { Name = x }),
				Condition = $"'Object Type Artifact Type Id' == OBJECT {rdoTypeId}"
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
			var fieldQuery = new QueryRequest
			{
				ObjectType = new ObjectTypeRef { ArtifactTypeID = (int)ArtifactType.Field },
				Fields = fieldNames.Select(x => new FieldRef {Name = x}),
				Condition = $"'Object Type Artifact Type ID' == OBJECT {rdoTypeId} AND 'DisplayName' == '{EscapeSingleQuote(displayName)}' AND 'Field Type' == '{fieldType}'"
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
			var rsapiClientFactory = new RsapiClientFactory();
			using (IRSAPIClient rsapiClient = rsapiClientFactory.CreateUserClient(_helper))
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