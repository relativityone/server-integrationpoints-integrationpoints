using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using Newtonsoft.Json;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class ImportNowController : ApiController
	{
		private const string RELATIVITY_USERID = "rel_uai";
		internal const string NO_PERMISSION_TO_IMPORT = "You do not have permission to push documents to the destination workspace selected. Please contact your system administrator.";
		internal const string NO_USERID = "Unable to determine the user id. Please contact your system administrator.";

		private readonly IJobManager _jobManager;
		private readonly IPermissionService _permissionService;
		private readonly IIntegrationPointRdoAdaptor _rdoDependenciesAdaptor;

		public ImportNowController(IJobManager jobManager,
			ICaseServiceContext caseServiceContext,
			IIntegrationPointService integrationPointService,
			JobHistoryService jobHistoryService,
			IPermissionService permissionService)
			: this(jobManager, permissionService,
				new IntegrationPointRdoInitializer(integrationPointService, caseServiceContext, jobHistoryService))
		{
		}

		internal ImportNowController(IJobManager jobManager,
			IPermissionService permissionService,
			IIntegrationPointRdoAdaptor rdoAdaptor)
		{
			_jobManager = jobManager;
			_permissionService = permissionService;
			_rdoDependenciesAdaptor = rdoAdaptor;
		}

		// POST api/importnow
		public HttpResponseMessage Post(Payload payload)
		{
			try
			{
				int workspaceID = payload.AppId;
				int relatedObjectArtifactID = payload.ArtifactId;
				Guid batchInstance = Guid.NewGuid();
				var jobDetails = new TaskParameters()
				{
					BatchInstance = batchInstance
				};
				
				_rdoDependenciesAdaptor.Initialize(relatedObjectArtifactID, batchInstance);

				int userId = GetUserIdIfExist();
				// if relativity provider is selected, we will create an export task
				if (_rdoDependenciesAdaptor.SourceProviderIdentifier.Equals(DocumentTransferProvider.Shared.Constants.RELATIVITY_PROVIDER_GUID))
				{
					DestinationWorkspace destinationWorkspace = JsonConvert.DeserializeObject<DestinationWorkspace>(_rdoDependenciesAdaptor.SourceConfiguration);
					if(_permissionService.UserCanImport(destinationWorkspace.TargetWorkspaceArtifactId) == false)
					{
						throw new Exception(NO_PERMISSION_TO_IMPORT);
					}

					if (userId == 0)
					{
						throw new Exception(NO_USERID);
					}

					_rdoDependenciesAdaptor.CreateJobHistoryRdo();
					_jobManager.CreateJobOnBehalfOfAUser(jobDetails,  TaskType.ExportService, workspaceID, relatedObjectArtifactID, userId);
				}
				else
				{
					_rdoDependenciesAdaptor.CreateJobHistoryRdo();
					_jobManager.CreateJobOnBehalfOfAUser(jobDetails, TaskType.SyncManager, workspaceID, relatedObjectArtifactID, userId);
				}
			}
			catch (AggregateException exception)
			{
				IEnumerable<string> innerExceptions = exception.InnerExceptions.Where(ex => ex != null).Select(ex => ex.Message);
				return Request.CreateResponse(HttpStatusCode.BadRequest, String.Format("{0} : {1}" , exception.Message, String.Join(",", innerExceptions)));
			}
			catch (Exception exception)
			{
				return Request.CreateResponse(HttpStatusCode.BadRequest, exception.Message);
			}
			return Request.CreateResponse(HttpStatusCode.OK);
		}


		private int GetUserIdIfExist()
		{
			var user = this.User as ClaimsPrincipal;
			if (user != null)
			{
				foreach (Claim claim in user.Claims)
				{
					if (RELATIVITY_USERID.Equals(claim.Type, StringComparison.OrdinalIgnoreCase))
					{
						return Convert.ToInt32(claim.Value);
					}
				}
			}
			return 0;
		}

		internal class IntegrationPointRdoInitializer : IIntegrationPointRdoAdaptor
		{
			private readonly IIntegrationPointService _integrationPointService;
			private readonly ICaseServiceContext _caseServiceContext;
			private readonly JobHistoryService _jobHistoryService;
			private IntegrationPoint _integrationPoint;
			private string _identifier;
			private string _sourceConfig;
			private Guid _batchInstance;

			public IntegrationPointRdoInitializer(IIntegrationPointService integrationPointService,
					ICaseServiceContext caseServiceContext,
					JobHistoryService jobHistoryService)
			{
				_integrationPointService = integrationPointService;
				_caseServiceContext = caseServiceContext;
				_jobHistoryService = jobHistoryService;
			}

			public void Initialize(int relatedObjectArtifactId, Guid batchInstance)
			{
				_batchInstance = batchInstance;
				_integrationPoint = _integrationPointService.GetRdo(relatedObjectArtifactId);
				SourceProvider provider = _caseServiceContext.RsapiService.SourceProviderLibrary.Read(_integrationPoint.SourceProvider.Value);
				_identifier = provider.Identifier;
				_sourceConfig = _integrationPoint.SourceConfiguration;
			}

			public void CreateJobHistoryRdo()
			{
				_jobHistoryService.CreateRdo(_integrationPoint, _batchInstance, null);
			}

			public string SourceProviderIdentifier
			{
				get { return _identifier; }
			}

			public string SourceConfiguration { get { return _sourceConfig; } }
		}

		internal interface IIntegrationPointRdoAdaptor
		{
			void Initialize(int relatedObjectArtifactId, Guid batchInstance);

			void CreateJobHistoryRdo();

			string SourceProviderIdentifier { get; }

			string SourceConfiguration { get; }
		}

		internal class DestinationWorkspace
		{
			public int TargetWorkspaceArtifactId;
		}

		public class Payload
		{
			public int AppId { get; set; }
			public int ArtifactId { get; set; }
		}
	}
}