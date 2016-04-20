using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using Newtonsoft.Json;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class IntegrationPointsAPIController : ApiController
	{
		private readonly IntegrationPointService _reader;
		private readonly RelativityUrlHelper _urlHelper;
		private readonly Core.Services.Synchronizer.RdoSynchronizerProvider _provider;
		private readonly ICaseServiceContext _context;
		private readonly IPermissionService _permissionService;

		public IntegrationPointsAPIController(IntegrationPointService reader,
			RelativityUrlHelper urlHelper,
			Core.Services.Synchronizer.RdoSynchronizerProvider provider,
			ICaseServiceContext context,
			IPermissionService permissionService)
		{
			_reader = reader;
			_urlHelper = urlHelper;
			_provider = provider;
			_context = context;
			_permissionService = permissionService;
		}
		[HttpGet]
		public HttpResponseMessage Get(int id)
		{
			var model = new IntegrationModel();
			model.ArtifactID = id;
			if (id > 0)
			{
				model = _reader.ReadIntegrationPoint(id);
			}
			if (model.DestinationProvider == 0)
			{
				model.DestinationProvider = _provider.GetRdoSynchronizerId(); //hard coded for your ease of use
			}
			return Request.CreateResponse(HttpStatusCode.Accepted, model);
		}

		[HttpPost]
		public HttpResponseMessage Update(int workspaceID, IntegrationModel model)
		{
			// check that only fields that are allowed to be changed are changed
			IntegrationModel existingModel = _reader.ReadIntegrationPoint(model.ArtifactID);
			List<string> invalidProperties = new List<string>();
			if (existingModel.LastRun.HasValue)
			{
				if (existingModel.Name != model.Name)
				{
					invalidProperties.Add("Name");	
				}
				if (existingModel.DestinationProvider != model.DestinationProvider)
				{
					invalidProperties.Add("Destination Provider");	
				}
				if (existingModel.SourceProvider != model.SourceProvider)
				{
					invalidProperties.Add("Source Provider");	
				}
				if (existingModel.Destination != model.Destination)
				{
					dynamic existingDestination = JsonConvert.DeserializeObject(existingModel.Destination);
					dynamic newDestination = JsonConvert.DeserializeObject(model.Destination);

					if (existingDestination.artifactTypeID != newDestination.artifactTypeID)
					{
						invalidProperties.Add("Destination RDO");
					}
					if (existingDestination.CaseArtifactId != newDestination.CaseArtifactId)
					{
						invalidProperties.Add("Case");	
					}
				}
				if (existingModel.SourceConfiguration != model.SourceConfiguration)
				{
					invalidProperties.Add("Source Configruation"); // This is the data provider specific settings
				}

			}

			// check permission if we want to push
			// needs to be here because custom page is the only place that has user context
			SourceProvider provider = _context.RsapiService.SourceProviderLibrary.Read(model.SourceProvider);
			if (provider.Identifier.Equals(DocumentTransferProvider.Shared.Constants.RELATIVITY_PROVIDER_GUID))
			{
				ImportNowController.DestinationWorkspace destinationWorkspace = JsonConvert.DeserializeObject<ImportNowController.DestinationWorkspace>(model.SourceConfiguration);
				if (_permissionService.UserCanImport(destinationWorkspace.TargetWorkspaceArtifactId) == false)
				{
					throw new Exception(ImportNowController.NO_PERMISSION_TO_IMPORT);
				}
			}

			if (invalidProperties.Any())
			{
				throw new Exception($"Unable to save Integration Point:{String.Join(",", invalidProperties.Select(x => $" {x}"))} cannot be changed once the Integration Point has been run");
			}

			int createdId = _reader.SaveIntegration(model);
			string result = _urlHelper.GetRelativityViewUrl(workspaceID, createdId, Data.ObjectTypes.IntegrationPoint);
			return Request.CreateResponse(HttpStatusCode.OK, new { returnURL = result });
		}

	}
}
