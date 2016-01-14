using System;
using System.Collections.Generic;
using System.Data;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Services.Syncronizer;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.IntegrationPoints.Core;
using kCura.Relativity.Client;
using IHelper = Relativity.API.IHelper;
using kCura.Apps.Common.Data;
using kCura.Apps.Common.Config;

namespace kCura.IntegrationPoints.DocumentTransferProvider
{
	//public class DocumentSynchronizer : RdoSynchronizer
	//{
	//	public DocumentSynchronizer(RelativityFieldQuery fieldQuery, ImportApiFactory factory)
	//		: base(fieldQuery, factory)
	//	{

	//	}

	//	public List<Relativity.Client.Artifact> GetRelativityFields(ImportSettings settings)
	//	{
	//		return base.GetRelativityFields(settings);
	//	}
	//}

	[Contracts.DataSourceProvider(kCura.IntegrationPoints.DocumentTransferProvider.Shared.Constants.PROVIDER_GUID)]
	public class DocumentTransferProvider : IDataSourceProvider, IInternalOnlyDataSourceProvider
	{
		private readonly IDataSyncronizerFactory _factory;

		private IHelper _client;

		public IHelper Client
		{ 
			get {return _client;}
			set { _client = value; }
		}

		public IEnumerable<FieldEntry> GetFields(string options)
		{
			int rdoTypeId = 10; // TODO: Get from options
			int workspaceId = 1105088; // TODO: Get from options

			RsapiClientFactory rsapiClientFactory = new RsapiClientFactory(_client);
			IRSAPIClient rsapiClient = rsapiClientFactory.CreateClientForWorkspace(workspaceId);
			Manager.Settings.Factory = new HelperConfigSqlServiceFactory(_client);

			RelativityFieldQuery query = new RelativityFieldQuery(rsapiClient);
			ImportApiFactory importApiFactory = new ImportApiFactory();

			RdoSynchronizer synchronizer = new RdoSynchronizer(query, importApiFactory);
			string testOptions = String.Format("{{\"artifactTypeID\":{0},\"ImportOverwriteMode\":\"AppendOverlay\",\"CaseArtifactId\":{1}}}", rdoTypeId, workspaceId);
			IEnumerable<FieldEntry> fieldEntries = synchronizer.GetFields(testOptions);

			//List<Artifact> fields = GetRelativityFields(client, workspaceId, rdoTypeId);
			//IEnumerable<FieldEntry> fieldEntries = ParseFields(fields);
			return fieldEntries;
		}

		//private List<Relativity.Client.Artifact> GetRelativityFields(IRSAPIClient client, int workspaceId, int rdoTypeId)
		//{
		//	RelativityFieldQuery query = new RelativityFieldQuery(client);
		//	var fields = query.GetFieldsForRDO(rdoTypeId);
		//	var mappableFields = GetImportAPI(client).GetWorkspaceFields(workspaceId, rdoTypeId);
		//	return fields.Where(x => mappableFields.Any(y => y.ArtifactID == x.ArtifactID)).ToList();
		//}

		//public IImportAPI GetImportAPI(IRSAPIClient client)
		//{
		//	string username = "XxX_BearerTokenCredentials_XxX";
		//	//ReadResult readResult = client.GenerateRelativityAuthenticationToken(client.APIOptions);
		//	//string authToken = readResult.Artifact.getFieldByName("AuthenticationToken").ToString();
		//	string authToken = System.Security.Claims.ClaimsPrincipal.Current.Claims.Single(x => x.Type.Equals("access_token")).Value;
		//	return new ExtendedImportAPI(username, authToken, "http://localhost/RelativityWebAPI/");
		//}

		//private List<string> IgnoredList
		//{
		//	get
		//	{
		//		// fields don't have any space in between words 
		//		var list = new List<string>
		//		{
		//			"Is System Artifact",
		//			"System Created By",
		//			"System Created On",
		//			"System Last Modified By",
		//			"System Last Modified On",
		//			"Artifact ID"
		//		};
		//		return list;
		//	}
		//}

		//protected IEnumerable<FieldEntry> ParseFields(List<Relativity.Client.Artifact> fields)
		//{
		//	foreach (var result in fields)
		//	{
		//		if (!IgnoredList.Contains(result.Name))
		//		{
		//			var idField = result.Fields.FirstOrDefault(x => x.Name.Equals("Is Identifier"));
		//			bool isIdentifier = false;
		//			if (idField != null)
		//			{
		//				isIdentifier = Convert.ToInt32(idField.Value) == 1;
		//				if (isIdentifier)
		//				{
		//					result.Name += " [Object Identifier]";
		//				}
		//			}
		//			yield return new FieldEntry() { DisplayName = result.Name, FieldIdentifier = result.ArtifactID.ToString(), IsIdentifier = isIdentifier, IsRequired = false };
		//		}
		//	}
		//}

		private IRSAPIClient CreateClient(int workspaceId)
		{
			// Create a new instance of RSAPIClient. The first parameter indicates the endpoint Uri, 
			// which indicates the scheme to use. The second parameter indicates the
			// authentication type. The RSAPIClient members page in the Services API class library
			// documents other possible constructors. The constructor also ensures a logged in session.

			string localHostFQDN = System.Net.Dns.GetHostEntry("localhost").HostName;
			Uri endpointUri = new Uri(string.Format("http://{0}/relativity.services", localHostFQDN));
			IRSAPIClient rsapiClient = new RSAPIClient(endpointUri, new IntegratedAuthCredentials());
			rsapiClient.APIOptions.WorkspaceID = workspaceId;
			return rsapiClient;
		}

		public IDataReader GetBatchableIds(FieldEntry identifier, string options)
		{
			throw new System.NotImplementedException();
		}

		public IDataReader GetData(IEnumerable<FieldEntry> fields, IEnumerable<string> entryIds, string options)
		{
			throw new NotImplementedException();
		}
	}
}