using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Contracts;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Models;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Extensions;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.IntegrationPoints.Web.Attributes;
using Newtonsoft.Json;
using Relativity.API;
using Relativity.Toggles;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class JobController : ApiController
	{
		private const string _RELATIVITY_USERID = "rel_uai";
		private const string _RUN_AUDIT_MESSAGE = "Transfer was attempted.";
		private const string _RETRY_AUDIT_MESSAGE = "Retry error was attempted.";
		private const string _STOP_AUDIT_MESSAGE = "Stop transfer was attempted.";

		private readonly IServiceFactory _serviceFactory;
		private readonly ICPHelper _helper;
		private readonly IHelperFactory _helperFactory;
		private readonly ICaseServiceContext _context;
		private readonly IContextContainerFactory _contextContainerFactory;
		private readonly ISerializer _serializer;
		private readonly IChoiceQuery _choiceQuery;
		private readonly IJobManager _jobService;
		private readonly IManagerFactory _managerFactory;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IIntegrationPointProviderValidator _ipValidator;
		private readonly IIntegrationPointPermissionValidator _permissionValidator;
		private readonly IToggleProvider _toggleProvider;

		public JobController(
			IServiceFactory serviceFactory, 
			ICPHelper helper, 
			IHelperFactory helperFactory,
			ICaseServiceContext context,
			IContextContainerFactory contextContainerFactory,
			ISerializer serializer,
			IChoiceQuery choiceQuery,
			IJobManager jobService,
			IManagerFactory managerFactory,
			IRepositoryFactory repositoryFactory,
			IIntegrationPointProviderValidator ipValidator,
			IIntegrationPointPermissionValidator permissionValidator,
			IToggleProvider toggleProvider)
		{
			_serviceFactory = serviceFactory;
			_helper = helper;
			_helperFactory = helperFactory;
			_context = context;
			_contextContainerFactory = contextContainerFactory;
			_serializer = serializer;
			_choiceQuery = choiceQuery;
			_jobService = jobService;
			_managerFactory = managerFactory;
			_repositoryFactory = repositoryFactory;
			_ipValidator = ipValidator;
			_permissionValidator = permissionValidator;
			_toggleProvider = toggleProvider;
		}

		// POST API/Job/Run
		[HttpPost]
		[LogApiExceptionFilter(Message = "Unable to run the transfer job.")]
		public HttpResponseMessage Run(Payload payload)
		{
			AuditAction(payload, _RUN_AUDIT_MESSAGE);

			IIntegrationPointRepository integrationPointRepository = _repositoryFactory.GetIntegrationPointRepository(_helper.GetActiveCaseID());
			var integrationPoint = _context.RsapiService.IntegrationPointLibrary.Read(Convert.ToInt32(payload.ArtifactId));
			DestinationConfiguration importSettings = JsonConvert.DeserializeObject<DestinationConfiguration>(integrationPoint.DestinationConfiguration);
			IHelper targetHelper = _helperFactory.CreateTargetHelper(_helper, importSettings.FederatedInstanceArtifactId, integrationPoint.SecuredConfiguration);

			IIntegrationPointService integrationPointService = _serviceFactory.CreateIntegrationPointService(_helper, targetHelper,
				_context, _contextContainerFactory, _serializer, _choiceQuery, _jobService, _managerFactory, _ipValidator, _permissionValidator, _toggleProvider);

			HttpResponseMessage httpResponseMessage = RunInternal(payload.AppId, payload.ArtifactId, integrationPointService.RunIntegrationPoint);
			return httpResponseMessage;
		}

		// POST API/Job/Retry
		[HttpPost]
		[LogApiExceptionFilter(Message = "Unable to retry run of the transfer job.")]
		public HttpResponseMessage Retry(Payload payload)
		{
			AuditAction(payload, _RETRY_AUDIT_MESSAGE);

			IntegrationPoint integrationPoint = _context.RsapiService.IntegrationPointLibrary.Read(Convert.ToInt32(payload.ArtifactId));
			DestinationConfiguration importSettings = JsonConvert.DeserializeObject<DestinationConfiguration>(integrationPoint.DestinationConfiguration);
			IHelper targetHelper = _helperFactory.CreateTargetHelper(_helper, importSettings.FederatedInstanceArtifactId, integrationPoint.SecuredConfiguration);

			IIntegrationPointService integrationPointService = _serviceFactory.CreateIntegrationPointService(_helper, targetHelper,
				_context, _contextContainerFactory, _serializer, _choiceQuery, _jobService, _managerFactory, _ipValidator, _permissionValidator, _toggleProvider);

			HttpResponseMessage httpResponseMessage = RunInternal(payload.AppId, payload.ArtifactId, integrationPointService.RetryIntegrationPoint);
			return httpResponseMessage;
		}

		// POST API/Job/Stop
		[HttpPost]
		public HttpResponseMessage Stop(Payload payload)
		{
			AuditAction(payload, _STOP_AUDIT_MESSAGE);

			string errorMessage = String.Empty;
			HttpStatusCode httpStatusCode = HttpStatusCode.OK;

			IIntegrationPointService integrationPointService = _serviceFactory.CreateIntegrationPointService(_helper, _helper,
				_context, _contextContainerFactory, _serializer, _choiceQuery, _jobService, _managerFactory, _ipValidator, _permissionValidator, _toggleProvider);

			try
			{
				integrationPointService.MarkIntegrationPointToStopJobs(payload.AppId, payload.ArtifactId);
			}
			catch (AggregateException exception)
			{
				// TODO: Add an extension to aggregate messages without stack traces. Place it in ExceptionExtensions.cs
				IEnumerable<string> innerExceptions = exception.InnerExceptions.Where(ex => ex != null).Select(ex => ex.Message);
				errorMessage = $"{exception.Message} : {String.Join(",", innerExceptions)}";
				httpStatusCode = HttpStatusCode.BadRequest;
				CreateRelativityError(errorMessage, exception.FlattenErrorMessages(), payload.AppId);
			}
			catch (Exception exception)
			{
				errorMessage = exception.Message;
				httpStatusCode = HttpStatusCode.BadRequest;
				CreateRelativityError(errorMessage, exception.FlattenErrorMessages(), payload.AppId);
			}

			HttpResponseMessage response = Request.CreateResponse(httpStatusCode);
			if (!String.IsNullOrEmpty(errorMessage))
			{
				response.Content = new StringContent(errorMessage, System.Text.Encoding.UTF8, "text/plain");
			}

			return response;
		}

		private HttpResponseMessage RunInternal(int workspaceId, int relatedObjectArtifactId, Action<int, int, int> integrationPointServiceMethod)
		{
			string errorMessage = String.Empty;
			HttpStatusCode httpStatusCode = HttpStatusCode.OK;
			try
			{
				int userId = GetUserIdIfExists();
				integrationPointServiceMethod(workspaceId, relatedObjectArtifactId, userId);
			}
			catch (AggregateException exception)
			{
				IEnumerable<string> innerExceptions = exception.InnerExceptions.Where(ex => ex != null).Select(ex => ex.Message);
				errorMessage = $"{exception.Message} : {String.Join(",", innerExceptions)}";
				httpStatusCode = HttpStatusCode.BadRequest;
			}
			catch (Exception exception)
			{
				errorMessage = exception.Message;
				httpStatusCode = HttpStatusCode.BadRequest;
			}

			HttpResponseMessage response = Request.CreateResponse(httpStatusCode);
			response.Content = new StringContent(errorMessage, System.Text.Encoding.UTF8, "text/plain");

			return response;
		}

		private void AuditAction(Payload payload, string auditMessage)
		{
			IContextContainer contextContainer = _contextContainerFactory.CreateContextContainer(_helper);
			IAuditManager auditManager = _managerFactory.CreateAuditManager(contextContainer, payload.AppId);
			AuditElement audit = new AuditElement {AuditMessage = auditMessage};
			auditManager.RelativityAuditRepository.CreateAuditRecord(payload.ArtifactId, audit);
		}

		private int GetUserIdIfExists()
		{
			var user = User as ClaimsPrincipal;
			if (user != null)
			{
				foreach (Claim claim in user.Claims)
				{
					if (_RELATIVITY_USERID.Equals(claim.Type, StringComparison.OrdinalIgnoreCase))
					{
						return Convert.ToInt32(claim.Value);
					}
				}
			}
			return 0;
		}

		private void CreateRelativityError(string message, string fullText, int workspaceArtifactId)
		{
			IContextContainer contextContainer = _contextContainerFactory.CreateContextContainer(_helper);
			IErrorManager errorManager = _managerFactory.CreateErrorManager(contextContainer);

			ErrorDTO error = new ErrorDTO()
			{
				Message = message,
				FullText = fullText,
				Source =  Core.Constants.IntegrationPoints.APPLICATION_NAME,
				WorkspaceId = workspaceArtifactId
			};

			errorManager.Create(new[] { error });
		}

		public class Payload
		{
			public int AppId { get; set; }
			public int ArtifactId { get; set; }
		}
	}
}