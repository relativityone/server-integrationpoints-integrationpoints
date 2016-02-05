﻿using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class IntegrationPointsAPIController : ApiController
	{
		private readonly IntegrationPointService _reader;
		private readonly RelativityUrlHelper _urlHelper;
		private readonly Core.Services.Synchronizer.RDOSynchronizerProvider _provider;
		public IntegrationPointsAPIController(IntegrationPointService reader, RelativityUrlHelper urlHelper, Core.Services.Synchronizer.RDOSynchronizerProvider provider)
		{
			_reader = reader;
			_urlHelper = urlHelper;
			_provider = provider;
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
			var createdId = _reader.SaveIntegration(model);
			var result = _urlHelper.GetRelativityViewUrl(workspaceID, createdId, Data.ObjectTypes.IntegrationPoint);
			return Request.CreateResponse(HttpStatusCode.OK, new { returnURL = result });
		}

	}
}
