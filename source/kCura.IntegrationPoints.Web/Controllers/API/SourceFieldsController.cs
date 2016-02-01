using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Queries;
using kCura.IntegrationPoints.Core.Services.Provider;
using Relativity.API;

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
		private readonly IHelper _helper;

		public SourceFieldsController(
			GetSourceProviderRdoByIdentifier sourceProviderIdentifier, 
			IDataProviderFactory factory, 
			IHelper helper)
		{
			_sourceProviderIdentifier = sourceProviderIdentifier;
			_factory = factory;
			_helper = helper;
		}

		[HttpPost]
		[Route("{workspaceID}/api/SourceFields/")]
		public HttpResponseMessage Get(SourceOptions data)
		{
			Data.SourceProvider providerRdo = _sourceProviderIdentifier.Execute(data.Type);
			Guid applicationGuid = new Guid(providerRdo.ApplicationIdentifier);
			var provider = _factory.GetDataProvider(applicationGuid, data.Type, _helper);
			var fields = provider.GetFields(data.Options.ToString()).ToList();
			return Request.CreateResponse(HttpStatusCode.OK, fields, Configuration.Formatters.JsonFormatter);
		}

	}
}
