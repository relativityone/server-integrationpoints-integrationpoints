using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.IntegrationPoints.Web.DataStructures;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using kCura.Relativity.ImportAPI;
using kCura.Relativity.ImportAPI.Enumeration;
using Newtonsoft.Json;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	internal class IntegrationPointDestinationConfiguration
	{
		public bool UseFolderPathInformation;
		public int FolderPathSourceField;
	}

	internal class IntegrationPointSourceConfiguration
	{
		public int SavedSearchArtifactId;
	}

	public class FolderPathController : ApiController
	{
		private readonly IRSAPIClient _client;
		private readonly IImportApiFactory _importApiFactory;
		private readonly IConfig _config;
		private readonly IRepositoryFactory _repositoryFactory;

		public FolderPathController(IRSAPIClient client,
			IImportApiFactory importApiFactory,
			IConfig config,
			IRepositoryFactory repositoryFactory)
		{
			_client = client;
			_importApiFactory = importApiFactory;
			_config = config;
			_repositoryFactory = repositoryFactory;
		}


		[HttpGet]
		[LogApiExceptionFilter(Message = "Unable to retrieve fields data.")]
		public HttpResponseMessage GetFields()
		{
			IImportAPI importApi = ImportApiConfiguration();
			List<FieldEntry> textFields = GetTextFields(Convert.ToInt32(ArtifactType.Document), false);

			var textMappableFields = GetFieldCategory(importApi, textFields);
			return Request.CreateResponse(HttpStatusCode.OK, textMappableFields, Configuration.Formatters.JsonFormatter);
		}

		[HttpGet]
		[LogApiExceptionFilter(Message = "Unabel to retrieve long text fields data.")]
		public HttpResponseMessage GetLongTextFields()
		{
			IImportAPI importApi = ImportApiConfiguration();
			List<FieldEntry> textFields = GetTextFields(Convert.ToInt32(ArtifactType.Document), true);

			IEnumerable<FieldEntry> textMappableFields = GetFieldCategory(importApi, textFields);
			return Request.CreateResponse(HttpStatusCode.OK, textMappableFields, Configuration.Formatters.JsonFormatter);
		}

		[HttpGet]
		[LogApiExceptionFilter(Message = "Unable to retrieve fields data.")]
		public HttpResponseMessage GetFolderCount(int integrationPointArtifactId)
		{
			IIntegrationPointRepository integrationPointRepository = _repositoryFactory.GetIntegrationPointRepository(_client.APIOptions.WorkspaceID);
			IntegrationPointDTO integrationPoint = integrationPointRepository.Read(Convert.ToInt32(integrationPointArtifactId));
			IntegrationPointSourceConfiguration sourceConfiguration = JsonConvert.DeserializeObject<IntegrationPointSourceConfiguration>(integrationPoint.SourceConfiguration);
			IntegrationPointDestinationConfiguration destinationConfiguration = JsonConvert.DeserializeObject<IntegrationPointDestinationConfiguration>(integrationPoint.DestinationConfiguration);

			if (!destinationConfiguration.UseFolderPathInformation)
			{
				return Request.CreateResponse(HttpStatusCode.OK, 0, Configuration.Formatters.JsonFormatter);
			}

			ArtifactDTO[] documentDtos = GetDocumentDtos(sourceConfiguration, destinationConfiguration);
			int folderCount = GetFolderCount(documentDtos);

			return Request.CreateResponse(HttpStatusCode.OK, folderCount, Configuration.Formatters.JsonFormatter);
		}

		private IImportAPI ImportApiConfiguration()
		{
			ImportSettings settings = new ImportSettings { WebServiceURL = _config.WebApiPath };
			IImportAPI importApi = _importApiFactory.GetImportAPI(settings);

			return importApi;
		}

		private IEnumerable<FieldEntry> GetFieldCategory(IImportAPI importApi, List<FieldEntry> textFields)
		{
			IEnumerable<Relativity.ImportAPI.Data.Field> workspaceFields = importApi.GetWorkspaceFields(_client.APIOptions.WorkspaceID, Convert.ToInt32(ArtifactType.Document));
			HashSet<int> mappableArtifactIds = new HashSet<int>(workspaceFields.Where(y => y.FieldCategory != FieldCategoryEnum.Identifier).Select(x => x.ArtifactID));
			IEnumerable<FieldEntry> textMappableFields = textFields.Where(x => mappableArtifactIds.Contains(Convert.ToInt32(x.FieldIdentifier)));

			return textMappableFields;
		}


		private ArtifactDTO[] GetDocumentDtos(IntegrationPointSourceConfiguration sourceConfiguration, IntegrationPointDestinationConfiguration destinationConfiguration)
		{
			Query<Document> query = new Query<Document>
			{
				Condition = new SavedSearchCondition(sourceConfiguration.SavedSearchArtifactId),
				Fields = new List<FieldValue> { new FieldValue(destinationConfiguration.FolderPathSourceField) }
			};

			QueryResultSet<Document> resultSet = _client.Repositories.Document.Query(query, 1000);

			ArtifactDTO[] results = { };
			if (resultSet != null && resultSet.Success)
			{
				results = resultSet.Results.Select(
					x => new ArtifactDTO(
						x.Artifact.ArtifactID,
						x.Artifact.ArtifactTypeID.Value,
						"Document",
						x.Artifact.Fields.Select(
							y => new ArtifactFieldDTO() { ArtifactId = y.ArtifactID, FieldType = y.FieldType.ToString(), Name = y.Name, Value = y.Value }))
					).ToArray();

			}
			return results;
		}

		private int GetFolderCount(ArtifactDTO[] artifactDtos)
		{
			FolderTree folderTree = new FolderTree();

			foreach (ArtifactDTO document in artifactDtos)
			{
				ArtifactFieldDTO artifactFieldDto = document.Fields.FirstOrDefault();
				string folderPath = String.Empty;
				if (artifactFieldDto != null)
				{
					folderPath = artifactFieldDto.Value as string;
				}

				if (!String.IsNullOrEmpty(folderPath) && folderPath != @"\")
				{
					folderTree.AddEntry(folderPath);
				}
			}
			return folderTree.FolderCount;
		}

		private List<FieldEntry> GetTextFields(int rdoTypeId, bool longTextFieldsOnly)
		{
			var rdoCondition = new ObjectCondition
			{
				Field = "Object Type Artifact Type ID",
				Operator = ObjectConditionEnum.AnyOfThese,
				Value = new List<int> { rdoTypeId }
			};

			var longTextCondition = new TextCondition
			{
				Field = "Field Type",
				Operator = TextConditionEnum.EqualTo,
				Value = "Long Text"
			};

			var fixedLengthTextCondition = new TextCondition
			{
				Field = "Field Type",
				Operator = TextConditionEnum.EqualTo,
				Value = "Fixed-Length Text"
			};

			Query query = new Query
			{
				ArtifactTypeName = "Field",
				Fields = new List<kCura.Relativity.Client.Field>(),
				Sorts = new List<Sort>()
				{
					new Sort()
					{
						Field = "Name",
						Direction = SortEnum.Ascending
					}
				}
			};
			CompositeCondition documentLongTextCondition = new CompositeCondition(rdoCondition, CompositeConditionEnum.And, longTextCondition);
			CompositeCondition documentFixedLengthTextCondition = new CompositeCondition(rdoCondition, CompositeConditionEnum.And, fixedLengthTextCondition);
			query.Condition = longTextFieldsOnly ? documentLongTextCondition : new CompositeCondition(documentLongTextCondition, CompositeConditionEnum.Or, documentFixedLengthTextCondition);
			var result = _client.Query(_client.APIOptions, query);

			if (!result.Success)
			{
				throw new Exception(result.Message);
			}
			List<FieldEntry> fieldEntries = ConvertToFieldEntries(result.QueryArtifacts);
			return fieldEntries;
		}

		private List<FieldEntry> ConvertToFieldEntries(List<kCura.Relativity.Client.Artifact> artifacts)
		{
			List<FieldEntry> fieldEntries = new List<FieldEntry>();

			foreach (kCura.Relativity.Client.Artifact artifact in artifacts)
			{
				foreach (kCura.Relativity.Client.Field field in artifact.Fields)
				{
					if (field.Name == "Name")
					{
						fieldEntries.Add(new FieldEntry()
						{
							DisplayName = field.Value as string,
							FieldIdentifier = artifact.ArtifactID.ToString(),
							IsRequired = false
						});
						break;
					}
				}
			}
			return fieldEntries;
		}
	}
}