using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Core.Validation.Abstract;
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

		public IntegrationPointProfilesAPIController(ICPHelper cpHelper, IIntegrationPointProfileService profileService, IIntegrationPointService integrationPointService,
			IRelativityUrlHelper urlHelper, IRelativityObjectManager objectManager, IValidationExecutor validationExecutor)
		{
			_cpHelper = cpHelper;
			_profileService = profileService;
			_integrationPointService = integrationPointService;
			_urlHelper = urlHelper;
			_objectManager = objectManager;
			_validationExecutor = validationExecutor;
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
				model = _profileService.ReadIntegrationPointProfile(artifactId);
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
				IntegrationPointProfileModel model = _profileService.ReadIntegrationPointProfile(artifactId);
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
					var apmMetricProperties = new APMMetric
					{
						Name =
							Core.Constants.IntegrationPoints.Telemetry
								.BUCKET_INTEGRATION_POINT_PROFILE_SAVE_DURATION_METRIC_COLLECTOR,
						CustomData = new Dictionary<string, object> { { Core.Constants.IntegrationPoints.Telemetry.CUSTOM_DATA_CORRELATIONID, model.Name } }
					};
					using (apmManger.LogTimedOperation(apmMetricProperties))
					{
						using (metricManager.LogDuration(Core.Constants.IntegrationPoints.Telemetry.BUCKET_INTEGRATION_POINT_PROFILE_SAVE_DURATION_METRIC_COLLECTOR,
							Guid.Empty, model.Name))
						{
							int createdId = _profileService.SaveIntegration(model);
							string result = _urlHelper.GetRelativityViewUrl(workspaceID, createdId, ObjectTypes.IntegrationPointProfile);

							return Request.CreateResponse(HttpStatusCode.OK, new { returnURL = result });
						}
					}
				}
			}
		}

		[HttpPost]
		[LogApiExceptionFilter(Message = "Unable to save integration point profile.")]
		public HttpResponseMessage SaveUsingIntegrationPoint(int workspaceID, int integrationPointArtifactId, string profileName)
		{
			using (IAPMManager apmManger = _cpHelper.GetServicesManager().CreateProxy<IAPMManager>(ExecutionIdentity.CurrentUser))
			{
				using (IMetricsManager metricManager = _cpHelper.GetServicesManager().CreateProxy<IMetricsManager>(ExecutionIdentity.CurrentUser))
				{
					var apmMetricProperties = new APMMetric
					{
						Name =
							Core.Constants.IntegrationPoints.Telemetry
								.BUCKET_INTEGRATION_POINT_PROFILE_SAVE_AS_PROFILE_DURATION_METRIC_COLLECTOR,
						CustomData = new Dictionary<string, object> { { Core.Constants.IntegrationPoints.Telemetry.CUSTOM_DATA_CORRELATIONID, profileName } }
					};
					using (apmManger.LogTimedOperation(apmMetricProperties))
					{
						using (metricManager.LogDuration(Core.Constants.IntegrationPoints.Telemetry.BUCKET_INTEGRATION_POINT_PROFILE_SAVE_AS_PROFILE_DURATION_METRIC_COLLECTOR,
							Guid.Empty, profileName))
						{
							var ip = _integrationPointService.ReadIntegrationPoint(integrationPointArtifactId);
							var model = IntegrationPointProfileModel.FromIntegrationPoint(ip, profileName);

							int createdId = _profileService.SaveIntegration(model);
							string result = _urlHelper.GetRelativityViewUrl(workspaceID, createdId, ObjectTypes.IntegrationPointProfile);

							return Request.CreateResponse(HttpStatusCode.OK, new { returnURL = result });
						}
					}
				}
			}
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
				ObjectTypeGuid = ObjectTypeGuids.IntegrationPointProfile
			};

			ValidationResult validationResult = _validationExecutor.ValidateOnProfile(validationContext);

			return MapToValidationResultOutputModel(validationResult);
		}

		private ValidationResultDTO MapToValidationResultOutputModel(ValidationResult validationResult)
		{
			var mapper = new ValidationResultMapper();
			return mapper.Map(validationResult);
		}
	}
}