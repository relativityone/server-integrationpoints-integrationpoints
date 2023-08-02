using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.IntegrationPoints.Web.Extensions;
using kCura.IntegrationPoints.Web.Helpers;
using kCura.IntegrationPoints.Web.Models;
using kCura.IntegrationPoints.Web.Models.Validation;
using Relativity.API;
using Relativity.IntegrationPoints.Services;
using Relativity.Logging;
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
        private readonly ICamelCaseSerializer _serializer;

        public IntegrationPointProfilesAPIController(
            ICPHelper cpHelper,
            IIntegrationPointProfileService profileService,
            IIntegrationPointService integrationPointService,
            IRelativityUrlHelper urlHelper,
            IRelativityObjectManager objectManager,
            IValidationExecutor validationExecutor,
            ICryptographyHelper cryptographyHelper,
            IAPILog logger,
            ICamelCaseSerializer serializer)
        {
            _cpHelper = cpHelper;
            _profileService = profileService;
            _integrationPointService = integrationPointService;
            _urlHelper = urlHelper;
            _objectManager = objectManager;
            _validationExecutor = validationExecutor;
            _cryptographyHelper = cryptographyHelper;
            _logger = logger;
            _serializer = serializer;
        }

        [HttpGet]
        [LogApiExceptionFilter(Message = "Unable to retrieve integration point profiles.")]
        public HttpResponseMessage GetAll()
        {
            var models = _profileService
                .ReadAllSlim()
                .Select(dto => dto.ToWebModel())
                .ToList();
            return Request.CreateResponse(HttpStatusCode.OK, models);
        }

        [HttpGet]
        [LogApiExceptionFilter(Message = "Unable to retrieve integration point profiles.")]
        public HttpResponseMessage GetByType(int artifactId)
        {
            var models = _profileService
                .ReadAllSlim()
                .Select(dto => dto.ToWebModel())
                .ToList();
            var profileModelsByType = models.Where(_ => _.Type == artifactId);
            return Request.CreateResponse(HttpStatusCode.OK, profileModelsByType);
        }

        [HttpGet]
        [LogApiExceptionFilter(Message = "Unable to retrieve integration point profile.")]
        public HttpResponseMessage Get(int artifactId)
        {
            IntegrationPointProfileWebModel webModel = artifactId > 0
                ? _profileService.Read(artifactId).ToWebModel(_serializer)
                : new IntegrationPointProfileWebModel
                    {
                        // we need this hack because frontend logic rely on this:
                        // export-provider-fields-step.js: [if (typeof ip.sourceConfiguration === "string")]
                        SourceConfiguration = string.Empty,
                        LogErrors = true,
                    };

            return Request.CreateResponse(HttpStatusCode.OK, webModel);
        }

        [LogApiExceptionFilter(Message = "Unable to validate integration point profile.")]
        public HttpResponseMessage GetValidatedProfileModel(int artifactId)
        {
            if (artifactId > 0)
            {
                IntegrationPointProfileDto dto = _profileService.Read(artifactId);
                ValidationResultDTO validationResult = ValidateIntegrationPointProfile(dto);

                var output = new ValidatedProfileDTO(dto.ToWebModel(_serializer), validationResult);
                return Request.CreateResponse(HttpStatusCode.OK, output);
            }

            return Request.CreateResponse(HttpStatusCode.NotAcceptable);
        }

        [HttpPost]
        [LogApiExceptionFilter(Message = "Unable to save or update integration point profile.")]
        public HttpResponseMessage Save(int workspaceID, IntegrationPointProfileWebModel webModel)
        {
            using (IAPMManager apmManger = _cpHelper.GetServicesManager().CreateProxy<IAPMManager>(ExecutionIdentity.CurrentUser))
            {
                using (IMetricsManager metricManager = _cpHelper.GetServicesManager().CreateProxy<IMetricsManager>(ExecutionIdentity.CurrentUser))
                {
                    string nameHash = _cryptographyHelper.CalculateHash(webModel.Name);
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
                            return SaveIntegrationPointProfile(workspaceID, webModel.ToDto(_serializer));
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
                if (response != null)
                {
                    _logger.LogWarning(ex, "Error occurred in SaveUsingIntegrationPoint request, but the profile was saved.");
                    return response;
                }

                throw;
            }
        }

        [HttpPost]
        [LogApiExceptionFilter(Message = "Unable to create Integration Point Profile from profile")]
        public HttpResponseMessage CreateProfileFromProfile(int artifactId, string name)
        {
            using (var _integrationPointProfileManager = _cpHelper.GetServicesManager()
                       .CreateProxy<IIntegrationPointProfileManager>(ExecutionIdentity.System))
            {

                IntegrationPointModel model = _integrationPointProfileManager.GetIntegrationPointProfileAsync(
                        _cpHelper.GetActiveCaseID(),
                        artifactId)
                    .GetAwaiter()
                    .GetResult();
                model.Name = name;
                CreateIntegrationPointRequest request = new CreateIntegrationPointRequest
                {
                    IntegrationPoint = model,
                    WorkspaceArtifactId = _cpHelper.GetActiveCaseID()
                };

                var result = _integrationPointProfileManager.CreateIntegrationPointProfileAsync(request)
                    .GetAwaiter()
                    .GetResult();
                return Request.CreateResponse(HttpStatusCode.OK);
            }
        }

        private HttpResponseMessage SaveIntegrationPointProfile(int workspaceID, IntegrationPointProfileDto profileDto)
        {
            int createdID;
            try
            {
                createdID = _profileService.SaveProfile(profileDto);
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
