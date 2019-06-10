using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.Converters;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.RSAPIClient;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.API;
using Relativity.Services.FieldManager;
using Relativity.Services.Objects.DataContracts;
using Field = kCura.Relativity.Client.DTOs.Field;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class FieldQueryRepository : IFieldQueryRepository
	{
		private readonly IHelper _helper;
		private readonly IServicesMgr _servicesMgr;
		private readonly IRelativityObjectManager _relativityObjectManager;
		private readonly int _workspaceArtifactID;

		public FieldQueryRepository(
			IHelper helper,
			IServicesMgr servicesMgr,
			IRelativityObjectManager relativityObjectManager,
			int workspaceArtifactId)
		{
			_helper = helper;
			_servicesMgr = servicesMgr;
			_relativityObjectManager = relativityObjectManager;
			_workspaceArtifactID = workspaceArtifactId;
		}

		public async Task<ArtifactFieldDTO[]> RetrieveLongTextFieldsAsync(int rdoTypeID)
		{
			const string longTextFieldName = "Long Text";

			var longTextFieldsQuery = new QueryRequest
			{
				ObjectType = new ObjectTypeRef { ArtifactTypeID = (int)ArtifactType.Field },
				Condition = $"'Object Type Artifact Type ID' == OBJECT {rdoTypeID} AND 'Field Type' == '{longTextFieldName}'"
			};

			IEnumerable<RelativityObject> relativityObjects = await _relativityObjectManager
				.QueryAsync(longTextFieldsQuery)
				.ConfigureAwait(false);

			ArtifactFieldDTO[] fieldDtos = relativityObjects
				.Select(x => ConvertArtifactDtoToArtifactFieldDto(x, longTextFieldName))
				.ToArray();

			return fieldDtos;
		}

		public Task<ArtifactDTO[]> RetrieveFieldsAsync(int rdoTypeID, HashSet<string> fieldNames)
		{
			var fieldQuery = new QueryRequest
			{
				ObjectType = new ObjectTypeRef { ArtifactTypeID = (int)ArtifactType.Field },
				Fields = fieldNames.Select(x => new FieldRef { Name = x }),
				Condition = $"'Object Type Artifact Type Id' == OBJECT {rdoTypeID}"
			};

			return _relativityObjectManager
				.QueryAsync(fieldQuery)
				.ToArtifactDTOsArrayAsyncDeprecated();
		}

		public ArtifactDTO[] RetrieveFields(int rdoTypeID, HashSet<string> fieldNames)
		{
			return RetrieveFieldsAsync(rdoTypeID, fieldNames).GetAwaiter().GetResult();
		}

		public ArtifactDTO RetrieveField(int rdoTypeID, string displayName, string fieldType, HashSet<string> fieldNames)
		{
			ArtifactDTO[] fieldsDtos = RetrieveFieldsAsync(rdoTypeID, displayName, fieldType, fieldNames).GetAwaiter().GetResult();
			return fieldsDtos.FirstOrDefault();
		}

		public ResultSet<Field> Read(Field dto)
		{
			var rsapiClientFactory = new RsapiClientFactory();
			using (IRSAPIClient rsapiClient = rsapiClientFactory.CreateUserClient(_helper))
			{
				rsapiClient.APIOptions.WorkspaceID = _workspaceArtifactID;

				try
				{
					return rsapiClient.Repositories.Field.Read(dto);
				}
				catch (Exception e)
				{
					throw new IntegrationPointsException("Unable to read Field dto", e)
					{
						ExceptionSource = IntegrationPointsExceptionSource.RSAPI
					};
				}
			}
		}

		public ArtifactDTO RetrieveTheIdentifierField(int rdoTypeID)
		{
			HashSet<string> fieldsToRetrieveWhenQueryFields = new HashSet<string> { "Name", "Is Identifier" };
			ArtifactDTO[] fieldsDtos = RetrieveFieldsAsync(rdoTypeID, fieldsToRetrieveWhenQueryFields).GetAwaiter().GetResult();
			ArtifactDTO identifierField = fieldsDtos.First(field => Convert.ToBoolean(field.Fields[1].Value));
			return identifierField;
		}

		public ArtifactFieldDTO[] RetrieveBeginBatesFields()
		{
			using (IFieldManager fieldManagerProxy = _servicesMgr.CreateProxy<IFieldManager>(ExecutionIdentity.System))
			{
				IEnumerable<global::Relativity.Services.Field.FieldRef> result = fieldManagerProxy.RetrieveBeginBatesFieldsAsync(_workspaceArtifactID).GetAwaiter().GetResult();
				return result.Select(ConvertFieldRefToArtifactFieldDto).ToArray();
			}
		}

		public int? RetrieveArtifactViewFieldId(int fieldArtifactID)
		{
			using (IFieldManager fieldManagerProxy = _servicesMgr.CreateProxy<IFieldManager>(ExecutionIdentity.System))
			{
				return fieldManagerProxy.RetrieveArtifactViewFieldIdAsync(_workspaceArtifactID, fieldArtifactID).GetAwaiter().GetResult();
			}
		}

		private Task<ArtifactDTO[]> RetrieveFieldsAsync(int rdoTypeID, string displayName, string fieldType, HashSet<string> fieldNames)
		{
			var fieldQuery = new QueryRequest
			{
				ObjectType = new ObjectTypeRef { ArtifactTypeID = (int)ArtifactType.Field },
				Fields = fieldNames.Select(x => new FieldRef { Name = x }),
				Condition = $"'Object Type Artifact Type ID' == OBJECT {rdoTypeID} AND 'DisplayName' == '{displayName.EscapeSingleQuote()}' AND 'Field Type' == '{fieldType}'"
			};

			return _relativityObjectManager
				.QueryAsync(fieldQuery)
				.ToArtifactDTOsArrayAsyncDeprecated();
		}

		private static ArtifactFieldDTO ConvertFieldRefToArtifactFieldDto(global::Relativity.Services.Field.FieldRef artifactDto)
		{
			return new ArtifactFieldDTO
			{
				ArtifactId = artifactDto.ArtifactID,
				Name = artifactDto.Name,
			};
		}

		private static ArtifactFieldDTO ConvertArtifactDtoToArtifactFieldDto(RelativityObject relativityObject, string fieldTypeName)
		{
			return new ArtifactFieldDTO
			{
				ArtifactId = relativityObject.ArtifactID,
				FieldType = fieldTypeName,
				Name = relativityObject.Name,
				Value = null // Field RDO's don't have values...setting this to NULL to be explicit
			};
		}
	}
}