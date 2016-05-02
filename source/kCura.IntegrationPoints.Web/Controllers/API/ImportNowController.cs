using System;
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
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Newtonsoft.Json;


namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class ImportNowController : ApiController
	{
		private const string _INTEGRATIONPOINT_ARTIFACT_ID_GUID = "A992C6FD-B6C2-4B97-AAFB-2CFB3F666F62";
		private const string _SOURCEPROVIDER_ARTIFACT_ID_GUID = "4A091F69-D750-441C-A4F0-24C990D208AE";

		private const string _RELATIVITY_USERID = "rel_uai";
		internal const string NO_PERMISSION_TO_IMPORT = "You do not have permission to push documents to the destination workspace selected. Please contact your system administrator.";
		internal const string NO_PERMISSION_TO_EDIT_DOCUMENTS =
			"You do not have permission to edit documents in the current workspace. Please contact your system administrator.";
		internal const string NO_USERID = "Unable to determine the user id. Please contact your system administrator.";

		private readonly IJobManager _jobManager;
		private readonly ICaseServiceContext _caseServiceContext;
		private readonly IPermissionService _permissionService;
		private readonly IIntegrationPointRdoAdaptor _rdoDependenciesAdaptor;

		public ImportNowController(IJobManager jobManager,
			ICaseServiceContext caseServiceContext,
			IIntegrationPointService integrationPointService,
			JobHistoryService jobHistoryService,
			IPermissionService permissionService)
			: this( jobManager, caseServiceContext, permissionService,
				new IntegrationPointRdoInitializer(integrationPointService, caseServiceContext, jobHistoryService))
		{
		}

		internal ImportNowController(IJobManager jobManager,
			ICaseServiceContext caseServiceContext,
			IPermissionService permissionService,
			IIntegrationPointRdoAdaptor rdoAdaptor)
		{
			_jobManager = jobManager;
			_caseServiceContext = caseServiceContext;
			_permissionService = permissionService;
			_rdoDependenciesAdaptor = rdoAdaptor;
		}

		// POST api/importnow
		[HttpPost]
		public HttpResponseMessage Post(Payload payload)
		{
			HttpResponseMessage httpResponseMessage = Internal(payload.AppId, payload.ArtifactId);
			return httpResponseMessage;
		}

		[HttpPost]
		public bool SubmitLastJob(int workspaceId)
		{
			// Get last created integration point
			Query<RDO> query1 = new Query<RDO>
			{
				Fields = new List<FieldValue> { new FieldValue(_SOURCEPROVIDER_ARTIFACT_ID_GUID) },
				Condition = new TextCondition(Guid.Parse(SourceProviderFieldGuids.Identifier), TextConditionEnum.EqualTo, DocumentTransferProvider.Shared.Constants.RELATIVITY_PROVIDER_GUID),
			};
			List<SourceProvider> sourceProviders = _caseServiceContext.RsapiService.SourceProviderLibrary.Query(query1);
			int sourceProviderArtifactId = sourceProviders.First().ArtifactId;

			Query<RDO> query2 = new Query<RDO>
			{
				Fields = new List<FieldValue> {new FieldValue(_INTEGRATIONPOINT_ARTIFACT_ID_GUID)},
				Condition = new WholeNumberCondition(Guid.Parse(IntegrationPointFieldGuids.SourceProvider), NumericConditionEnum.EqualTo, sourceProviderArtifactId),
				Sorts = new List<Sort>
				{
					new Sort
					{
						Field = "ArtifactID",
						Direction = SortEnum.Descending
					}
				}
			};

			List<IntegrationPoint> integrationPoints = _caseServiceContext.RsapiService.IntegrationPointLibrary.Query(query2);
			if (!integrationPoints.Any())
			{
				return false;
			}

			HttpResponseMessage message = Internal(workspaceId, integrationPoints.First().ArtifactId);
			return message.IsSuccessStatusCode;
		}

		private HttpResponseMessage Internal(int workspaceId, int relatedObjectArtifactId)
		{
			try
			{
				Guid batchInstance = Guid.NewGuid();
				var jobDetails = new TaskParameters()
				{
					BatchInstance = batchInstance
				};
				
				_rdoDependenciesAdaptor.Initialize(relatedObjectArtifactId, batchInstance);

				int userId = GetUserIdIfExists();
				// if relativity provider is selected, we will create an export task
				if (_rdoDependenciesAdaptor.SourceProviderIdentifier.Equals(DocumentTransferProvider.Shared.Constants.RELATIVITY_PROVIDER_GUID))
				{
					WorkspaceConfiguration workspaceConfiguration = JsonConvert.DeserializeObject<WorkspaceConfiguration>(_rdoDependenciesAdaptor.SourceConfiguration);
					if(_permissionService.UserCanImport(workspaceConfiguration.TargetWorkspaceArtifactId) == false)
					{
						throw new Exception(NO_PERMISSION_TO_IMPORT);
					}
					if (_permissionService.UserCanEditDocuments(workspaceConfiguration.SourceWorkspaceArtifactId) == false)
					{
						throw new Exception(NO_PERMISSION_TO_EDIT_DOCUMENTS);
					}

					if (userId == 0)
					{
						throw new Exception(NO_USERID);
					}

					_rdoDependenciesAdaptor.CreateJobHistoryRdo();
					_jobManager.CreateJobOnBehalfOfAUser(jobDetails,  TaskType.ExportService, workspaceId, relatedObjectArtifactId, userId);
				}
				else
				{
					_rdoDependenciesAdaptor.CreateJobHistoryRdo();
					_jobManager.CreateJobOnBehalfOfAUser(jobDetails, TaskType.SyncManager, workspaceId, relatedObjectArtifactId, userId);
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

		private int GetUserIdIfExists()
		{
			var user = this.User as ClaimsPrincipal;
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

		internal class WorkspaceConfiguration
		{
			public int TargetWorkspaceArtifactId;
			public int SourceWorkspaceArtifactId;
		}

		public class Payload
		{
			public int AppId { get; set; }
			public int ArtifactId { get; set; }
		}
	}
}