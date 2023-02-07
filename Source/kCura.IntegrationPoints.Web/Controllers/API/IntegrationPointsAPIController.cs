using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.IntegrationPoints.Web.Extensions;
using kCura.IntegrationPoints.Web.Helpers;
using kCura.IntegrationPoints.Web.Models;
using kCura.IntegrationPoints.Web.Models.Validation;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
    public class IntegrationPointsAPIController : ApiController
    {
        private readonly IIntegrationPointService _integrationPointService;
        private readonly IRelativityUrlHelper _urlHelper;
        private readonly Core.Services.Synchronizer.IRdoSynchronizerProvider _provider;
        private readonly IAPILog _logger;
        private readonly ICamelCaseSerializer _serializer;

        public IntegrationPointsAPIController(
            IRelativityUrlHelper urlHelper,
            Core.Services.Synchronizer.IRdoSynchronizerProvider provider,
            IIntegrationPointService integrationPointService,
            ICamelCaseSerializer serializer,
            IAPILog logger)
        {
            _urlHelper = urlHelper;
            _provider = provider;
            _integrationPointService = integrationPointService;
            _serializer = serializer;
            _logger = logger;
        }

        [HttpGet]
        [LogApiExceptionFilter(Message = "Unable to retrive integration point data.")]
        public HttpResponseMessage Get(int id)
        {
            try
            {
                var model = new IntegrationPointWebModel
                {
                    ArtifactID = id,
                    LogErrors = true,

                    // we need this hack because frontend logic rely on this:
                    // export-provider-fields-step.js: [if (typeof ip.sourceConfiguration === "string")]
                    SourceConfiguration = string.Empty,
                };

                if (id > 0)
                {
                    model = _integrationPointService.Read(id).ToWebModel(_serializer);
                }

                if (model.DestinationProvider == 0)
                {
                    model.DestinationProvider = _provider.GetRdoSynchronizerId();
                }

                return Request.CreateResponse(HttpStatusCode.Accepted, model);
            }
            catch (Exception exception)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, exception.Message);
            }
        }

        [HttpPost]
        [LogApiExceptionFilter(Message = "Unable to save or update integration point.")]
        public HttpResponseMessage Update(int workspaceID, IntegrationPointWebModel webModel, bool mappingHasWarnings = false, bool destinationWorkspaceChanged = false,
            bool clearAndProceedSelected = false, MappingType mappingType = MappingType.Loaded)
        {
            if (mappingHasWarnings)
            {
                _logger.LogWarning("Saving Integration Point with potentially invalid field mappings.");
            }

            if (destinationWorkspaceChanged)
            {
                _logger.LogInformation("Saving Integration Point Artifact ID: {IntegrationPointID} with changed destination workspace.", webModel.ArtifactID);
            }

            LogMappingInfo(mappingHasWarnings, clearAndProceedSelected, mappingType);

            int createdId;
            try
            {
                createdId = _integrationPointService.SaveIntegrationPoint(webModel.ToDto(_serializer));
            }
            catch (IntegrationPointValidationException ex)
            {
                var validationResultMapper = new ValidationResultMapper();
                ValidationResultDTO validationResultDto = validationResultMapper.Map(ex.ValidationResult);
                return Request.CreateResponse(HttpStatusCode.NotAcceptable, validationResultDto);
            }

            if (mappingHasWarnings)
            {
                _logger.LogWarning("Saved Integration Point ArtifactID: {IntegrationPointID} with potentially invalid field mappings.", createdId);
            }

            string result = _urlHelper.GetRelativityViewUrl(workspaceID, createdId, Data.ObjectTypes.IntegrationPoint);

            return Request.CreateResponse(HttpStatusCode.OK, new { returnURL = result });
        }

        private void LogMappingInfo(bool mappingHasWarnings, bool clearAndProceedSelected, MappingType mappingType)
        {
            _logger.LogInformation("Saved IntegrationPoint with following options: {options}", new
            {
                MappingHasWarnings = mappingHasWarnings,
                ClearAndProceedSelected = clearAndProceedSelected,
                MappingType = mappingType
            });
        }

        public enum MappingType
        {
            View,
            SavedSearch,
            AutoMap,
            Manual,
            Loaded
        }
    }
}
