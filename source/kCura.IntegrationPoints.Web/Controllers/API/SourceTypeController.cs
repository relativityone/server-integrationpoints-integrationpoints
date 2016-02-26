using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Services.SourceTypes;
using kCura.IntegrationPoints.Web.Models;
using kCura.IntegrationPoints.Web.Toggles;
using Relativity.Toggles;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class SourceTypeController : ApiController
	{
		private readonly ISourceTypeFactory _factory;
		private readonly IToggleProvider _toggleProvider;

		public SourceTypeController(ISourceTypeFactory factory, IToggleProvider toggleProvider)
		{
			_factory = factory;
			_toggleProvider = toggleProvider;
		}

		[HttpGet]
		public HttpResponseMessage Get()
		{
			List<SourceTypeModel> list = _factory.GetSourceTypes().Select(x => new SourceTypeModel()
			{
				name = x.Name,
				id = x.ArtifactID,
				value = x.ID,
				url = x.SourceURL
			}).ToList();

			// TODO: Remove the toggle once the Relativity provider is ready
			bool isShowRelativityDataProviderToggleEnabled = _toggleProvider.IsEnabled<ShowRelativityDataProviderToggle>();
			if (!isShowRelativityDataProviderToggleEnabled)
			{
				for (int i = 0; i < list.Count; i++)
				{
					if (list.ElementAt(i).value ==
					    kCura.IntegrationPoints.DocumentTransferProvider.Shared.Constants.RELATIVITY_PROVIDER_GUID)
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
