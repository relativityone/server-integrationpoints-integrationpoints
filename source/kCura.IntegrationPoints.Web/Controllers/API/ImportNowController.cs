﻿using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Contracts.Synchronizer;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Newtonsoft.Json;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class ImportNowController : ApiController
	{
		private readonly IJobManager _jobManager;
		private IntegrationPointService _integrationPointService;
		private JobHistoryService _jobHistoryService;
		private ICaseServiceContext _caseServiceContext;

		public ImportNowController(IJobManager jobManager,
			ICaseServiceContext caseServiceContext,
			IntegrationPointService integrationPointService,
			JobHistoryService jobHistoryService)
		{
			_jobManager = jobManager;
			_integrationPointService = integrationPointService;
			_jobHistoryService = jobHistoryService;
			_caseServiceContext = caseServiceContext;
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
		  	var sourceProvider = _caseServiceContext.RsapiService.SourceProviderLibrary.Read(integrationPoint.SourceProvider.Value);

            var json = JsonConvert.DeserializeObject<ImportSettings>(integrationPoint.DestinationConfiguration);
            if (json.DestinationProviderType != null && json.DestinationProviderType.ToLower() == "fileshare")
		    {
                _jobManager.CreateJob(jobDetails, TaskType.ExportManager, workspaceID, relatedObjectArtifactID);
            }
            // if relativity provider is selected, we will create an export task
            else if (sourceProvider.Identifier.Equals(DocumentTransferProvider.Shared.Constants.RELATIVITY_PROVIDER_GUID))
			{
				_jobManager.CreateJob(jobDetails, TaskType.ExportService, workspaceID, relatedObjectArtifactID);
			}
			else
			{
				_jobManager.CreateJob(jobDetails, TaskType.SyncManager, workspaceID, relatedObjectArtifactID);
			}
			return Request.CreateResponse(HttpStatusCode.OK);
		}

		public class Payload
		{
			public int AppId { get; set; }
			public int ArtifactId { get; set; }
		}
	}
}