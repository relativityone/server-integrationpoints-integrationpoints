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

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class FieldRepository : KeplerServiceBase, IFieldRepository
	{
		private readonly IHelper _helper;
		private readonly int _workspaceArtifactId;

		public FieldRepository(
			IHelper helper,
			IObjectQueryManagerAdaptor objectQueryManagerAdaptor, 
			int workspaceArtifactId) : base(objectQueryManagerAdaptor)
		{
			_helper = helper;
			_workspaceArtifactId = workspaceArtifactId;
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

		public ArtifactDTO RetrieveTheIdentifierField(int rdoTypeId)
		{
			HashSet<string> fieldsToRetrieveWhenQueryFields = new HashSet<string>() { "Name", "Is Identifier" };
			ArtifactDTO[] fieldsDtos = RetrieveFieldsAsync(rdoTypeId, fieldsToRetrieveWhenQueryFields).GetResultsWithoutContextSync();
			ArtifactDTO identifierField = fieldsDtos.First(field => Convert.ToBoolean(field.Fields[1].Value));
			return identifierField;
		}
	}
}