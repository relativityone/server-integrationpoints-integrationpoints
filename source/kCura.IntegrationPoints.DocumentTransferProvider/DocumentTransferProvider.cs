﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.Contracts.RDO;
using kCura.IntegrationPoints.Data.Managers;
using kCura.IntegrationPoints.Data.Managers.Implementations;
using kCura.IntegrationPoints.DocumentTransferProvider.Adaptors;
using kCura.IntegrationPoints.DocumentTransferProvider.Adaptors.Implementations;
using kCura.IntegrationPoints.DocumentTransferProvider.DataReaders;
using kCura.Relativity.Client;
using kCura.Relativity.ImportAPI;
using Newtonsoft.Json;
using Relativity.API;
using Relativity.Services.ObjectQuery;

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
			ArtifactDTO[] fields = GetRelativityFields(settings.SourceWorkspaceArtifactId, Convert.ToInt32(ArtifactType.Document));
			IEnumerable<FieldEntry> fieldEntries = ParseFields(fields);

			return fieldEntries;
		}

		private ArtifactDTO[] GetRelativityFields(int workspaceId, int rdoTypeId)
		{
			IObjectQueryManagerAdaptor objectQueryManagerAdaptor = new ObjectQueryManagerAdaptor(_helper.GetServicesManager().CreateProxy<IObjectQueryManager>(ExecutionIdentity.System), workspaceId, Convert.ToInt32(ArtifactType.Field));
			IFieldRepository fieldRepository = new KeplerFieldRepository(objectQueryManagerAdaptor);
			ArtifactDTO[] fieldArtifacts = fieldRepository.RetrieveFieldsAsync(
				rdoTypeId,
				new HashSet<string>(new[]
				{
					Shared.Constants.Fields.Name,
					Shared.Constants.Fields.Choices,
					Shared.Constants.Fields.ObjectTypeArtifactTypeId,
					Shared.Constants.Fields.FieldType,
					Shared.Constants.Fields.FieldTypeId,
					Shared.Constants.Fields.IsIdentifier,
					Shared.Constants.Fields.FieldTypeName
				})).ConfigureAwait(false).GetAwaiter().GetResult();

			HashSet<int> mappableArtifactIds = new HashSet<int>(GetImportAPI().GetWorkspaceFields(workspaceId, rdoTypeId).Select(x => x.ArtifactID));

			// Contains is 0(1) https://msdn.microsoft.com/en-us/library/kw5aaea4.aspx
			return fieldArtifacts.Where(x => mappableArtifactIds.Contains(x.ArtifactId)).ToArray();
		}

		private IEnumerable<FieldEntry> ParseFields(ArtifactDTO[] fieldArtifacts)
		{
			foreach (ArtifactDTO fieldArtifact in fieldArtifacts)
			{
				string fieldName = String.Empty;
				int isIdentifierFieldValue = 0;

				foreach (ArtifactFieldDTO field in fieldArtifact.Fields)
				{
					if (field.Name == Shared.Constants.Fields.Name)
					{
						fieldName = field.Value as string;
					}
					else if (field.Name == Shared.Constants.Fields.IsIdentifier)
					{
						try
						{
							isIdentifierFieldValue = Convert.ToInt32(field.Value);
						}
						catch
						{
							// suppress error for invalid casts
						}
					}
				}

				bool isIdentifier = isIdentifierFieldValue > 0;
				if (isIdentifier)
				{
					fieldName += Shared.Constants.OBJECT_IDENTIFIER_APPENDAGE_TEXT;
				}

				yield return new FieldEntry()
				{
					DisplayName = fieldName,
					FieldIdentifier = fieldArtifact.ArtifactId.ToString(),
					IsIdentifier = isIdentifier,
					IsRequired = false
				};
			}
		}

		private IImportAPI GetImportAPI()
		{
			const string username = "XxX_BearerTokenCredentials_XxX";
			string authToken = System.Security.Claims.ClaimsPrincipal.Current.Claims.Single(x => x.Type.Equals("access_token")).Value;

			// TODO: we need to make IIntegrationPointsConfig a dependency or use a factory -- biedrzycki: Feb 16th, 2016
			IIntegrationPointsConfig config = new ConfigAdapter();
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
			IObjectQueryManagerAdaptor repository = new ObjectQueryManagerAdaptor(_helper.GetServicesManager().CreateProxy<IObjectQueryManager>(ExecutionIdentity.System), settings.SourceWorkspaceArtifactId, Convert.ToInt32(ArtifactType.Document));
			IRSAPIClient rsapiClient = _helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.System);
			rsapiClient.APIOptions.WorkspaceID = settings.SourceWorkspaceArtifactId;
			// TODO: create constant
			ISavedSearchRepository savedSearchRepository = new RsapiSavedSearchRepository(rsapiClient, settings.SavedSearchArtifactId, 1000);
			IDataReader dataReader = new DocumentArtifactIdDataReader(savedSearchRepository);

			return dataReader;
		}

		/// <summary>
		/// Gets the RDO's who's artifact ids exist in the entryIds list
		/// (This method is called in batches of normally 1000 entryIds)
		/// </summary>
		/// <param name="fields">The fieldArtifacts the user mapped</param>
		/// <param name="entryIds">The artifact ids of the documents to copy (in string format)</param>
		/// <param name="options">The saved search artifact id (unused in this method)</param>
		/// <returns>An IDataReader that contains the Document RDO's for the entryIds</returns>
		public IDataReader GetData(IEnumerable<FieldEntry> fields, IEnumerable<string> entryIds, string options)
		{
//			DocumentTransferSettings settings = JsonConvert.DeserializeObject<DocumentTransferSettings>(options);
//
//			// TODO: DI or factory
//			int documentTypeId = Convert.ToInt32(ArtifactType.Document);
//			IObjectQueryManagerAdaptor documentRepository = new ObjectQueryManagerAdaptor(_helper.GetServicesManager().CreateProxy<IObjectQueryManager>(ExecutionIdentity.System), settings.SourceWorkspaceArtifactId, documentTypeId);
//			IDocumentRepository documentRepository = new KeplerDocumentRepository(documentRepository);
//
//			int fieldTypeArtifactId = Convert.ToInt32(ArtifactType.Field);
//			IObjectQueryManagerAdaptor fieldRepository = new ObjectQueryManagerAdaptor(_helper.GetServicesManager().CreateProxy<IObjectQueryManager>(ExecutionIdentity.System), settings.SourceWorkspaceArtifactId, fieldTypeArtifactId);
//			IFieldRepository fieldRepository = new KeplerFieldRepository(fieldRepository);
//
//			IDBContext dbContext = _helper.GetDBContext(settings.SourceWorkspaceArtifactId);
//
//			ArtifactFieldDTO[] longTextFields = fieldRepository.RetrieveLongTextFieldsAsync(documentTypeId).ConfigureAwait(false).GetAwaiter().GetResult();

			//IDataReader dataReader = new DocumentTransferDataReader(
			//	documentRepository,
			//	entryIds.Select(x => Convert.ToInt32(x)),
			//	fields,
			//	longTextFields.Select(x => x.ArtifactId),
			//	dbContext);

			return null;
		}
	}
}