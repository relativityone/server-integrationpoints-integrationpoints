using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Common.Context;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Web.Attributes;
using Relativity;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.FieldsMapping.ImportApi;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class FolderPathController : ApiController
	{
		private readonly IFieldService _fieldService;
		private readonly IChoiceService _choiceService;
		private readonly IWorkspaceContext _workspaceIdProvider;
		private readonly IImportApiFacade _importApiFacade;

		public FolderPathController(
			IFieldService fieldService,
			IChoiceService choiceService,
			IWorkspaceContext workspaceIdProvider,
			IImportApiFacade importApiFacade
			)
		{
			_fieldService = fieldService;
			_choiceService = choiceService; 
			_workspaceIdProvider = workspaceIdProvider;
			_importApiFacade = importApiFacade;
		}
		
		[HttpGet]
		[LogApiExceptionFilter(Message = "Unable to retrieve fields data.")]
		public HttpResponseMessage GetFields()
		{
			List<FieldEntry> textFields = _fieldService.GetAllTextFields(_workspaceIdProvider.GetWorkspaceID(), (int)ArtifactType.Document).ToList();

			IEnumerable<FieldEntry> textMappableFields = GetFieldCategory(textFields);

			return Request.CreateResponse(HttpStatusCode.OK, textMappableFields, Configuration.Formatters.JsonFormatter);
		}

		[HttpGet]
		[LogApiExceptionFilter(Message = "Unable to retrieve long text fields data.")]
		public HttpResponseMessage GetLongTextFields()
		{
			List<FieldEntry> textFields = _fieldService.GetLongTextFields(_workspaceIdProvider.GetWorkspaceID(), (int)ArtifactType.Document).ToList();

			IEnumerable<FieldEntry> textMappableFields = GetFieldCategory(textFields);

			return Request.CreateResponse(HttpStatusCode.OK, textMappableFields, Configuration.Formatters.JsonFormatter);
		}

		[HttpGet]
		[LogApiExceptionFilter(Message = "Unable to retrieve choice fields data.")]
		public HttpResponseMessage GetChoiceFields()
		{
			List<FieldEntry> choiceFields = _choiceService.GetChoiceFields(_workspaceIdProvider.GetWorkspaceID(),(int)ArtifactType.Document);

			IEnumerable<FieldEntry> choiceMappableFields = GetFieldCategory(choiceFields);

			return Request.CreateResponse(HttpStatusCode.OK, choiceMappableFields, Configuration.Formatters.JsonFormatter);
		}

		private IEnumerable<FieldEntry> GetTextMappableFields(int workspaceId, List<FieldEntry> textFields)
		{
			HashSet<int> mappableArtifactIds = _importApiFacade.GetMappableArtifactIdsWithNotIdentifierFieldCategory(workspaceId, Convert.ToInt32(ArtifactType.Document));
			return textFields.Where(x => mappableArtifactIds.Contains(Convert.ToInt32(x.FieldIdentifier)));
		}

		private IEnumerable<FieldEntry> GetFieldCategory(List<FieldEntry> textFields)
		{
			int workspaceId = _workspaceIdProvider.GetWorkspaceID();
			return GetTextMappableFields(workspaceId, textFields);
		}
	}
}
