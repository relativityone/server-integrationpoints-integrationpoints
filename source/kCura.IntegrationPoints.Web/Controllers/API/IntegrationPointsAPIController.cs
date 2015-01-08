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
		public IntegrationPointsAPIController(IntegrationPointService reader,RelativityUrlHelper urlHelper)
		{
			_reader = reader;
			_urlHelper = urlHelper;
		}
		[HttpGet]
		public HttpResponseMessage Get(int id)
		{
			var model = new IntegrationModel();
			model.ArtifactID = id;
			if (id> 0)
			{
				model = _reader.ReadIntegrationPoint(id);
			}

			return Request.CreateResponse(HttpStatusCode.Accepted, model);
		}

		[HttpPost]
		public HttpResponseMessage Update(IntegrationModel model)
		{
			var createdId = _reader.SaveIntegration(model);
			var result = _urlHelper.GetRelativityViewUrl(createdId, 0);
			return Request.CreateResponse(HttpStatusCode.Redirect, result);
		}

	}
}
