using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.Core.Services.RDO;
using kCura.IntegrationPoints.DocumentTransferProvider.Adaptors;
using kCura.IntegrationPoints.DocumentTransferProvider.Adaptors.Implementations;
using kCura.IntegrationPoints.DocumentTransferProvider.DataReaders;
using kCura.Relativity.Client;
using kCura.Relativity.ImportAPI;
using Newtonsoft.Json;
using Relativity.API;
using Relativity.Services.ObjectQuery;
using Query = Relativity.Services.ObjectQuery.Query;

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

	[Contracts.DataSourceProvider(Shared.Constants.RELATIVITY_PROVIDER_GUID)]
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
			QueryDataItemResult[] fields = GetRelativityFields(settings.SourceWorkspaceArtifactId, Convert.ToInt32(ArtifactType.Document));
			IEnumerable<FieldEntry> fieldEntries = ParseFields(fields);
			return fieldEntries;
		}

		private QueryDataItemResult[] GetRelativityFields(int workspaceId, int rdoTypeId)
		{
			IRDORepository rdoRepository = new RDORepository(_helper.GetServicesManager().CreateProxy<IObjectQueryManager>(ExecutionIdentity.System), workspaceId, rdoTypeId);
			var fieldQuery = new Query()
			{
				Fields = new []{ "Name", "Choices", "Object Type Artifact Type ID", "Field Type", "Field Type ID", "Is Identifier", "Field Type Name"},
				Condition = $"'Object Type Artifact Type ID' == {rdoTypeId}"
			};
			ObjectQueryResutSet fields = rdoRepository.RetrieveAsync(fieldQuery, String.Empty).Result;
			if (!fields.Success)
			{
				var messages = fields.Message;
				var e = messages; 
				throw new Exception(e);	
			}

			HashSet<int> mappableArtifactIds = new HashSet<int>(GetImportAPI().GetWorkspaceFields(workspaceId, rdoTypeId).Select(x => x.ArtifactID));

			// Contains is 0(1) https://msdn.microsoft.com/en-us/library/kw5aaea4.aspx
			return fields.Data.DataResults.Where(x => mappableArtifactIds.Contains(x.ArtifactId)).ToArray();
		}

		private IEnumerable<FieldEntry> ParseFields(QueryDataItemResult[] fields)
		{
			foreach (QueryDataItemResult result in fields)
			{
				DataItemFieldResult idField = result.Fields.FirstOrDefault(x => x.Name.Equals("Is Identifier"));
				bool isIdentifier = false;
				if (idField != null)
				{
					isIdentifier = Convert.ToInt32(idField.Value) == 1;
					if (isIdentifier)
					{
						idField.Name += Shared.Constants.OBJECT_IDENTIFIER_APPENDAGE_TEXT;
					}
				}
				yield return new FieldEntry() { DisplayName = result.TextIdentifier, FieldIdentifier = result.ArtifactId.ToString(), IsIdentifier = isIdentifier, IsRequired = false };
			}
		}

		private IImportAPI GetImportAPI()
		{
			const string username = "XxX_BearerTokenCredentials_XxX";
			string authToken = System.Security.Claims.ClaimsPrincipal.Current.Claims.Single(x => x.Type.Equals("access_token")).Value;

			// TODO: we need to make IIntegrationPointsConfig a dependency or use a factory -- biedrzycki: Feb 16th, 2016
			IIntegrationPointsConfig config = new ConfigAdapter(_helper.GetDBContext(-1));
			return new ExtendedImportAPI(username, authToken, config.GetWebApiUrl);
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
			// TODO: DI or factory
			IRDORepository repository = new RDORepository(_helper.GetServicesManager().CreateProxy<IObjectQueryManager>(ExecutionIdentity.System), settings.SourceWorkspaceArtifactId, Convert.ToInt32(ArtifactType.Document));
			IDataReader dataReader = new DocumentArtifactIdDataReader(repository, settings.SavedSearchArtifactId);

			return dataReader;
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

			// TODO: DI or factory
			int fieldTypeArtifactId = Convert.ToInt32(ArtifactType.Field);
            IRDORepository repository = new RDORepository(_helper.GetServicesManager().CreateProxy<IObjectQueryManager>(ExecutionIdentity.System), settings.SourceWorkspaceArtifactId, fieldTypeArtifactId);
			QueryDataItemResult[] longTextfieldEntries = GetLongTextFields(repository, fieldTypeArtifactId);
            repository = new RDORepository(_helper.GetServicesManager().CreateProxy<IObjectQueryManager>(ExecutionIdentity.System), settings.SourceWorkspaceArtifactId, Convert.ToInt32(ArtifactType.Document));
			IDataReader dataReader =  new DocumentTranfserDataReader(repository, entryIds.Select(x => Convert.ToInt32(x)), fields, longTextfieldEntries);

			return dataReader;
		}

		private QueryDataItemResult[] GetLongTextFields(IRDORepository rdoRepository, int rdoTypeId)
		{
			var longTextFieldsQuery = new global::Relativity.Services.ObjectQuery.Query()
			{
				Condition = $"('Object Type Artifact Type ID' == {rdoTypeId} AND 'Field Type' == 'Long Text')",
			};

			ObjectQueryResutSet result = rdoRepository.RetrieveAsync(longTextFieldsQuery, String.Empty).Result;

			if (!result.Success)
			{
				var messages = result.Message;
				var e = messages;
				throw new Exception(e);
			}
			return result.Data.DataResults;
		}
	}
}