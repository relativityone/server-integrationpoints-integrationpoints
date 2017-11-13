using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Services.SourceTypes;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.IntegrationPoints.Web.Models;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class SourceTypeController : ApiController
	{
		private readonly ISourceTypeFactory _factory;
		private readonly ICaseServiceContext _serviceContext;
		private readonly IRsapiRdoQuery _rdoQuery;
		private readonly IAPILog _apiLog;

		public SourceTypeController(ISourceTypeFactory factory,
			ICaseServiceContext serviceContext,
			IRsapiRdoQuery objectTypeQuery,
			ICPHelper helper)
		{
			_factory = factory;
			_rdoQuery = objectTypeQuery;
			_serviceContext = serviceContext;
			_apiLog = helper.GetLoggerFactory().GetLogger();
		}

		[HttpGet]
		[LogApiExceptionFilter(Message = "Unable to retrieve source provider list.")]
		public HttpResponseMessage Get()
		{
			_apiLog.LogDebug("Retriving Source Provider Types...");
			Dictionary<Guid, int> rdoTypesCache = _rdoQuery.GetRdoGuidToArtifactIdMap(_serviceContext.WorkspaceUserID);
			List<SourceTypeModel> list = _factory.GetSourceTypes().Select(x => new SourceTypeModel()
			{
				name = x.Name,
				id = x.ArtifactID,
				value = x.ID,
				url = x.SourceURL,
				Config = new SourceProviderConfigModel(x.Config, rdoTypesCache)
			}).ToList();

			_apiLog.LogDebug($"Retriving Source Provider Types completed. Found: {list.Count} types");

			return Request.CreateResponse(HttpStatusCode.OK, list);
		}
	}
}
