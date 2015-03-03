using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Queries;
using kCura.IntegrationPoints.Core.Services.Provider;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class SourceOptions
	{
		public Guid Type { get; set; }
		public object Options { get; set; }
	}

	public class SourceFieldsController : ApiController
	{
		private readonly GetSourceProviderRdoByIdentifier _sourceProviderIdentifier;
		private readonly IDataProviderFactory _factory;
		public SourceFieldsController(GetSourceProviderRdoByIdentifier sourceProviderIdentifier, IDataProviderFactory factory)
		{
			_sourceProviderIdentifier = sourceProviderIdentifier;
			_factory = factory;
		}

		[HttpPost]
		[Route("{workspaceID}/api/SourceFields/")]
		public HttpResponseMessage Get(SourceOptions data)
		{
			Data.SourceProvider providerRdo = _sourceProviderIdentifier.Execute(data.Type);
			Guid applicationGuid = new Guid(providerRdo.ApplicationIdentifier);
			var provider = _factory.GetDataProvider(applicationGuid, data.Type);
			var fields = provider.GetFields(data.Options.ToString());
			return Request.CreateResponse(HttpStatusCode.OK, fields, Configuration.Formatters.JsonFormatter);
		}

	}
}
