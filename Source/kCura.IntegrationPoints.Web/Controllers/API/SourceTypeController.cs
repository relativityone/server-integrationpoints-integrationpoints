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
using kCura.IntegrationPoints.ImportProvider;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class SourceTypeController : ApiController
	{
		private readonly ISourceTypeFactory _factory;
		private readonly ICaseServiceContext _serviceContext;
		private readonly IObjectTypeQuery _rdoQuery;
        private readonly IToggleProvider _toggleProvider;

        public SourceTypeController(ISourceTypeFactory factory,
            IToggleProvider toggleProvider,
			ICaseServiceContext serviceContext,
			RSAPIRdoQuery objectTypeQuery)
		{
			_factory = factory;
			_rdoQuery = objectTypeQuery;
			_serviceContext = serviceContext;
            _toggleProvider = toggleProvider;
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
				Config = new SourceProviderConfigModel(x.Config, rdoTypesCache)
			}).ToList();

            // TODO: Remove the toggle once the Import provider is ready
            bool isShowImportProviderToggleEnabled = _toggleProvider.IsEnabled<ShowFileShareImportProviderToggle>();//TODO toggle
            if (!isShowImportProviderToggleEnabled)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (list.ElementAt(i).value.ToUpper() == "548F0873-8E5E-4DA6-9F27-5F9CDA764636")//TODO GUID Import provider
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
