using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class ImportNowController : ApiController
	{
		private const string _INTEGRATIONPOINT_ARTIFACT_ID_GUID = "A992C6FD-B6C2-4B97-AAFB-2CFB3F666F62";
		private const string _SOURCEPROVIDER_ARTIFACT_ID_GUID = "4A091F69-D750-441C-A4F0-24C990D208AE";
		private const string _RELATIVITY_USERID = "rel_uai";

		private readonly IIntegrationPointService _integrationPointService;
		private readonly ICaseServiceContext _caseServiceContext;

		public ImportNowController(ICaseServiceContext caseServiceContext, IIntegrationPointService integrationPointService)
		{
			_caseServiceContext = caseServiceContext;
			_integrationPointService = integrationPointService;
		}
		
		// POST API/ImportNow
		[HttpPost]
		public HttpResponseMessage Post(Payload payload)
		{
			HttpResponseMessage httpResponseMessage = Internal(payload.AppId, payload.ArtifactId, _integrationPointService.RunIntegrationPoint);
			return httpResponseMessage;
		}

		// POST API/SubmitLastJob
		[HttpPost]
		public bool SubmitLastJob(int workspaceId)
		{
			// Get last created integration point
			Query<RDO> query1 = new Query<RDO>
			{
				Fields = new List<FieldValue> { new FieldValue(_SOURCEPROVIDER_ARTIFACT_ID_GUID) },
				Condition = new TextCondition(Guid.Parse(SourceProviderFieldGuids.Identifier), TextConditionEnum.EqualTo, DocumentTransferProvider.Shared.Constants.RELATIVITY_PROVIDER_GUID),
			};
			List<SourceProvider> sourceProviders = _caseServiceContext.RsapiService.SourceProviderLibrary.Query(query1);
			int sourceProviderArtifactId = sourceProviders.First().ArtifactId;

			Query<RDO> query2 = new Query<RDO>
			{
				Fields = new List<FieldValue> { new FieldValue(_INTEGRATIONPOINT_ARTIFACT_ID_GUID) },
				Condition = new WholeNumberCondition(Guid.Parse(IntegrationPointFieldGuids.SourceProvider), NumericConditionEnum.EqualTo, sourceProviderArtifactId),
				Sorts = new List<Sort>
				{
					new Sort
					{
						Field = "ArtifactID",
						Direction = SortEnum.Descending
					}
				}
			};

			List<IntegrationPoint> integrationPoints = _caseServiceContext.RsapiService.IntegrationPointLibrary.Query(query2);
			if (!integrationPoints.Any())
			{
				return false;
			}

			HttpResponseMessage message = Internal(workspaceId, integrationPoints.First().ArtifactId, _integrationPointService.RunIntegrationPoint);
			return message.IsSuccessStatusCode;
		}

		// POST API/RetryJob
		[HttpPost]
		public HttpResponseMessage RetryJob(Payload payload)
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
				return Request.CreateResponse(HttpStatusCode.BadRequest, String.Format("{0} : {1}" , exception.Message, String.Join(",", innerExceptions)));
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