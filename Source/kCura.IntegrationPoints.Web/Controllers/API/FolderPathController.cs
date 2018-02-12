using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.IntegrationPoints.Web.Providers;
using kCura.Relativity.Client;
using kCura.Relativity.ImportAPI;
using kCura.Relativity.ImportAPI.Enumeration;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;
using Sort = Relativity.Services.Objects.DataContracts.Sort;
using Fields = kCura.IntegrationPoints.Core.Constants.Fields;
using SortEnum = Relativity.Services.Objects.DataContracts.SortEnum;

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

		private const string _GET_FIELD_CATEGORY_ERROR =
				"GetFieldCategory: returned workspacedId {wid}, differs from old workspace id {owid}. Using the old one as a fallback.";
		private const string _DIFFERENT_FIELDS_ERROR =
				"Method: {method}, Old fields ({@oldFields}) and new fields ({@newFields}) are different! Fall back to old values.";

		private readonly IRSAPIClient _client;
		private readonly IImportApiFactory _importApiFactory;
		private readonly IConfig _config;
		private readonly IFieldService _fieldService;
		private readonly IChoiceService _choiceService;
		private readonly IWorkspaceIdProvider _workspaceIdProvider;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IAPILog _logger;

		public FolderPathController(IRSAPIClient client,
			IFieldService fieldService,
			IImportApiFactory importApiFactory,
			IConfig config,
			IWorkspaceIdProvider workspaceIdProvider,
			IRepositoryFactory repositoryFactory,
			ICPHelper helper,
			IChoiceService choiceService)
		{
			_client = client;
			_importApiFactory = importApiFactory;
			_config = config;
			_fieldService = fieldService; 
			_workspaceIdProvider = workspaceIdProvider;
			_repositoryFactory = repositoryFactory;
			_choiceService = choiceService;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<FolderPathController>();
		}


		[HttpGet]
		[LogApiExceptionFilter(Message = "Unable to retrieve fields data.")]
		public HttpResponseMessage GetFields()
		{
			IImportAPI importApi = ImportApiConfiguration();
			List<FieldEntry> oldtextFields = OldGetTextFields(Convert.ToInt32(ArtifactType.Document), false);
			List<FieldEntry> textFields = _fieldService.GetTextFields(Convert.ToInt32(ArtifactType.Document), false);

			if (!AssertFieldsEquality(oldtextFields, textFields))
			{
				LogFieldsInequality(nameof(GetFields), oldtextFields, textFields);
				textFields = oldtextFields;
			}

			IEnumerable<FieldEntry> textMappableFields = GetFieldCategory(importApi, textFields);
			return Request.CreateResponse(HttpStatusCode.OK, textMappableFields, Configuration.Formatters.JsonFormatter);
		}

		[HttpGet]
		[LogApiExceptionFilter(Message = "Unable to retrieve long text fields data.")]
		public HttpResponseMessage GetLongTextFields()
		{
			IImportAPI importApi = ImportApiConfiguration();
			List<FieldEntry> oldtextFields = OldGetTextFields(Convert.ToInt32(ArtifactType.Document), true);
			List<FieldEntry> textFields = _fieldService.GetTextFields(Convert.ToInt32(ArtifactType.Document), true);

			if (!AssertFieldsEquality(oldtextFields, textFields))
			{
				LogFieldsInequality(nameof(GetLongTextFields), oldtextFields, textFields);
				textFields = oldtextFields;
			}

			IEnumerable<FieldEntry> textMappableFields = GetFieldCategory(importApi, textFields);
			return Request.CreateResponse(HttpStatusCode.OK, textMappableFields, Configuration.Formatters.JsonFormatter);
		}

		[HttpGet]
		[LogApiExceptionFilter(Message = "Unable to retrieve choice fields data.")]
		public HttpResponseMessage GetChoiceFields()
		{
			IImportAPI importApi = ImportApiConfiguration();
			List<FieldEntry> choiceFields = _choiceService.GetChoiceFields(Convert.ToInt32(ArtifactType.Document));

			IEnumerable<FieldEntry> choiceMappableFields = GetFieldCategory(importApi, choiceFields);
			return Request.CreateResponse(HttpStatusCode.OK, choiceMappableFields, Configuration.Formatters.JsonFormatter);
		}

		// TODO remove after tests on prod
		private IImportAPI ImportApiConfiguration()
		{
			var settings = new ImportSettings { WebServiceURL = _config.WebApiPath };
			IImportAPI importApi = _importApiFactory.GetImportAPI(settings);

			return importApi;
		}

		private int GetWorkspaceId()
		{
			var oldWorkspaceId = _client.APIOptions.WorkspaceID;
			var workspaceId = _workspaceIdProvider.GetWorkspaceId();

			// TODO remove after tests on prod
			if (workspaceId != oldWorkspaceId)
			{
				LogGetFieldCategoryError(workspaceId, oldWorkspaceId);
				workspaceId = oldWorkspaceId;
			}
			return workspaceId;
		}

		private IEnumerable<FieldEntry> GetTextMappableFields(int workspaceId,  List<FieldEntry> textFields)
		{
			ArtifactDTO[] workspaceFields = _repositoryFactory
				.GetFieldQueryRepository(workspaceId)
				.RetrieveFields(Convert.ToInt32(ArtifactType.Document), new HashSet<string>(new[] { Fields.IsIdentifier }));
			var mappableArtifactIds = GetMappableFieldsArtifactIDs(workspaceFields);
			return textFields.Where(x => mappableArtifactIds.Contains(Convert.ToInt32(x.FieldIdentifier)));
		}

		// TODO remove after tests on prod
		private IEnumerable<FieldEntry> GetOldTextMappableFields(IImportAPI importApi, int workspaceId, List<FieldEntry> textFields)
		{
			IEnumerable<Relativity.ImportAPI.Data.Field> oldWorkspaceFields = importApi.GetWorkspaceFields(workspaceId, Convert.ToInt32(ArtifactType.Document));
			var oldMappableArtifactIds = new HashSet<int>(oldWorkspaceFields.Where(y => y.FieldCategory != FieldCategoryEnum.Identifier).Select(x => x.ArtifactID));
			return textFields.Where(x => oldMappableArtifactIds.Contains(Convert.ToInt32(x.FieldIdentifier)));
		}

		private IEnumerable<FieldEntry> GetFieldCategory(IImportAPI importApi, List<FieldEntry> textFields)
		{
			var workspaceId = GetWorkspaceId();

			// TODO remove after tests on prod
			var oldTextMappableFields = GetOldTextMappableFields(importApi, workspaceId, textFields);

			var textMappableFields = GetTextMappableFields(workspaceId, textFields);

			// TODO remove after tests on prod
			if (!AssertFieldsEquality(oldTextMappableFields, textMappableFields))
			{
				LogFieldsInequality(nameof(GetFieldCategory), oldTextMappableFields, textMappableFields);
				textMappableFields = oldTextMappableFields;
			}

			return textMappableFields;
		}

		private HashSet<int> GetMappableFieldsArtifactIDs(IEnumerable<ArtifactDTO> fields)
		{
			Func<object, int> obj2Int = (x) => Convert.ToInt32(x);
			Func<ArtifactFieldDTO, bool> fieldIsNotIdentifier = (x) => obj2Int(x.Value) == 0; // 0 means false
			var artifactIds = fields
				.Where(x => x.Fields.Any(f => f.Name == Fields.IsIdentifier && fieldIsNotIdentifier(f)))
				.Select(y => y.ArtifactId);
			return new HashSet<int>(artifactIds);
		}

		// TODO remove after tests on prod
		private List<FieldEntry> OldGetTextFields(int rdoTypeId, bool longTextFieldsOnly)
		{
			var rdoCondition = new ObjectCondition
			{
				Field = Fields.ObjectTypeArtifactTypeId,
				Operator = ObjectConditionEnum.AnyOfThese,
				Value = new List<int> { rdoTypeId }
			};

			var longTextCondition = new TextCondition
			{
				Field = Fields.FieldType,
				Operator = TextConditionEnum.EqualTo,
				Value = FieldTypes.LongText
			};

			var fixedLengthTextCondition = new TextCondition
			{
				Field = Fields.FieldType,
				Operator = TextConditionEnum.EqualTo,
				Value = FieldTypes.FixedLengthText
			};

			var query = new Query
			{
				ArtifactTypeName = "Field",
				Fields = new List<kCura.Relativity.Client.Field>(),
				Sorts = new List<kCura.Relativity.Client.Sort>()
				{
					new kCura.Relativity.Client.Sort
					{
						Field = Fields.Name,
						Direction = kCura.Relativity.Client.SortEnum.Ascending
					}
				}
			};
			var documentLongTextCondition = new CompositeCondition(rdoCondition, CompositeConditionEnum.And, longTextCondition);
			var documentFixedLengthTextCondition = new CompositeCondition(rdoCondition, CompositeConditionEnum.And, fixedLengthTextCondition);
			query.Condition = longTextFieldsOnly ? documentLongTextCondition : new CompositeCondition(documentLongTextCondition, CompositeConditionEnum.Or, documentFixedLengthTextCondition);

			kCura.Relativity.Client.QueryResult result = _client.Query(_client.APIOptions, query);

			if (!result.Success)
			{
				throw new Exception(result.Message);
			}
			List<FieldEntry> fieldEntries = _choiceService.ConvertToFieldEntries(result.QueryArtifacts);
			return fieldEntries;
		}

		// TODO remove after tests on prod
		private void LogGetFieldCategoryError(int workspaceId, int oldWorkspaceId)
		{
			using (_logger.LogContextPushProperty("ErrorKind", "ResultsDiscrepancy"))
			{
				_logger.LogError(_GET_FIELD_CATEGORY_ERROR, workspaceId, oldWorkspaceId);
			}
		}


		// TODO remove after tests on prod
		private bool AssertFieldsEquality(IEnumerable<FieldEntry> oldFields, IEnumerable<FieldEntry> newFields)
		{
			if (oldFields == null || newFields == null)
			{
				return oldFields == newFields;
			}
			if (oldFields.Count() != newFields.Count())
			{
				return false;
			}
			var results = oldFields.Zip(newFields,
				(o, n) => o.FieldIdentifier == n.FieldIdentifier && o.DisplayName == n.DisplayName);
			return results.Count() > 0
				? results.Aggregate((x, y) => x && y)
				: true;
		}

		// TODO remove after tests on prod
		private void LogFieldsInequality(string method, IEnumerable<FieldEntry> oldFields, IEnumerable<FieldEntry> newFields)
		{
			using (_logger.LogContextPushProperty("ErrorKind", "ResultsDiscrepancy"))
			{
				_logger.LogError(_DIFFERENT_FIELDS_ERROR, method, oldFields, newFields);

			}
		}
	}

}