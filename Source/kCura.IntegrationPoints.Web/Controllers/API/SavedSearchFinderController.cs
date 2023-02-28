using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Data.DTO;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.IntegrationPoints.Web.Models;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
    public class SavedSearchFinderController : ApiController
    {
        private const int _DEFAULT_PAGE_SIZE = 100;
        private readonly IRepositoryFactory _repositoryFactory;

        public SavedSearchFinderController(IRepositoryFactory repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
        }

        [HttpGet]
        [LogApiExceptionFilter(Message = "Unable to retrieve public saved searches list.")]
        public HttpResponseMessage Get(int workspaceId)
        {
            ISavedSearchQueryRepository savedSearchQueryRepository = _repositoryFactory.GetSavedSearchQueryRepository(workspaceId);
            List<SavedSearchModel> results = savedSearchQueryRepository.RetrievePublicSavedSearches().Select(item => new SavedSearchModel(item)).ToList();

            return Request.CreateResponse(HttpStatusCode.OK, results);
        }

        [HttpGet]
        [LogApiExceptionFilter(Message = "Unable to retrieve public saved searches list.")]
        public HttpResponseMessage Get(int workspaceId, string search, int page = 1, int pageSize = _DEFAULT_PAGE_SIZE)
        {
            ISavedSearchQueryRepository savedSearchQueryRepository = _repositoryFactory.GetSavedSearchQueryRepository(workspaceId);

            var request = new SavedSearchQueryRequest(search, page, pageSize);
            SavedSearchQueryResult results = savedSearchQueryRepository.RetrievePublicSavedSearches(request);

            List<SavedSearchModel> mappedResults = results.SavedSearches.Select(item => new SavedSearchModel(item)).ToList();
            var output = new SavedSearchResultsModel
            {
                Results = mappedResults,
                TotalResults = results.TotalResults,
                HasMoreResults = results.HasMoreResults
            };

            return Request.CreateResponse(HttpStatusCode.OK, output);
        }

        [HttpGet]
        [LogApiExceptionFilter(Message = "Unable to retrieve public saved search.")]
        public HttpResponseMessage Get(int workspaceId, int savedSearchId)
        {
            ISavedSearchQueryRepository savedSearchQueryRepository = _repositoryFactory.GetSavedSearchQueryRepository(workspaceId);

            SavedSearchDTO savedSearch = savedSearchQueryRepository.RetrieveSavedSearch(savedSearchId);
            if (savedSearch != null && savedSearch.IsPublic)
            {
                var output = new SavedSearchModel(savedSearch);
                return Request.CreateResponse(HttpStatusCode.OK, output);
            }

            return Request.CreateResponse(HttpStatusCode.NotFound);
        }
    }
}
