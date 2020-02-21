using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using kCura.IntegrationPoints.DocumentTransferProvider;
using kCura.IntegrationPoints.Web.Attributes;
using Relativity.IntegrationPoints.FieldsMapping;
using Relativity.IntegrationPoints.FieldsMapping.FieldClassifiers;
using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace kCura.IntegrationPoints.Web.Controllers.API.FieldMappings
{
	public class FieldMappingsController : ApiController
	{
		private readonly IFieldsClassifierRunner _fieldsClassifierRunner;
		private readonly IImportApiFactory _importApiFactory;
		private readonly IAutomapRunner _automapRunner;

		public FieldMappingsController(IFieldsClassifierRunner fieldsClassifierRunner, IImportApiFactory importApiFactory, IAutomapRunner automapRunner)
		{
			_fieldsClassifierRunner = fieldsClassifierRunner;
			_importApiFactory = importApiFactory;
			_automapRunner = automapRunner;
		}

		[HttpGet]
		[LogApiExceptionFilter(Message = "Error while retrieving fields from source workspace.")]
		public async Task<HttpResponseMessage> GetMappableFieldsFromSourceWorkspace(int workspaceID)
		{
			IList<FieldClassificationResult> sourceWorkspaceFields = await GetMappableFieldsFromSourceWorkspaceAsync(workspaceID).ConfigureAwait(false);
			return Request.CreateResponse(HttpStatusCode.OK, sourceWorkspaceFields, Configuration.Formatters.JsonFormatter);
		}

		[HttpGet]
		[LogApiExceptionFilter(Message = "Error while retrieving fields from destination workspace.")]
		public async Task<HttpResponseMessage> GetMappableFieldsFromDestinationWorkspace(int workspaceID)
		{
			IList<FieldClassificationResult> destinationWorkspaceFields = await GetMappableFieldsFromDestinationWorkspaceAsync(workspaceID).ConfigureAwait(false);
			return Request.CreateResponse(HttpStatusCode.OK, destinationWorkspaceFields, Configuration.Formatters.JsonFormatter);
		}

		[HttpPost]
		[LogApiExceptionFilter(Message = "Error while auto mapping fields")]
		public HttpResponseMessage AutoMapFields([FromBody] AutomapRequest request)
		{
			return Request.CreateResponse(HttpStatusCode.OK, _automapRunner.MapFields(request.SourceFields, request.DestinationFields, request.MatchOnlyIdentifiers), Configuration.Formatters.JsonFormatter);
		}

		[HttpPost]
		[LogApiExceptionFilter(Message = "Error while auto mapping fields from saved search")]
		public async Task<HttpResponseMessage> AutoMapFieldsFromSavedSearch([FromBody] AutomapRequest request, int sourceWorkspaceID, int savedSearchID)
		{
			IEnumerable<FieldMap> fieldMap = await _automapRunner
				.MapFieldsFromSavedSearchAsync(request.SourceFields, request.DestinationFields, sourceWorkspaceID, savedSearchID)
				.ConfigureAwait(false);
			return Request.CreateResponse(HttpStatusCode.OK, fieldMap, Configuration.Formatters.JsonFormatter);
		}
		
		private async Task<IList<FieldClassificationResult>> GetMappableFieldsFromSourceWorkspaceAsync(int workspaceID)
		{
			IList<IFieldsClassifier> sourceFieldsClassifiers = new List<IFieldsClassifier>
			{
				new RipFieldsClassifier(),
				new SystemFieldsClassifier(),
				new NotSupportedByIAPIFieldsClassifier(_importApiFactory.Create()),
				new ObjectFieldsClassifier()
			};

			IList<FieldClassificationResult> filteredFields = await _fieldsClassifierRunner.GetFilteredFieldsAsync(workspaceID, sourceFieldsClassifiers).ConfigureAwait(false);
			return filteredFields;
		}

		private async Task<IList<FieldClassificationResult>> GetMappableFieldsFromDestinationWorkspaceAsync(int workspaceID)
		{
			IList<IFieldsClassifier> destinationFieldsClassifiers = new List<IFieldsClassifier>
			{
				new RipFieldsClassifier(),
				new SystemFieldsClassifier(),
				new NotSupportedByIAPIFieldsClassifier(_importApiFactory.Create()),
				new OpenToAssociationsFieldsClassifier(),
				new ObjectFieldsClassifier()
			};

			IList<FieldClassificationResult> filteredFields = await _fieldsClassifierRunner.GetFilteredFieldsAsync(workspaceID, destinationFieldsClassifiers).ConfigureAwait(false);
			return filteredFields;
		}
	}
}