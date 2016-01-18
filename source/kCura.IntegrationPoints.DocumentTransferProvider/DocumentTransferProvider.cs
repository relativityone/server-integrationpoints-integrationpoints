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

namespace kCura.IntegrationPoints.DocumentTransferProvider
{
	[Contracts.DataSourceProvider(Shared.Constants.PROVIDER_GUID)]
	public class DocumentTransferProvider : IDataSourceProvider
	{
		public IEnumerable<FieldEntry> GetFields(string options)
		{
			// TODO: Make this work with some type of RSAPI connection
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
			var fields = query.GetFieldsForRDO(rdoTypeId);
			var mappableFields = GetImportAPI(client).GetWorkspaceFields(workspaceId, rdoTypeId);
			return fields.Where(x => mappableFields.Any(y => y.ArtifactID == x.ArtifactID)).ToList();
		}

		private IEnumerable<FieldEntry> ParseFields(List<Artifact> fields)
		{
			foreach (var result in fields)
			{
				var idField = result.Fields.FirstOrDefault(x => x.Name.Equals("Is Identifier"));
				bool isIdentifier = false;
				if (idField != null)
				{
					isIdentifier = Convert.ToInt32(idField.Value) == 1;
					if (isIdentifier)
					{
						result.Name += " [Object Identifier]";
					}
				}
				yield return new FieldEntry() { DisplayName = result.Name, FieldIdentifier = result.ArtifactID.ToString(), IsIdentifier = isIdentifier, IsRequired = false };
			}
		}

		private IRSAPIClient CreateClient(int workspaceId)
		{
			string localHostFqdn = System.Net.Dns.GetHostEntry("localhost").HostName;
			Uri endpointUri = new Uri(string.Format("http://{0}/relativity.services", localHostFqdn));
			IRSAPIClient rsapiClient = new RSAPIClient(endpointUri, new IntegratedAuthCredentials());
			rsapiClient.APIOptions.WorkspaceID = workspaceId;
			return rsapiClient;
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
				return new DocumentTranfserDataReader(relativityClient, entryIds.Select(x => Convert.ToInt32(x)), fields);
			}

		}
	}
}