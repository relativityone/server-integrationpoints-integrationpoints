﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Web.Models;
using kCura.Relativity.Client;
using Relativity.Core.Service;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class SavedSearchFinderController : ApiController
	{
		private readonly IRSAPIClient _context;
		private readonly IHtmlSanitizerManager _htmlSanitizerManager;
		private readonly IErrorRepository _errorRepository;

		public SavedSearchFinderController(IRSAPIClient context, IRepositoryFactory repositoryFactory, IHtmlSanitizerManager htmlSanitizerManager)
		{
			_context = context;
			_htmlSanitizerManager = htmlSanitizerManager;
			_errorRepository = repositoryFactory.GetErrorRepository(_context.APIOptions.WorkspaceID);
		}

		[HttpGet]
		public HttpResponseMessage Get()
		{
			try
			{
				var results = SavedSearchModel.GetAllPublicSavedSearches(_context, _htmlSanitizerManager);
				return Request.CreateResponse(HttpStatusCode.OK, results);
			}
			catch (Exception exception)
			{
				ErrorDTO error = new ErrorDTO()
				{
					Message = "Unable to retrieve the saved searches. Please contract the system administrator.",
					FullText = $"{exception.Message}{Environment.NewLine}{exception.StackTrace}",
				};
				_errorRepository.Create(new[] { error });
				return Request.CreateResponse(HttpStatusCode.InternalServerError, new List<SavedSearchModel>());
			}
		}
	}
}