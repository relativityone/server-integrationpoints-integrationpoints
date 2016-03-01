using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Services;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class ImportNowController : ApiController
	{
		private readonly IJobManager _jobManager;
		private IntegrationPointService _integrationPointService;
		private JobHistoryService _jobHistoryService;

		public ImportNowController(IJobManager jobManager, IntegrationPointService integrationPointService, JobHistoryService jobHistoryService)
		{
			_jobManager = jobManager;
			_integrationPointService = integrationPointService;
			_jobHistoryService = jobHistoryService;
		}

		// POST api/importnow
		public HttpResponseMessage Post(Payload payload)
		{
			int workspaceID = payload.AppId;
			int relatedObjectArtifactID = payload.ArtifactId;
			Guid batchInstance = Guid.NewGuid();
			var jobDetails = new TaskParameters()
			{
				BatchInstance = batchInstance
			};
			Data.IntegrationPoint integrationPoint = _integrationPointService.GetRdo(relatedObjectArtifactID);
			_jobHistoryService.CreateRdo(integrationPoint, batchInstance, null);
			_jobManager.CreateJob(jobDetails, TaskType.SyncManager, workspaceID, relatedObjectArtifactID);

			return Request.CreateResponse(HttpStatusCode.OK);
		}

		public class Payload
		{
			public int AppId { get; set; }
			public int ArtifactId { get; set; }
		}
	}
}