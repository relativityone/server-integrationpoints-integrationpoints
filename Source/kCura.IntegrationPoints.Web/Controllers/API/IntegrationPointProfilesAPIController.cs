using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Castle.Core.Internal;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.IntegrationPoints.Web.Helpers;
using kCura.IntegrationPoints.Web.Models;
using kCura.IntegrationPoints.Web.Models.Validation;
using Relativity.API;
using Relativity.Telemetry.Services.Interface;
using Relativity.Telemetry.Services.Metrics;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
    public class IntegrationPointProfilesAPIController : ApiController
    {
        private readonly ICPHelper _cpHelper;
        private readonly IIntegrationPointService _integrationPointService;
        private readonly IIntegrationPointProfileService _profileService;
        private readonly IRelativityUrlHelper _urlHelper;
        private readonly IRelativityObjectManager _objectManager;
        private readonly IValidationExecutor _validationExecutor;
        private readonly ICryptographyHelper _cryptographyHelper;
        private readonly IAPILog _logger;

        public IntegrationPointProfilesAPIController(ICPHelper cpHelper, IIntegrationPointProfileService profileService, IIntegrationPointService integrationPointService,
            IRelativityUrlHelper urlHelper, IRelativityObjectManager objectManager, IValidationExecutor validationExecutor, ICryptographyHelper cryptographyHelper, IAPILog logger)
        {
            _cpHelper = cpHelper;
            _profileService = profileService;
            _integrationPointService = integrationPointService;
            _urlHelper = urlHelper;
            _objectManager = objectManager;
            _validationExecutor = validationExecutor;
            _cryptographyHelper = cryptographyHelper;
            _logger = logger;
        }

        [HttpGet]
        [LogApiExceptionFilter(Message = "Unable to retrieve integration point profiles.")]
        public HttpResponseMessage GetAll()
        {
            var models = _profileService.ReadIntegrationPointProfiles();
            return Request.CreateResponse(HttpStatusCode.OK, models);
        }

        [HttpGet]
        [LogApiExceptionFilter(Message = "Unable to retrieve integration point profiles.")]
        public HttpResponseMessage GetByType(int artifactId)
        {
            var models = _profileService.ReadIntegrationPointProfilesSimpleModel();
            var profileModelsByType = models.Where(_ => _.Type == artifactId);
            return Request.CreateResponse(HttpStatusCode.OK, profileModelsByType);
        }

        [HttpGet]
        [LogApiExceptionFilter(Message = "Unable to retrieve integration point profile.")]
        public HttpResponseMessage Get(int artifactId)
        {
            IntegrationPointProfileModel model;
            if (artifactId > 0)
            {
                model = _profileService.ReadIntegrationPointProfileModel(artifactId);
            }
            else
            {
                model = new IntegrationPointProfileModel();
            }

            return Request.CreateResponse(HttpStatusCode.OK, model);
        }

        [LogApiExceptionFilter(Message = "Unable to validate integration point profile.")]
        public HttpResponseMessage GetValidatedProfileModel(int artifactId)
        {
            if (artifactId > 0)
            {
                IntegrationPointProfileModel model = _profileService.ReadIntegrationPointProfileModel(artifactId);
                ValidationResultDTO validationResult = ValidateIntegrationPointProfile(model);
                
                var output = new ValidatedProfileDTO(model, validationResult);
                return Request.CreateResponse(HttpStatusCode.OK, output);
            }

            return Request.CreateResponse(HttpStatusCode.NotAcceptable);
        }
        
        [HttpPost]
        [LogApiExceptionFilter(Message = "Unable to save or update integration point profile.")]
        public HttpResponseMessage Save(int workspaceID, IntegrationPointProfileModel model)
        {
            using (IAPMManager apmManger = _cpHelper.GetServicesManager().CreateProxy<IAPMManager>(ExecutionIdentity.CurrentUser))
            {
                using (IMetricsManager metricManager = _cpHelper.GetServicesManager().CreateProxy<IMetricsManager>(ExecutionIdentity.CurrentUser))
                {
                    string nameHash = _cryptographyHelper.CalculateHash(model.Name);
                    var apmMetricProperties = new APMMetric
                    {
                        Name =
                            Core.Constants.IntegrationPoints.Telemetry
                                .BUCKET_INTEGRATION_POINT_PROFILE_SAVE_DURATION_METRIC_COLLECTOR,
                        CustomData = new Dictionary<string, object> { { Core.Constants.IntegrationPoints.Telemetry.CUSTOM_DATA_CORRELATIONID, nameHash } }
                    };
                    using (apmManger.LogTimedOperation(apmMetricProperties))
                    {
                        using (metricManager.LogDuration(Core.Constants.IntegrationPoints.Telemetry.BUCKET_INTEGRATION_POINT_PROFILE_SAVE_DURATION_METRIC_COLLECTOR,
                            Guid.Empty, nameHash))
                        {
                            return SaveIntegrationPointProfile(workspaceID, model);
                        }
                    }
                }
            }
        }

        [HttpPost]
        [LogApiExceptionFilter(Message = "Unable to save integration point profile.")]
        public HttpResponseMessage SaveUsingIntegrationPoint(int workspaceID, IntegrationPointProfileFromIntegrationPointModel model)
        {
            HttpResponseMessage response = null;
            try
            {
                using (IAPMManager apmManger = _cpHelper.GetServicesManager().CreateProxy<IAPMManager>(ExecutionIdentity.CurrentUser))
                {
                    using (IMetricsManager metricManager = _cpHelper.GetServicesManager().CreateProxy<IMetricsManager>(ExecutionIdentity.CurrentUser))
                    {
                        string profileNameHash = _cryptographyHelper.CalculateHash(model.ProfileName);
                        var apmMetricProperties = new APMMetric
                        {
                            Name =
                                Core.Constants.IntegrationPoints.Telemetry
                                    .BUCKET_INTEGRATION_POINT_PROFILE_SAVE_AS_PROFILE_DURATION_METRIC_COLLECTOR,
                            CustomData = new Dictionary<string, object> { { Core.Constants.IntegrationPoints.Telemetry.CUSTOM_DATA_CORRELATIONID, profileNameHash } }
                        };
                        using (apmManger.LogTimedOperation(apmMetricProperties))
                        {
                            using (metricManager.LogDuration(Core.Constants.IntegrationPoints.Telemetry.BUCKET_INTEGRATION_POINT_PROFILE_SAVE_AS_PROFILE_DURATION_METRIC_COLLECTOR,
                                Guid.Empty, profileNameHash))
                            {
                                IntegrationPoint integrationPoint = _integrationPointService.ReadIntegrationPoint(model.IntegrationPointArtifactId);
                                IntegrationPointProfileModel profileModel = IntegrationPointProfileModel.FromIntegrationPoint(integrationPoint, model.ProfileName);

                                response = SaveIntegrationPointProfile(workspaceID, profileModel);

                                return response;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if(response != null)
                {
                    _logger.LogWarning(ex, "Error occurred in SaveUsingIntegrationPoint request, but the profile was saved.");
                    return response;
                }

                throw;
            }
        }

        private HttpResponseMessage SaveIntegrationPointProfile(int workspaceID, IntegrationPointProfileModel model)
        {
            int createdID;
            try
            {
                createdID = _profileService.SaveIntegration(model);
            }
            catch (IntegrationPointValidationException ex)
            {
                var validationResultMapper = new ValidationResultMapper();
                ValidationResultDTO validationResultDto = validationResultMapper.Map(ex.ValidationResult);
                return Request.CreateResponse(HttpStatusCode.NotAcceptable, validationResultDto);
            }

            string result = _urlHelper.GetRelativityViewUrl(workspaceID, createdID, ObjectTypes.IntegrationPointProfile);

            return Request.CreateResponse(HttpStatusCode.OK, new { returnURL = result });
        }

        private ValidationResultDTO ValidateIntegrationPointProfile(IntegrationPointProfileModel model)
        {
            SourceProvider sourceProvider = _objectManager.Read<SourceProvider>(model.SourceProvider);
            DestinationProvider destinationProvider = _objectManager.Read<DestinationProvider>(model.DestinationProvider);
            IntegrationPointType integrationPointType = _objectManager.Read<IntegrationPointType>(model.Type);

            ValidationContext validationContext = new ValidationContext
            {
                DestinationProvider = destinationProvider,
                IntegrationPointType = integrationPointType,
                Model = model,
                SourceProvider = sourceProvider,
                ObjectTypeGuid = ObjectTypeGuids.IntegrationPointProfileGuid
            };

            ValidationResult validationResult = _validationExecutor.ValidateOnProfile(validationContext);
            validationResult.AppendTextToShortMessage(IntegrationPointProviderValidationMessages.ARTIFACT_NOT_EXIST, IntegrationPointProviderValidationMessages.NEXT_BUTTON_INSTRUCTION);

            return MapToValidationResultOutputModel(validationResult);
        }

        private ValidationResultDTO MapToValidationResultOutputModel(ValidationResult validationResult)
        {
            var mapper = new ValidationResultMapper();
            return mapper.Map(validationResult);
        }
    }
}