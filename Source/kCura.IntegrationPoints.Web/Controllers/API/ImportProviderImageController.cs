﻿using System.Linq;
using System.Net;
using System.Web.Http;
using kCura.WinEDDS.Service.Export;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using kCura.IntegrationPoints.Core.Authentication;
using Relativity.API;
using Relativity.DataExchange.Service;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
    public class ImportProviderImageController : ApiController
    {

		private readonly IManagerFactory _managerFactory;
		private readonly ICaseManagerFactory _caseManagerFactory;
		private readonly ICPHelper _helper;
		private readonly ICredentialProvider _credential;

		public ImportProviderImageController(
			IManagerFactory managerFactory, 
			ICPHelper helper, 
			ICredentialProvider credential, 
			ICaseManagerFactory caseManagerFactory)
		{
			_managerFactory = managerFactory;
			_helper = helper;
			_credential = credential;
			_caseManagerFactory = caseManagerFactory;
		}

		[HttpGet]
		[LogApiExceptionFilter(Message = "Unable to retrieve list of Overlay Identifier values.")]
		public IHttpActionResult GetOverlayIdentifierFields(int workspaceArtifactId)
		{
			Core.Managers.IFieldManager fieldManager = _managerFactory.CreateFieldManager();
			
			ArtifactFieldDTO[] fieldResults = fieldManager.RetrieveBeginBatesFields(workspaceArtifactId);
			return Json(fieldResults);
		}

		[HttpGet]
		[LogApiExceptionFilter(Message = "Unable to retrieve list of file repository values.")]
		public IHttpActionResult GetFileRepositories(int workspaceArtifactId)
		{
			CookieContainer cookieContainer = new CookieContainer();
			ICaseManager caseManager = _caseManagerFactory.Create(_credential.Authenticate(cookieContainer), cookieContainer);
			
			CaseInfo caseInfo = caseManager.Read(workspaceArtifactId);
			string[] fileRepos = caseManager.GetAllDocumentFolderPathsForCase(caseInfo.ArtifactID).OrderBy(x => x).ToArray(); ;

			return Json(fileRepos);
		}

		[HttpGet]
		[LogApiExceptionFilter(Message = "Unable to retrieve the default file repo.")]
		public IHttpActionResult GetDefaultFileRepo(int workspaceArtifactId)
		{
			CookieContainer cookieContainer = new CookieContainer();
			ICaseManager caseManager = _caseManagerFactory.Create(_credential.Authenticate(cookieContainer), cookieContainer);

			CaseInfo caseInfo = caseManager.Read(workspaceArtifactId);

			return Json(caseInfo.DocumentPath);
		}
	}
}
