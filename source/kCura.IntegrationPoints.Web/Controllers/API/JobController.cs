using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Services;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class JobController : ApiController
	{
		private const string _RELATIVITY_USERID = "rel_uai";

		private readonly IIntegrationPointService _integrationPointService;

		public JobController(IIntegrationPointService integrationPointService)
		{
			_integrationPointService = integrationPointService;
		}
		
		// POST API/Job/Run
		[HttpPost]
		public HttpResponseMessage Run(Payload payload)
		{
			HttpResponseMessage httpResponseMessage = Internal(payload.AppId, payload.ArtifactId, _integrationPointService.RunIntegrationPoint);
			return httpResponseMessage;
		}

		// POST API/Job/Retry
		[HttpPost]
		public HttpResponseMessage Retry(Payload payload)
		{
			HttpResponseMessage httpResponseMessage = Internal(payload.AppId, payload.ArtifactId, _integrationPointService.RetryIntegrationPoint);
			return httpResponseMessage;
		}

		private HttpResponseMessage Internal(int workspaceId, int relatedObjectArtifactId, Action<int, int, int> integrationPointServiceMethod)
		{
			try
			{
				int userId = GetUserIdIfExists();
				integrationPointServiceMethod(workspaceId, relatedObjectArtifactId, userId);
			}
			catch (AggregateException exception)
			{
				IEnumerable<string> innerExceptions = exception.InnerExceptions.Where(ex => ex != null).Select(ex => ex.Message);
				return Request.CreateResponse(HttpStatusCode.BadRequest, String.Format("{0} : {1}", exception.Message, String.Join(",", innerExceptions)));
			}
			catch (Exception exception)
			{
				return Request.CreateResponse(HttpStatusCode.BadRequest, exception.Message);
			}
			return Request.CreateResponse(HttpStatusCode.OK);
		}

		private int GetUserIdIfExists()
		{
			var user = User as ClaimsPrincipal;
			if (user != null)
			{
				foreach (Claim claim in user.Claims)
				{
					if (_RELATIVITY_USERID.Equals(claim.Type, StringComparison.OrdinalIgnoreCase))
					{
						return Convert.ToInt32(claim.Value);
					}
				}
			}
			return 0;
		}

		public class Payload
		{
			public int AppId { get; set; }
			public int ArtifactId { get; set; }
		}
	}
}