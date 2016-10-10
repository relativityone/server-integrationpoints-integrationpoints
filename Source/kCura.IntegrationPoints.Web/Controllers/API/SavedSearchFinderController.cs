using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.IntegrationPoints.Web.Models;
using kCura.Relativity.Client;
using Relativity.Core.Service;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class SavedSearchFinderController : ApiController
	{
		private readonly IRSAPIClient _context;
		private readonly IHtmlSanitizerManager _htmlSanitizerManager;

		public SavedSearchFinderController(IRSAPIClient context, IHtmlSanitizerManager htmlSanitizerManager)
		{
			_context = context;
			_htmlSanitizerManager = htmlSanitizerManager;
		}

		[HttpGet]
		[LogApiExceptionFilter(Message = "Unable to retrieve public saved searches list.")]
		public HttpResponseMessage Get()
		{
			var results = SavedSearchModel.GetAllPublicSavedSearches(_context, _htmlSanitizerManager);
			return Request.CreateResponse(HttpStatusCode.OK, results);
		}
	}
}