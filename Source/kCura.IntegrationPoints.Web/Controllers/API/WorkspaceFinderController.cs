﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.IntegrationPoints.Web.Models;
using kCura.Relativity.Client;
using Relativity.API;
using Relativity.Core.Service;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class WorkspaceFinderController : ApiController
	{
		private readonly IHtmlSanitizerManager _htmlSanitizerManager;
		private readonly IHelperFactory _helperFactory;
		private readonly ICPHelper _helper;
		private readonly IManagerFactory _managerFactory;
		private readonly IContextContainerFactory _contextContainerFactory;

		public WorkspaceFinderController(IManagerFactory managerFactory, IContextContainerFactory contextContainerFactory, IHtmlSanitizerManager htmlSanitizerManager, ICPHelper helper, IHelperFactory helperFactory)
		{
			_managerFactory = managerFactory;
			_contextContainerFactory = contextContainerFactory;
			_helper = helper;
			_helperFactory = helperFactory;
			_htmlSanitizerManager = htmlSanitizerManager;
		}

		[HttpGet]
		[LogApiExceptionFilter(Message = "Unable to retrieve the workspace information.")]
		public HttpResponseMessage Get(int? federatedInstanceId = null)
		{
			var targetHelper = _helperFactory.CreateTargetHelper(_helper, federatedInstanceId);
			IWorkspaceManager workspaceManager =
				_managerFactory.CreateWorkspaceManager(_contextContainerFactory.CreateContextContainer(_helper, targetHelper.GetServicesManager()));
			IEnumerable<WorkspaceDTO> userWorkspaces = workspaceManager.GetUserActiveWorkspaces();

			WorkspaceModel[] workspaceModels = userWorkspaces.Select(x =>
				new WorkspaceModel
				{
					DisplayName = Utils.GetFormatForWorkspaceOrJobDisplay(
						_htmlSanitizerManager.Sanitize(x.Name).CleanHTML,
						x.ArtifactId
					),
					Value = x.ArtifactId
				}
			).ToArray();

			return Request.CreateResponse(HttpStatusCode.OK, workspaceModels);
		}
	}
}