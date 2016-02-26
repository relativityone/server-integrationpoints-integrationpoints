namespace kCura.IntegrationPoints.Web.Controllers.API
{
	using System;
	using System.Collections.Generic;
	using System.Net;
	using System.Net.Http;
	using System.Web.Http;
	using kCura.IntegrationPoints.Web.Models;
	using kCura.Relativity.Client;

	public class SavedSearchFinderController : ApiController
	{
		private readonly IRSAPIClient _context;

		public SavedSearchFinderController(IRSAPIClient context)
		{
			_context = context;
		}

		[HttpGet]
		public HttpResponseMessage Get()
		{
			try
			{
				var results = SavedSearchModel.GetAllPublicSavedSearches(_context);
				return Request.CreateResponse(HttpStatusCode.OK, results);
			}
			catch (Exception)
			{
				return Request.CreateResponse(HttpStatusCode.InternalServerError, new List<SavedSearchModel>());
			}
		}
	}
}