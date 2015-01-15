using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Services.Provider;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class SourceOptions
	{
		public Guid Type { get; set; }
		public string Options { get; set; }
	}

	public class SourceFieldsController : ApiController
	{
		private readonly IDataProviderFactory _factory;
		public SourceFieldsController(IDataProviderFactory factory)
		{
			_factory = factory;
		}

		[HttpPost]
		[Route("{workspaceID}/api/SourceFields/")]
		public HttpResponseMessage Get(SourceOptions data)
		{
			var provider = _factory.GetDataProvider(data.Type);
			var fields = provider.GetFields(data.Options);
			return Request.CreateResponse(HttpStatusCode.OK, fields, Configuration.Formatters.JsonFormatter);
		}

	}
}
