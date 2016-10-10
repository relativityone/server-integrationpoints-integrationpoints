using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Services.DestinationTypes;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Services.SourceTypes;
using kCura.IntegrationPoints.Core.Services.Synchronizer;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.IntegrationPoints.Web.Models;
using kCura.IntegrationPoints.Web.Toggles;
using Relativity.Toggles;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class DestinationTypeController : ApiController
	{
		private readonly IDestinationTypeFactory _factory;
		private readonly IToggleProvider _toggleProvider;
		private readonly ICaseServiceContext _serviceContext;
		private readonly IObjectTypeQuery _rdoQuery;

		public DestinationTypeController(IDestinationTypeFactory factory,
			IToggleProvider toggleProvider,
			ICaseServiceContext serviceContext,
			RSAPIRdoQuery objectTypeQuery)
		{
			_factory = factory;
			_toggleProvider = toggleProvider;
			_rdoQuery = objectTypeQuery;
			_serviceContext = serviceContext;
		}

		[HttpGet]
		[LogApiExceptionFilter(Message = "Unable to retrieve destination provider type info.")]
		public HttpResponseMessage Get()
		{
			Dictionary<Guid, int> rdoTypesCache = _rdoQuery.GetRdoGuidToArtifactIdMap(_serviceContext.WorkspaceUserID);
			List<DestinationType> list = _factory.GetDestinationTypes().ToList();

			// TODO: Remove the toggle once the Relativity Destination provider is ready
			bool isShowRelativityDataProviderToggleEnabled = _toggleProvider.IsEnabled<ShowFileShareDataProviderToggle>();//TODO toggle
			if (!isShowRelativityDataProviderToggleEnabled)
			{
				for (int i = 0; i < list.Count; i++)
				{
					if (list.ElementAt(i).ID == RdoSynchronizerProvider.FILES_SYNC_TYPE_GUID)//TODO GUID Fileshare provider
					{
						list.RemoveAt(i);
						break;
					}
				}
			}
			return Request.CreateResponse(HttpStatusCode.OK, list);
		}
	}
}
