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
		private readonly IIntegrationPointService _reader;
		private readonly IRelativityUrlHelper _urlHelper;
		private readonly Core.Services.Synchronizer.IRdoSynchronizerProvider _provider;
		private readonly ICaseServiceContext _context;
		private readonly IPermissionService _permissionService;

		private const string UNABLE_TO_SAVE_FORMAT = "Unable to save Integration Point:{0} cannot be changed once the Integration Point has been run";

		public IntegrationPointsAPIController(IIntegrationPointService reader,
			IRelativityUrlHelper urlHelper,
			Core.Services.Synchronizer.IRdoSynchronizerProvider provider,
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
			List<string> invalidProperties = new List<string>();
			IntegrationModel existingModel = null;
			if (model.ArtifactID > 0)
			{
				try
				{
					existingModel = _reader.ReadIntegrationPoint(model.ArtifactID);
				}
				catch (Exception e)
				{
					throw new Exception("Unable to save Integration Point: Unable to retrieve Integration Point", e);
				}

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
					if (existingModel.SourceProvider != model.SourceProvider)
					{
						// If the source provider has been changed, the code below this exception is invalid
						invalidProperties.Add("Source Provider");
						throw new Exception(String.Format(UNABLE_TO_SAVE_FORMAT, String.Join(",", invalidProperties.Select(x => $" {x}"))));
					}

					model.HasErrors = existingModel.HasErrors;
				}
			}

			// check permission if we want to push
			// needs to be here because custom page is the only place that has user context
			SourceProvider provider = null;
			try
			{
				provider = _context.RsapiService.SourceProviderLibrary.Read(model.SourceProvider);
			}
			catch (Exception e)
			{
				throw new Exception("Unable to save Integration Point: Unable to retrieve source provider", e);	
			}

			if (provider.Identifier.Equals(DocumentTransferProvider.Shared.Constants.RELATIVITY_PROVIDER_GUID))
			{
				ImportNowController.WorkspaceConfiguration workspaceConfiguration = JsonConvert.DeserializeObject<ImportNowController.WorkspaceConfiguration>(model.SourceConfiguration);
				if (_permissionService.UserCanImport(workspaceConfiguration.TargetWorkspaceArtifactId) == false)
				{
					throw new Exception(ImportNowController.NO_PERMISSION_TO_IMPORT);
				}
				if (_permissionService.UserCanEditDocuments(workspaceConfiguration.SourceWorkspaceArtifactId) == false)
				{
					throw new Exception(ImportNowController.NO_PERMISSION_TO_EDIT_DOCUMENTS);
				}

				if (existingModel != null && (existingModel.SourceConfiguration != model.SourceConfiguration))
				{
					invalidProperties.Add("Source Configuration");
				}
			}

			if (invalidProperties.Any())
			{
				throw new Exception(String.Format(UNABLE_TO_SAVE_FORMAT, String.Join(",", invalidProperties.Select(x => $" {x}"))));
			}

			int createdId = _reader.SaveIntegration(model);
			string result = _urlHelper.GetRelativityViewUrl(workspaceID, createdId, Data.ObjectTypes.IntegrationPoint);
			return Request.CreateResponse(HttpStatusCode.OK, new { returnURL = result });
		}

	}
}
