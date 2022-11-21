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
            var models = _profileService.ReadAll();
            return Request.CreateResponse(HttpStatusCode.OK, models);
        }

        [HttpGet]
        [LogApiExceptionFilter(Message = "Unable to retrieve integration point profiles.")]
        public HttpResponseMessage GetByType(int artifactId)
        {
            var models = _profileService.ReadAll();
            var profileModelsByType = models.Where(_ => _.Type == artifactId);
            return Request.CreateResponse(HttpStatusCode.OK, profileModelsByType);
        }

        [HttpGet]
        [LogApiExceptionFilter(Message = "Unable to retrieve integration point profile.")]
        public HttpResponseMessage Get(int artifactId)
        {
            IntegrationPointProfileDto dto;
            if (artifactId > 0)
            {
                dto = _profileService.Read(artifactId);
            }
            else
            {
                dto = new IntegrationPointProfileDto();
            }

            return Request.CreateResponse(HttpStatusCode.OK, dto);
        }

        [LogApiExceptionFilter(Message = "Unable to validate integration point profile.")]
        public HttpResponseMessage GetValidatedProfileModel(int artifactId)
        {
            if (artifactId > 0)
            {
                IntegrationPointProfileDto dto = _profileService.Read(artifactId);
                ValidationResultDTO validationResult = ValidateIntegrationPointProfile(dto);

                var output = new ValidatedProfileDTO(dto, validationResult);
                return Request.CreateResponse(HttpStatusCode.OK, output);
            }

            return Request.CreateResponse(HttpStatusCode.NotAcceptable);
        }

        [HttpPost]
        [LogApiExceptionFilter(Message = "Unable to save or update integration point profile.")]
        public HttpResponseMessage Save(int workspaceID, IntegrationPointProfileDto dto)
        {
            using (IAPMManager apmManger = _cpHelper.GetServicesManager().CreateProxy<IAPMManager>(ExecutionIdentity.CurrentUser))
            {
                using (IMetricsManager metricManager = _cpHelper.GetServicesManager().CreateProxy<IMetricsManager>(ExecutionIdentity.CurrentUser))
                {
                    string nameHash = _cryptographyHelper.CalculateHash(dto.Name);
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
                            return SaveIntegrationPointProfile(workspaceID, dto);
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
                                IntegrationPointDto integrationPointDto = _integrationPointService.Read(model.IntegrationPointArtifactId);
                                IntegrationPointProfileDto profileDto = integrationPointDto.ToProfileDto(model.ProfileName);

                                response = SaveIntegrationPointProfile(workspaceID, profileDto);

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

        private HttpResponseMessage SaveIntegrationPointProfile(int workspaceID, IntegrationPointProfileDto dto)
        {
            int createdID;
            try
            {
                createdID = _profileService.SaveProfile(dto);
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

        private ValidationResultDTO ValidateIntegrationPointProfile(IntegrationPointProfileDto dto)
        {
            SourceProvider sourceProvider = _objectManager.Read<SourceProvider>(dto.SourceProvider);
            DestinationProvider destinationProvider = _objectManager.Read<DestinationProvider>(dto.DestinationProvider);
            IntegrationPointType integrationPointType = _objectManager.Read<IntegrationPointType>(dto.Type);

            ValidationContext validationContext = new ValidationContext
            {
                DestinationProvider = destinationProvider,
                IntegrationPointType = integrationPointType,
                Model = dto,
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
