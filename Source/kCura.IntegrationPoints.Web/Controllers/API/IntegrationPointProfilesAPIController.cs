using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Web.Attributes;
using Relativity.API;
using Relativity.Services.DataContracts.DTOs.MetricsCollection;
using Relativity.Telemetry.Services.Metrics;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class IntegrationPointProfilesAPIController : ApiController
	{
		private readonly ICPHelper _cpHelper;
		private readonly IIntegrationPointService _integrationPointService;
		private readonly IIntegrationPointProfileService _profileService;
		private readonly IRelativityUrlHelper _urlHelper;
		private readonly ICaseServiceContext _context;
		private readonly IIntegrationPointProviderValidator _integrationModelValidator;

		public IntegrationPointProfilesAPIController(ICPHelper cpHelper, IIntegrationPointProfileService profileService, IIntegrationPointService integrationPointService,
			IRelativityUrlHelper urlHelper, ICaseServiceContext context, IIntegrationPointProviderValidator integrationModelValidator)
		{
			_cpHelper = cpHelper;
			_profileService = profileService;
			_integrationPointService = integrationPointService;
			_urlHelper = urlHelper;
			_context = context;
			_integrationModelValidator = integrationModelValidator;
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
			var models = _profileService.ReadIntegrationPointProfiles();
			var profileModelsByType = models.Where(_=>_.Type==artifactId);
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
				
				SourceProvider sourceProvider = _context.RsapiService.SourceProviderLibrary.Read(model.SourceProvider);
				DestinationProvider destinationProvider = _context.RsapiService.DestinationProviderLibrary.Read(model.DestinationProvider);

				ValidationResult validationResult = _integrationModelValidator.Validate(model, sourceProvider, destinationProvider);

				if (validationResult.IsValid)
				{
					return Request.CreateResponse(HttpStatusCode.OK, model);
				}
				return Request.CreateResponse(HttpStatusCode.NotAcceptable,model);
			}

			return Request.CreateResponse(HttpStatusCode.NotAcceptable);
		}

		[HttpPost]
		[LogApiExceptionFilter(Message = "Unable to save or update integration point profile.")]
		public HttpResponseMessage Save(int workspaceID, IntegrationPointProfileModel model)
		{
			using (IMetricsManager metricManager = _cpHelper.GetServicesManager().CreateProxy<IMetricsManager>(ExecutionIdentity.CurrentUser))
			{
				using (metricManager.LogDuration(Core.Constants.IntegrationPoints.Telemetry.BUCKET_INTEGRATION_POINT_PROFILE_SAVE_DURATION_METRIC_COLLECTOR,
					Guid.Empty, model.Name, MetricTargets.APMandSUM))
				{
					int createdId = _profileService.SaveIntegration(model);
					string result = _urlHelper.GetRelativityViewUrl(workspaceID, createdId, ObjectTypes.IntegrationPointProfile);

					return Request.CreateResponse(HttpStatusCode.OK, new {returnURL = result});
				}
			}
		}

		[HttpPost]
		[LogApiExceptionFilter(Message = "Unable to save integration point profile.")]
		public HttpResponseMessage SaveUsingIntegrationPoint(int workspaceID, int integrationPointArtifactId, string profileName)
		{
			using (IMetricsManager metricManager = _cpHelper.GetServicesManager().CreateProxy<IMetricsManager>(ExecutionIdentity.CurrentUser))
			{
				using (metricManager.LogDuration(Core.Constants.IntegrationPoints.Telemetry.BUCKET_INTEGRATION_POINT_PROFILE_SAVE_AS_PROFILE_DURATION_METRIC_COLLECTOR,
					Guid.Empty, profileName, MetricTargets.APMandSUM))
				{
					var ip = _integrationPointService.GetRdo(integrationPointArtifactId);
					var model = IntegrationPointProfileModel.FromIntegrationPoint(ip, profileName);

					int createdId = _profileService.SaveIntegration(model);
					string result = _urlHelper.GetRelativityViewUrl(workspaceID, createdId, ObjectTypes.IntegrationPointProfile);

					return Request.CreateResponse(HttpStatusCode.OK, new {returnURL = result});
				}
			}
		}
	}
}