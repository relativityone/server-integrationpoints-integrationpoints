﻿using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Web.Attributes;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class SavedSearchesTreeController : ApiController
	{
		private readonly ISavedSearchesTreeService _savedSearchesService;

		public SavedSearchesTreeController(ISavedSearchesTreeService savedSearchesService)
		{
			_savedSearchesService = savedSearchesService;
		}

		[HttpGet]
		[LogApiExceptionFilter(Message = "Unable to retrieve saved searches list.")]
		public HttpResponseMessage Get(int workspaceArtifactId)
		{
			var tree = _savedSearchesService.GetSavedSearchesTree(workspaceArtifactId);
			return Request.CreateResponse(HttpStatusCode.OK, tree);
		}
	}
}