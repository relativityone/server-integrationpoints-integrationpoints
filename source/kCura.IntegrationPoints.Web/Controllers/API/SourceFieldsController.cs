using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.Provider;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class SourceFieldsController : ApiController
	{
		private readonly IDataProviderFactory _factory;
		public SourceFieldsController()
		{
			
		}

		
		[Route("{workspaceID}/api/SourceFields/")]
		public HttpResponseMessage Get(string type, string options)
		{
			//var provider =_factory.GetDataProvider();
			//var fields = provider.GetFields(options);
			var fields = new List<FieldEntry>()
			{
				new FieldEntry() {DisplayName = "Age", FieldIdentifier = "2"},
				new FieldEntry() {DisplayName = "Database Name", FieldIdentifier = "1"},
				new FieldEntry() {DisplayName = "Date", FieldIdentifier = "4"},
				new FieldEntry() {DisplayName = "Department", FieldIdentifier = "3"},
				new FieldEntry() {DisplayName = "Field", FieldIdentifier = "5"},
			};
			return Request.CreateResponse(HttpStatusCode.OK, fields, Configuration.Formatters.JsonFormatter);
		}

	}
}
