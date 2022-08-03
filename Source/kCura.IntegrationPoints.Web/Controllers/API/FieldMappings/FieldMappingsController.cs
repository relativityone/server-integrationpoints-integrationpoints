using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.IntegrationPoints.Web.Models;
using Relativity.API;
using Relativity.IntegrationPoints.FieldsMapping;
using Relativity.IntegrationPoints.FieldsMapping.Metrics;
using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace kCura.IntegrationPoints.Web.Controllers.API.FieldMappings
{
    public class FieldMappingsController : ApiController
    {
        private const string _AUTOMAP_ALL_METRIC_NAME = "AutoMapAll";
        private const string _AUTOMAP_SAVED_SEARCH_METRIC_NAME = "AutoMapSavedSearch";
        private const string _AUTOMAP_VIEW_METRIC_NAME = "AutoMapView";
        private const string _INVALID_MAPPING_METRIC_NAME = "InvalidMapping";

        private readonly IFieldsClassifyRunnerFactory _fieldsClassifyRunnerFactory;
        private readonly IAutomapRunner _automapRunner;
        private readonly IFieldsMappingValidator _fieldsMappingValidator;
        private readonly IMetricsSender _metricsSender;
        private readonly IMetricBucketNameGenerator _metricBucketNameGenerator;
        private readonly IAPILog _logger;

        public FieldMappingsController(IFieldsClassifyRunnerFactory fieldsClassifyRunnerFactory, IAutomapRunner automapRunner, IFieldsMappingValidator fieldsMappingValidator, IMetricsSender metricsSender,
            IMetricBucketNameGenerator metricBucketNameGenerator, IAPILog logger)
        {
            _fieldsClassifyRunnerFactory = fieldsClassifyRunnerFactory;
            _automapRunner = automapRunner;
            _fieldsMappingValidator = fieldsMappingValidator;
            _metricsSender = metricsSender;
            _metricBucketNameGenerator = metricBucketNameGenerator;
            _logger = logger;
        }

        [HttpGet]
        [LogApiExceptionFilter(Message = "Error while retrieving fields from source workspace.")]
        public async Task<HttpResponseMessage> GetMappableFieldsFromSourceWorkspace(int workspaceID, int artifactTypeId)
        {
            IFieldsClassifierRunner fieldsClassifierRunner = _fieldsClassifyRunnerFactory.CreateForSourceWorkspace(artifactTypeId);

            IEnumerable<ClassifiedFieldDTO> filteredFields = (await fieldsClassifierRunner.GetFilteredFieldsAsync(workspaceID, artifactTypeId).ConfigureAwait(false))
                .Select(x => new ClassifiedFieldDTO(x));

            return Request.CreateResponse(HttpStatusCode.OK, filteredFields, Configuration.Formatters.JsonFormatter);
        }

        [HttpGet]
        [LogApiExceptionFilter(Message = "Error while retrieving fields from destination workspace.")]
        public async Task<HttpResponseMessage> GetMappableFieldsFromDestinationWorkspace(int workspaceID, int artifactTypeId)
        {
            IFieldsClassifierRunner fieldsClassifierRunner = _fieldsClassifyRunnerFactory.CreateForDestinationWorkspace(artifactTypeId);

            IEnumerable<ClassifiedFieldDTO> filteredFields = (await fieldsClassifierRunner.GetFilteredFieldsAsync(workspaceID, artifactTypeId).ConfigureAwait(false))
                .Select(x => new ClassifiedFieldDTO(x));

            return Request.CreateResponse(HttpStatusCode.OK, filteredFields, Configuration.Formatters.JsonFormatter);
        }

        [HttpPost]
        [LogApiExceptionFilter(Message = "Error while auto mapping fields")]
        public async Task<HttpResponseMessage> AutoMapFields([FromBody] AutomapRequest request, int workspaceID, string destinationProviderGuid)
        {
            string name = await _metricBucketNameGenerator.GetAutoMapBucketNameAsync(_AUTOMAP_ALL_METRIC_NAME, Guid.Parse(destinationProviderGuid), workspaceID).ConfigureAwait(false);
            _metricsSender.CountOperation(name);

            return Request.CreateResponse(HttpStatusCode.OK, _automapRunner.MapFields(request.SourceFields, request.DestinationFields, destinationProviderGuid, workspaceID, request.MatchOnlyIdentifiers),
                Configuration.Formatters.JsonFormatter);
        }

        [HttpPost]
        [LogApiExceptionFilter(Message = "Error while auto mapping fields from saved search")]
        public async Task<HttpResponseMessage> AutoMapFieldsFromSavedSearch([FromBody] AutomapRequest request, int sourceWorkspaceID, int savedSearchID, string destinationProviderGuid)
        {
            string name = await _metricBucketNameGenerator.GetAutoMapBucketNameAsync(_AUTOMAP_SAVED_SEARCH_METRIC_NAME, Guid.Parse(destinationProviderGuid), sourceWorkspaceID).ConfigureAwait(false);
            _metricsSender.CountOperation(name);

            IEnumerable<FieldMap> fieldMap = await _automapRunner
                .MapFieldsFromSavedSearchAsync(request.SourceFields, request.DestinationFields, destinationProviderGuid, sourceWorkspaceID, savedSearchID)
                .ConfigureAwait(false);

            return Request.CreateResponse(HttpStatusCode.OK, fieldMap, Configuration.Formatters.JsonFormatter);
        }

        [HttpPost]
        [LogApiExceptionFilter(Message = "Error while auto mapping fields from view")]
        public async Task<HttpResponseMessage> AutoMapFieldsFromView([FromBody] AutomapRequest request, int sourceWorkspaceID, int viewID, string destinationProviderGuid)
        {
            string name = await _metricBucketNameGenerator.GetAutoMapBucketNameAsync(_AUTOMAP_VIEW_METRIC_NAME, Guid.Parse(destinationProviderGuid), sourceWorkspaceID).ConfigureAwait(false);
            _metricsSender.CountOperation(name);

            IEnumerable<FieldMap> fieldMap = await _automapRunner
                .MapFieldsFromViewAsync(request.SourceFields, request.DestinationFields, destinationProviderGuid, sourceWorkspaceID, viewID)
                .ConfigureAwait(false);

            return Request.CreateResponse(HttpStatusCode.OK, fieldMap, Configuration.Formatters.JsonFormatter);
        }

        [HttpPost]
        [LogApiExceptionFilter(Message = "Error while validating mapped fields")]
        public async Task<HttpResponseMessage> ValidateAsync([FromBody] IEnumerable<FieldMap> mappedFields, int workspaceID, int destinationWorkspaceID, string destinationProviderGuid,
            int sourceArtifactTypeId, int destinationArtifactTypeId)
        {
            FieldMappingValidationResult fieldMappingValidationResult;

            try
            {
                fieldMappingValidationResult = (await _fieldsMappingValidator.ValidateAsync(mappedFields, workspaceID, destinationWorkspaceID, sourceArtifactTypeId, destinationArtifactTypeId).ConfigureAwait(false));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred when validating fields mapping.");
                fieldMappingValidationResult = new FieldMappingValidationResult();
            }

            if (fieldMappingValidationResult.InvalidMappedFields.Any())
            {
                string name = _metricBucketNameGenerator.GetAutoMapBucketNameAsync(_INVALID_MAPPING_METRIC_NAME, Guid.Parse(destinationProviderGuid), workspaceID).GetAwaiter().GetResult();
                _metricsSender.CountOperation(name);
            }

            return Request.CreateResponse(HttpStatusCode.OK, fieldMappingValidationResult, Configuration.Formatters.JsonFormatter);
        }
    }
}