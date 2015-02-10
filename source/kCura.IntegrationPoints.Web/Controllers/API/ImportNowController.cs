﻿using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Contracts.Agent;


namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class ImportNowController : ApiController
	{
		private readonly IJobManager _jobManager;
		public ImportNowController(IJobManager jobManager)
		{
			_jobManager = jobManager;
		}

		// POST api/importnow
		public HttpResponseMessage Post(Payload payload)
		{

			int workspaceID = payload.AppId;
			int relatedObjectArtifactID = payload.ArtifactId;

			_jobManager.CreateJob<object>(null, TaskType.SyncManager, workspaceID, relatedObjectArtifactID);
			return Request.CreateResponse(HttpStatusCode.OK);
		}

		public class Payload
		{
			public int AppId { get; set; }
			public int ArtifactId { get; set; }
		}
	}
}