using System;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using Relativity.CustomPages;
using Relativity.CustomPages.localhost;
using kCura.IntegrationPoints.Web;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class IntegrationPointsAPIController : ApiController
	{
		private readonly IntegrationPointService _reader;
		private readonly RelativityUrlHelper _urlHelper;
		private readonly Core.Services.Syncronizer.RDOSyncronizerProvider _provider;
		public IntegrationPointsAPIController(IntegrationPointService reader, RelativityUrlHelper urlHelper, Core.Services.Syncronizer.RDOSyncronizerProvider provider)
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
				model.DestinationProvider = _provider.GetRdoSyncronizerId(); //hard coded for your ease of use
			}
			return Request.CreateResponse(HttpStatusCode.Accepted, model);
		}

		[HttpPost]
		public HttpResponseMessage Update(int workspaceID, IntegrationModel model)
		{
			var createdId = _reader.SaveIntegration(model);
			//I'm already sorry to use name instead of guid
			//is this merge issue???
			//var result = _urlHelper.GetRelativityViewUrl(workspaceID, createdId, Data.ObjectTypes.IntegrationPoint);
			var result = _urlHelper.GetRelativityViewUrl(workspaceID, createdId, Data.ObjectTypes.IntegrationPoint);
			return Request.CreateResponse(HttpStatusCode.OK, new { returnURL = result });
		}

	}
}
