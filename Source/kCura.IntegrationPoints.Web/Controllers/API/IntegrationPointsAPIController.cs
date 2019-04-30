using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.IntegrationPoints.Web.Helpers;
using kCura.IntegrationPoints.Web.Models.Validation;
using Newtonsoft.Json;
using Relativity.API;
using Relativity.Telemetry.Services.Interface;
using Relativity.Telemetry.Services.Metrics;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class IntegrationPointsAPIController : ApiController
	{
		private readonly IServiceFactory _serviceFactory;
		private readonly IRelativityUrlHelper _urlHelper;
		private readonly Core.Services.Synchronizer.IRdoSynchronizerProvider _provider;
		private readonly ICPHelper _cpHelper;
		private readonly IHelperFactory _helperFactory;

		public IntegrationPointsAPIController(
			IServiceFactory serviceFactory,
			IRelativityUrlHelper urlHelper,
			Core.Services.Synchronizer.IRdoSynchronizerProvider provider,
			ICPHelper cpHelper,
			IHelperFactory helperFactory)
		{
			_serviceFactory = serviceFactory;
			_urlHelper = urlHelper;
			_provider = provider;
			_cpHelper = cpHelper;
			_helperFactory = helperFactory;
		}

		[HttpGet]
		[LogApiExceptionFilter(Message = "Unable to retrive integration point data.")]
		public HttpResponseMessage Get(int id)
		{
			try
			{
				var model = new IntegrationPointModel();
				model.ArtifactID = id;
				if (id > 0)
				{
					IIntegrationPointService integrationPointService =  _serviceFactory.CreateIntegrationPointService(_cpHelper, _cpHelper);
					model = integrationPointService.ReadIntegrationPointModel(id);
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
		public HttpResponseMessage Update(int workspaceID, IntegrationPointModel model)
		{
			using (IAPMManager apmManger = _cpHelper.GetServicesManager().CreateProxy<IAPMManager>(ExecutionIdentity.CurrentUser))
			{
				using (IMetricsManager metricManager = _cpHelper.GetServicesManager().CreateProxy<IMetricsManager>(ExecutionIdentity.CurrentUser))
				{
					var apmMetricProperties = new APMMetric
					{
						Name =
							Core.Constants.IntegrationPoints.Telemetry
								.BUCKET_INTEGRATION_POINT_REC_SAVE_DURATION_METRIC_COLLECTOR,
						CustomData = new Dictionary<string, object> { { Core.Constants.IntegrationPoints.Telemetry.CUSTOM_DATA_CORRELATIONID, model.Name } }
					};
					using (apmManger.LogTimedOperation(apmMetricProperties))
					{
						using (metricManager.LogDuration(Core.Constants.IntegrationPoints.Telemetry.BUCKET_INTEGRATION_POINT_REC_SAVE_DURATION_METRIC_COLLECTOR,
							Guid.Empty, model.Name))
						{
							ImportSettings importSettings = JsonConvert.DeserializeObject<ImportSettings>(model.Destination);
							IHelper targetHelper = _helperFactory.CreateTargetHelper(_cpHelper, importSettings.FederatedInstanceArtifactId, model.SecuredConfiguration);

							IIntegrationPointService integrationPointService = _serviceFactory.CreateIntegrationPointService(_cpHelper, targetHelper);

							int createdId;
							try
							{
								createdId = integrationPointService.SaveIntegration(model);
							}
							catch (IntegrationPointValidationException ex)
							{
								var validationResultMapper = new ValidationResultMapper();
								ValidationResultDTO validationResultDto = validationResultMapper.Map(ex.ValidationResult);
								return Request.CreateResponse(HttpStatusCode.NotAcceptable, validationResultDto);
							}

							string result = _urlHelper.GetRelativityViewUrl(workspaceID, createdId, Data.ObjectTypes.IntegrationPoint);

							return Request.CreateResponse(HttpStatusCode.OK, new { returnURL = result });
						}
					}
				}
			}
		}
	}
}