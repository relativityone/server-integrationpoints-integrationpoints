using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Services.DestinationTypes;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Services.Synchronizer;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.IntegrationPoints.Web.Toggles;
using Relativity.Toggles;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class ToggleAPIController: ApiController
	{
		private readonly IToggleProvider _toggleProvider;

		public ToggleAPIController(IToggleProvider toggleProvider)
		{
			_toggleProvider = toggleProvider;
		}

		[HttpGet]
		[LogApiExceptionFilter(Message = "Missing toggle definition.")]
		public HttpResponseMessage Get(string toggleName)
		{
			try
			{
				var isEnabledByName = _toggleProvider.IsEnabledByName(toggleName);
				return Request.CreateResponse(HttpStatusCode.OK, isEnabledByName);
			}
			catch (MissingFeatureException)
			{
				return Request.CreateResponse(HttpStatusCode.NotFound, false);
			}
			
		}
	}
}