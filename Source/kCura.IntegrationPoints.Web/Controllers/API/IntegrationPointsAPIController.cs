using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Utils;
using kCura.IntegrationPoints.Core.Validation;
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
        private readonly IServiceFactory _serviceFactory;
        private readonly IRelativityUrlHelper _urlHelper;
        private readonly Core.Services.Synchronizer.IRdoSynchronizerProvider _provider;
        private readonly ICPHelper _cpHelper;
        private readonly IAPILog _logger;

        public IntegrationPointsAPIController(
            IServiceFactory serviceFactory,
            IRelativityUrlHelper urlHelper,
            Core.Services.Synchronizer.IRdoSynchronizerProvider provider,
            ICPHelper cpHelper,
            IAPILog logger)
        {
            _serviceFactory = serviceFactory;
            _urlHelper = urlHelper;
            _provider = provider;
            _cpHelper = cpHelper;
            _logger = logger;
        }

        [HttpGet]
        [LogApiExceptionFilter(Message = "Unable to retrive integration point data.")]
        public HttpResponseMessage Get(int id)
        {
            try
            {
                var model = new IntegrationPointWebModel();
                model.ArtifactID = id;
                if (id > 0)
                {
                    IIntegrationPointService integrationPointService = _serviceFactory.CreateIntegrationPointService(_cpHelper);
                    model = integrationPointService.Read(id).ToWebModel();
                }
                if (model.DestinationProvider == 0)
                {
                    model.DestinationProvider = _provider.GetRdoSynchronizerId();
                }

                RemoveInstanceToInstanceSettingsFromModel(model);

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

            IIntegrationPointService integrationPointService = _serviceFactory.CreateIntegrationPointService(_cpHelper);

            int createdId;
            try
            {
                createdId = integrationPointService.SaveIntegrationPoint(webModel.ToDto());
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

        private void RemoveInstanceToInstanceSettingsFromModel(IntegrationPointWebModel webModel)
        {
            //We need to reset the values from the database that have federated instance other than null.
            //We do not want to forward the federated instance to the user interface.
            if (!webModel.SourceConfiguration.Contains("\"FederatedInstanceArtifactId\":null") &&
                webModel.SourceConfiguration.Contains("FederatedInstanceArtifactId"))
            {
                webModel.SourceConfiguration = null;
            }
            else if (webModel.SourceConfiguration.Contains("\"FederatedInstanceArtifactId\":null"))
            {
                webModel.SourceConfiguration = JsonUtils.RemoveProperty(webModel.SourceConfiguration, "FederatedInstanceArtifactId");
            }
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
