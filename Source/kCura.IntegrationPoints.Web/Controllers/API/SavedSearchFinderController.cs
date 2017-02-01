using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.IntegrationPoints.Web.Models;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class SavedSearchFinderController : ApiController
	{
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
			List<SavedSearchModel> results = savedSearchQueryRepository.RetrievePublicSavedSearches().Select(
				item => new SavedSearchModel()
				{
					DisplayName = item.Name,
					Value = item.ArtifactId
				}).ToList();

			return Request.CreateResponse(HttpStatusCode.OK, results);
		}
	}
}