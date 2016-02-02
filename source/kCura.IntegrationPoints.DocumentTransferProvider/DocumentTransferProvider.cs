using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.DocumentTransferProvider.Adaptors;
using kCura.IntegrationPoints.DocumentTransferProvider.Adaptors.Implementations;
using kCura.IntegrationPoints.DocumentTransferProvider.DataReaders;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.Relativity.Client;
using kCura.Relativity.ImportAPI;
using Newtonsoft.Json;
using Artifact = kCura.Relativity.Client.Artifact;
using Field = kCura.Relativity.Client.Field;
using Relativity.API;

namespace kCura.IntegrationPoints.DocumentTransferProvider
{
	public class DestinationConfigurationModel
	{
		public int ArtifactId;
		public string ImportOverwriteMode;
		public int CaseArtifactId;
		public bool CustodianManagerFieldContainsLink;
		public bool ExtractedTextFieldContainsFilePath;
	}

	[Contracts.DataSourceProvider(Shared.Constants.PROVIDER_GUID)]
	public class DocumentTransferProvider : IDataSourceProvider
	{
		private readonly IHelper _helper;

		public DocumentTransferProvider(IHelper helper)
		{
			_helper = helper;
		}

		public IEnumerable<FieldEntry> GetFields(string options)
		{
			DocumentTransferSettings settings = JsonConvert.DeserializeObject<DocumentTransferSettings>(options);
			using (IRSAPIClient client = CreateClient(settings.WorkspaceArtifactId))
			{
				List<Artifact> fields = GetRelativityFields(client, settings.WorkspaceArtifactId, Convert.ToInt32(ArtifactType.Document));
				IEnumerable<FieldEntry> fieldEntries = ParseFields(fields);
				return fieldEntries;
			}
		}

		private List<Relativity.Client.Artifact> GetRelativityFields(IRSAPIClient client, int workspaceId, int rdoTypeId)
		{
			RelativityFieldQuery query = new RelativityFieldQuery(client);
			List<Artifact> fields = query.GetFieldsForRdo(rdoTypeId);
			HashSet<int> mappableArtifactIds = new HashSet<int>(GetImportAPI(client).GetWorkspaceFields(workspaceId, rdoTypeId).Select(x => x.ArtifactID));

			// Contains is 0(1) https://msdn.microsoft.com/en-us/library/kw5aaea4.aspx
			return fields.Where(x => mappableArtifactIds.Contains(x.ArtifactID)).ToList();
		}

		private IEnumerable<FieldEntry> ParseFields(List<Artifact> fields)
		{
			foreach (Artifact result in fields)
			{
				Field idField = result.Fields.FirstOrDefault(x => x.Name.Equals("Is Identifier"));
				bool isIdentifier = false;
				if (idField != null)
				{
					isIdentifier = Convert.ToInt32(idField.Value) == 1;
					if (isIdentifier)
					{
						result.Name += Shared.Constants.OBJECT_IDENTIFIER_APPENDAGE_TEXT;
					}
				}
				yield return new FieldEntry() { DisplayName = result.Name, FieldIdentifier = result.ArtifactID.ToString(), IsIdentifier = isIdentifier, IsRequired = false };
			}
		}

		private IRSAPIClient CreateClient(int workspaceId)
		{
			IRSAPIClient client = _helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser);
			client.APIOptions.WorkspaceID = workspaceId;

			return client;
		}

		private IImportAPI GetImportAPI(IRSAPIClient client)
		{
			string username = "XxX_BearerTokenCredentials_XxX";
			//ReadResult readResult = client.GenerateRelativityAuthenticationToken(client.APIOptions);
			//string authToken = readResult.Artifact.getFieldByName("AuthenticationToken").ToString();
			string authToken = System.Security.Claims.ClaimsPrincipal.Current.Claims.Single(x => x.Type.Equals("access_token")).Value;
			return new ExtendedImportAPI(username, authToken, "http://localhost/RelativityWebAPI/");
		}

		/// <summary>
		/// Gets all of the artifact ids that can be batched in reads
		/// </summary>
		/// <param name="identifier">The identifying field (Control Number)</param>
		/// <param name="options">The artifactId of the saved search in string format</param>
		/// <returns>An IDataReader containing all of the saved search's document artifact ids</returns>
		public IDataReader GetBatchableIds(FieldEntry identifier, string options)
		{
			DocumentTransferSettings settings = JsonConvert.DeserializeObject<DocumentTransferSettings>(options);
			using (IRSAPIClient client = CreateClient(settings.WorkspaceArtifactId))
			{
				IRelativityClientAdaptor relativityClient = new RelativityClientAdaptor(client);
				return new DocumentArtifactIdDataReader(relativityClient, settings.SavedSearchArtifactId);
			}
		}

		/// <summary>
		/// Gets the RDO's who's artifact ids exist in the entryIds list
		/// (This method is called in batches of normally 1000 entryIds)
		/// </summary>
		/// <param name="fields">The fields the user mapped</param>
		/// <param name="entryIds">The artifact ids of the documents to copy (in string format)</param>
		/// <param name="options">The saved search artifact id (unused in this method)</param>
		/// <returns>An IDataReader that contains the Document RDO's for the entryIds</returns>
		public IDataReader GetData(IEnumerable<FieldEntry> fields, IEnumerable<string> entryIds, string options)
		{
			DocumentTransferSettings settings = JsonConvert.DeserializeObject<DocumentTransferSettings>(options);

			using (IRSAPIClient client = CreateClient(settings.WorkspaceArtifactId))
			{
				IRelativityClientAdaptor relativityClient = new RelativityClientAdaptor(client);

				// TODO - modify query to only get 'Field Type'. SAMO - 1/29/2016
				//List<Artifact> documentFields = GetExtractedTextFields(client, (int)ArtifactType.Document);

				List<Artifact> fieldEntries = GetLongTextFields(client, Convert.ToInt32(ArtifactType.Document));
				return new DocumentTranfserDataReader(relativityClient, entryIds.Select(x => Convert.ToInt32(x)), fields, fieldEntries);
			}
		}

		private List<Artifact> GetLongTextFields(IRSAPIClient client, int rdoTypeId)
		{
			CompositeCondition condition = new CompositeCondition()
			{
				Condition1 =
					new ObjectCondition
					{
						Field = "Object Type Artifact Type ID",
						Operator = ObjectConditionEnum.AnyOfThese,
						Value = new List<int> { rdoTypeId }
					},
				Operator = CompositeConditionEnum.And,
				Condition2 =
					new TextCondition()
					{
						Field = "Field Type",
						Operator = TextConditionEnum.EqualTo,
						Value = "Long Text"
					}
			};

			Query query = new Query()
			{
				ArtifactTypeName = "Field",
				Fields = new List<Field>(),
				Condition = condition
			};

			var result = client.Query(client.APIOptions, query);
			if (!result.Success)
			{
				var messages = result.Message;
				var e = messages;
				throw new Exception(e);
			}
			return result.QueryArtifacts;
		}
	}
}