using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class IntegrationPointsAPIController : ApiController
	{
		private readonly IIntegrationPointService _reader;
		private readonly IRelativityUrlHelper _urlHelper;
		private readonly Core.Services.Synchronizer.IRdoSynchronizerProvider _provider;

		public IntegrationPointsAPIController(IIntegrationPointService reader,
			IRelativityUrlHelper urlHelper,
			Core.Services.Synchronizer.IRdoSynchronizerProvider provider)
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
				try
				{
					model.DestinationProvider = _provider.GetRdoSynchronizerId(); //hard coded for your ease of use
				}
				catch (Exception e)
				{
					model.DestinationProvider = 0;
				}
			}
			return Request.CreateResponse(HttpStatusCode.Accepted, model);
		}

		[HttpPost]
		public HttpResponseMessage Update(int workspaceID, IntegrationModel model)
		{
			try
			{
				int createdId = _reader.SaveIntegration(model);
				string result = _urlHelper.GetRelativityViewUrl(workspaceID, createdId, Data.ObjectTypes.IntegrationPoint);
				return Request.CreateResponse(HttpStatusCode.OK, new { returnURL = result });
			}
			catch (Exception exception)
			{
				return Request.CreateResponse(HttpStatusCode.PreconditionFailed, exception.Message);
			}
		}
	}
}