using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Web.Models;
using kCura.Relativity.Client;
using Relativity.Core.Service;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
    public class SavedSearchFinderController : ApiController
    {
        private readonly IRSAPIClient _context;
        private IHtmlSanitizerManager _htmlSanitizerManager;


        public SavedSearchFinderController(IRSAPIClient context, IHtmlSanitizerManager htmlSanitizerManager)
        {
            _context = context;
            _htmlSanitizerManager = htmlSanitizerManager;
        }

        [HttpGet]
        public HttpResponseMessage Get()
        {
            try
            {
                var results = SavedSearchModel.GetAllPublicSavedSearches(_context, _htmlSanitizerManager);
                return Request.CreateResponse(HttpStatusCode.OK, results);
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new List<SavedSearchModel>());
            }
        }
    }
}