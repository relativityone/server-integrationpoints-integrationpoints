#pragma warning disable CS0618 // Type or member is obsolete (IRSAPI deprecation)
#pragma warning disable CS0612 // Type or member is obsolete (IRSAPI deprecation)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Common.Context;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Synchronizers.RDO.ImportAPI;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.Relativity.Client;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class FolderPathController : ApiController
	{
		private const string _GET_WORKSPACE_ID_ERROR = "{method}: returned workspacedId {wid}, differs from old workspace id {owid}. Using the old one as a fallback.";

		private readonly IRSAPIClient _client;
		private readonly IFieldService _fieldService;
		private readonly IChoiceService _choiceService;
		private readonly IWorkspaceContext _workspaceIdProvider;
		private readonly IImportApiFacade _importApiFacade;
		private readonly IAPILog _logger;

		public FolderPathController(
			IRSAPIClient client,
			IFieldService fieldService,
			IChoiceService choiceService,
			IWorkspaceContext workspaceIdProvider,
			IImportApiFacade importApiFacade,
			ICPHelper helper
			)
		{
			_client = client;
			_fieldService = fieldService;
			_choiceService = choiceService; 
			_workspaceIdProvider = workspaceIdProvider;
			_importApiFacade = importApiFacade;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<FolderPathController>();
		}
		
		[HttpGet]
		[LogApiExceptionFilter(Message = "Unable to retrieve fields data.")]
		public HttpResponseMessage GetFields()
		{
			List<FieldEntry> textFields = _fieldService.GetAllTextFields(GetWorkspaceId(), (int)ArtifactType.Document).ToList();

			IEnumerable<FieldEntry> textMappableFields = GetFieldCategory(textFields);

			return Request.CreateResponse(HttpStatusCode.OK, textMappableFields, Configuration.Formatters.JsonFormatter);
		}

		[HttpGet]
		[LogApiExceptionFilter(Message = "Unable to retrieve long text fields data.")]
		public HttpResponseMessage GetLongTextFields()
		{
			List<FieldEntry> textFields = _fieldService.GetLongTextFields(GetWorkspaceId(), (int)ArtifactType.Document).ToList();

			IEnumerable<FieldEntry> textMappableFields = GetFieldCategory(textFields);

			return Request.CreateResponse(HttpStatusCode.OK, textMappableFields, Configuration.Formatters.JsonFormatter);
		}

		[HttpGet]
		[LogApiExceptionFilter(Message = "Unable to retrieve choice fields data.")]
		public HttpResponseMessage GetChoiceFields()
		{
			List<FieldEntry> choiceFields = _choiceService.GetChoiceFields(Convert.ToInt32(ArtifactType.Document));

			IEnumerable<FieldEntry> choiceMappableFields = GetFieldCategory(choiceFields);

			return Request.CreateResponse(HttpStatusCode.OK, choiceMappableFields, Configuration.Formatters.JsonFormatter);
		}

		// TODO clean after tests on prod
		private int GetWorkspaceId()
		{
			int oldWorkspaceId = _client.APIOptions.WorkspaceID;
			int workspaceId = _workspaceIdProvider.GetWorkspaceID();

			if (workspaceId != oldWorkspaceId)
			{
				LogGetWorkspaceIdError(nameof(GetWorkspaceId), workspaceId, oldWorkspaceId);
				workspaceId = oldWorkspaceId;
			}
			return workspaceId;
		}

		private IEnumerable<FieldEntry> GetTextMappableFields(int workspaceId, List<FieldEntry> textFields)
		{
			HashSet<int> mappableArtifactIds = _importApiFacade.GetMappableArtifactIdsWithNotIdentifierFieldCategory(workspaceId, Convert.ToInt32(ArtifactType.Document));
			return textFields.Where(x => mappableArtifactIds.Contains(Convert.ToInt32(x.FieldIdentifier)));
		}

		private IEnumerable<FieldEntry> GetFieldCategory(List<FieldEntry> textFields)
		{
			int workspaceId = GetWorkspaceId();
			return GetTextMappableFields(workspaceId, textFields);
		}

		// TODO remove after tests on prod
		private void LogGetWorkspaceIdError(string methodName, int workspaceId, int oldWorkspaceId)
		{
			using (_logger.LogContextPushProperty("ErrorKind", "ResultsDiscrepancy"))
			{
				_logger.LogError(_GET_WORKSPACE_ID_ERROR, methodName, workspaceId, oldWorkspaceId);
			}
		}

	}

}
#pragma warning restore CS0612 // Type or member is obsolete (IRSAPI deprecation)
#pragma warning restore CS0618 // Type or member is obsolete (IRSAPI deprecation)
