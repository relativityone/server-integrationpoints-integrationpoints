using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Services.SourceTypes;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Web.Models;
using kCura.IntegrationPoints.Web.Toggles;
using Relativity.Toggles;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class SourceTypeController : ApiController
	{
		private readonly ISourceTypeFactory _factory;
		private readonly IToggleProvider _toggleProvider;
		private readonly ICaseServiceContext _serviceContext;
		private readonly IObjectTypeQuery _rdoQuery;

		public SourceTypeController(ISourceTypeFactory factory,
			IToggleProvider toggleProvider,
			ICaseServiceContext serviceContext,
			IObjectTypeQuery objectTypeQuery)
		{
			_factory = factory;
			_toggleProvider = toggleProvider;
			_rdoQuery = objectTypeQuery;
			_serviceContext = serviceContext;
		}

		[HttpGet]
		public HttpResponseMessage Get()
		{

			Dictionary<Guid, int> rdoTypesCache = _rdoQuery.GetRdoGuidToArtifactIdMap(_serviceContext.WorkspaceUserID);
			List<SourceTypeModel> list = _factory.GetSourceTypes().Select(x => new SourceTypeModel()
			{
				name = x.Name,
				id = x.ArtifactID,
				value = x.ID,
				url = x.SourceURL,
				config = new SourceProviderConfigModel(x.Config, rdoTypesCache)
			}).ToList();

			// TODO: Remove the toggle once the Relativity provider is ready
			bool isShowRelativityDataProviderToggleEnabled = _toggleProvider.IsEnabled<ShowRelativityDataProviderToggle>();
			if (!isShowRelativityDataProviderToggleEnabled)
			{
				for (int i = 0; i < list.Count; i++)
				{
					if (list.ElementAt(i).value == DocumentTransferProvider.Shared.Constants.RELATIVITY_PROVIDER_GUID)
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
