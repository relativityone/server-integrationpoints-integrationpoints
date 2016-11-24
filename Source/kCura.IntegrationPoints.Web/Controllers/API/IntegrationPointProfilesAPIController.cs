using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data;
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

		public IntegrationPointProfilesAPIController(ICPHelper cpHelper, IIntegrationPointProfileService profileService, IIntegrationPointService integrationPointService,
			IRelativityUrlHelper urlHelper)
		{
			_cpHelper = cpHelper;
			_profileService = profileService;
			_integrationPointService = integrationPointService;
			_urlHelper = urlHelper;
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