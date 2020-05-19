﻿using System;
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
		private const string _INVALID_MAPPING_METRIC_NAME = "InvalidMapping";

		private readonly IFieldsClassifyRunnerFactory _fieldsClassifyRunnerFactory;
		private readonly IAutomapRunner _automapRunner;
		private readonly IFieldsMappingValidator _fieldsMappingValidator;
		private readonly IMetricsSender _metricsSender;
		private readonly IAPILog _logger;

		public FieldMappingsController(IFieldsClassifyRunnerFactory fieldsClassifyRunnerFactory, IAutomapRunner automapRunner, IFieldsMappingValidator fieldsMappingValidator, IMetricsSender metricsSender,
			IAPILog logger)
		{
			_fieldsClassifyRunnerFactory = fieldsClassifyRunnerFactory;
			_automapRunner = automapRunner;
			_fieldsMappingValidator = fieldsMappingValidator;
			_metricsSender = metricsSender;
			_logger = logger;
		}

		[HttpGet]
		[LogApiExceptionFilter(Message = "Error while retrieving fields from source workspace.")]
		public async Task<HttpResponseMessage> GetMappableFieldsFromSourceWorkspace(int workspaceID)
		{
			IFieldsClassifierRunner fieldsClassifierRunner = _fieldsClassifyRunnerFactory.CreateForSourceWorkspace();

			IEnumerable<ClassifiedFieldDTO> filteredFields = (await fieldsClassifierRunner.GetFilteredFieldsAsync(workspaceID).ConfigureAwait(false))
				.Select(x => new ClassifiedFieldDTO(x));

			return Request.CreateResponse(HttpStatusCode.OK, filteredFields, Configuration.Formatters.JsonFormatter);
		}

		[HttpGet]
		[LogApiExceptionFilter(Message = "Error while retrieving fields from destination workspace.")]
		public async Task<HttpResponseMessage> GetMappableFieldsFromDestinationWorkspace(int workspaceID)
		{
			IFieldsClassifierRunner fieldsClassifierRunner = _fieldsClassifyRunnerFactory.CreateForDestinationWorkspace();

			IEnumerable<ClassifiedFieldDTO> filteredFields = (await fieldsClassifierRunner.GetFilteredFieldsAsync(workspaceID).ConfigureAwait(false))
				.Select(x => new ClassifiedFieldDTO(x));

			return Request.CreateResponse(HttpStatusCode.OK, filteredFields, Configuration.Formatters.JsonFormatter);
		}

		[HttpPost]
		[LogApiExceptionFilter(Message = "Error while auto mapping fields")]
		public HttpResponseMessage AutoMapFields([FromBody] AutomapRequest request)
		{
			_metricsSender.CountOperation(_AUTOMAP_ALL_METRIC_NAME);

			return Request.CreateResponse(HttpStatusCode.OK, _automapRunner.MapFields(request.SourceFields, request.DestinationFields, request.MatchOnlyIdentifiers), Configuration.Formatters.JsonFormatter);
		}

		[HttpPost]
		[LogApiExceptionFilter(Message = "Error while auto mapping fields from saved search")]
		public async Task<HttpResponseMessage> AutoMapFieldsFromSavedSearch([FromBody] AutomapRequest request, int sourceWorkspaceID, int savedSearchID)
		{
			_metricsSender.CountOperation(_AUTOMAP_SAVED_SEARCH_METRIC_NAME);

			IEnumerable<FieldMap> fieldMap = await _automapRunner
				.MapFieldsFromSavedSearchAsync(request.SourceFields, request.DestinationFields, sourceWorkspaceID, savedSearchID)
				.ConfigureAwait(false);
			return Request.CreateResponse(HttpStatusCode.OK, fieldMap, Configuration.Formatters.JsonFormatter);
		}

		[HttpPost]
		[LogApiExceptionFilter(Message = "Error while validating mapped fields")]
		public async Task<HttpResponseMessage> ValidateAsync([FromBody] IEnumerable<FieldMap> mappedFields, int workspaceID, int destinationWorkspaceID)
		{
			FieldMappingValidationResult fieldMappingValidationResult;

			try
			{
				fieldMappingValidationResult = (await _fieldsMappingValidator.ValidateAsync(mappedFields, workspaceID, destinationWorkspaceID).ConfigureAwait(false));
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Exception occurred when validating fields mapping.");
				fieldMappingValidationResult = new FieldMappingValidationResult();
			}

			if (fieldMappingValidationResult.InvalidMappedFields.Any())
			{
				_metricsSender.CountOperation(_INVALID_MAPPING_METRIC_NAME);
			}

			return Request.CreateResponse(HttpStatusCode.OK, fieldMappingValidationResult, Configuration.Formatters.JsonFormatter);
		}
	}
}