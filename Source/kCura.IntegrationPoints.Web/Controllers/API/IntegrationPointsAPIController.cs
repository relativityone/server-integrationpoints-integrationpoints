using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.IntegrationPoints.Web.Attributes;
using Newtonsoft.Json;
using Relativity.API;
using Relativity.Services.DataContracts.DTOs.MetricsCollection;
using Relativity.Telemetry.Services.Metrics;
using Relativity.Toggles;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class IntegrationPointsAPIController : ApiController
	{
		private readonly IServiceFactory _serviceFactory;
		private readonly IRelativityUrlHelper _urlHelper;
		private readonly Core.Services.Synchronizer.IRdoSynchronizerProvider _provider;
		private readonly ICPHelper _cpHelper;
		private readonly ICaseServiceContext _context;
		private readonly IContextContainerFactory _contextContainerFactory;
		private readonly ISerializer _serializer;
		private readonly IChoiceQuery _choiceQuery;
		private readonly IJobManager _jobManager;
		private readonly IJobHistoryService _jobHistoryService;
		private readonly IManagerFactory _managerFactory;
		private readonly IHelperFactory _helperFactory;
		private readonly IIntegrationPointProviderValidator _ipValidator;
		private readonly IIntegrationPointPermissionValidator _permissionValidator;
		private readonly IToggleProvider _toggleProvider;

		public IntegrationPointsAPIController(
			IServiceFactory serviceFactory,
			IRelativityUrlHelper urlHelper,
			Core.Services.Synchronizer.IRdoSynchronizerProvider provider,
			ICPHelper cpHelper,
			ICaseServiceContext context,
			IContextContainerFactory contextContainerFactory,
			ISerializer serializer, 
			IChoiceQuery choiceQuery,
			IJobManager jobManager,
			IJobHistoryService jobHistoryService,
			IManagerFactory managerFactory,
			IHelperFactory helperFactory,
			IIntegrationPointProviderValidator ipValidator,
			IIntegrationPointPermissionValidator permissionValidator,
			IToggleProvider toggleProvider)
		{
			_serviceFactory = serviceFactory;
			_urlHelper = urlHelper;
			_provider = provider;
			_cpHelper = cpHelper;
			_context = context;
			_contextContainerFactory = contextContainerFactory;
			_serializer = serializer;
			_choiceQuery = choiceQuery;
			_jobManager = jobManager;
			_jobHistoryService = jobHistoryService;
			_managerFactory = managerFactory;
			_helperFactory = helperFactory;
			_ipValidator = ipValidator;
			_permissionValidator = permissionValidator;
			_toggleProvider = toggleProvider;
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
					IIntegrationPointService integrationPointService =  _serviceFactory.CreateIntegrationPointService(_cpHelper, _cpHelper, 
						_context ,_contextContainerFactory, _serializer, _choiceQuery, _jobManager, _managerFactory, _ipValidator, _permissionValidator, _toggleProvider);
					model = integrationPointService.ReadIntegrationPoint(id);
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
			using (IMetricsManager metricManager = _cpHelper.GetServicesManager().CreateProxy<IMetricsManager>(ExecutionIdentity.CurrentUser))
			{
				using (metricManager.LogDuration(Core.Constants.IntegrationPoints.Telemetry.BUCKET_INTEGRATION_POINT_REC_SAVE_DURATION_METRIC_COLLECTOR,
					Guid.Empty, model.Name, MetricTargets.APMandSUM))
				{
						ImportSettings importSettings = JsonConvert.DeserializeObject<ImportSettings>(model.Destination);
						IHelper targetHelper;
						if (importSettings.FederatedInstanceArtifactId != null)
						{
							targetHelper = _helperFactory.CreateOAuthClientHelper(_cpHelper, importSettings.FederatedInstanceArtifactId.Value);
						}
						else
						{
							targetHelper = _cpHelper;
						}

						IIntegrationPointService integrationPointService = _serviceFactory.CreateIntegrationPointService(_cpHelper, targetHelper, 
							_context, _contextContainerFactory, _serializer, _choiceQuery, _jobManager, _managerFactory, _ipValidator, _permissionValidator, _toggleProvider);
						
						int createdId;
						try
						{
							createdId = integrationPointService.SaveIntegration(model);
						}
						catch (IntegrationPointProviderValidationException ex)
						{
							return Request.CreateResponse(HttpStatusCode.NotAcceptable, String.Join("<br />", ex.Result.Messages));
						}

						string result = _urlHelper.GetRelativityViewUrl(workspaceID, createdId, Data.ObjectTypes.IntegrationPoint);

						return Request.CreateResponse(HttpStatusCode.OK, new { returnURL = result });
				}
			}
		}
	}
}