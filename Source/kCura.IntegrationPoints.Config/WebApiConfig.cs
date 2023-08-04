using System;
using kCura.IntegrationPoints.Domain;
using Relativity.API;

namespace kCura.IntegrationPoints.Config
{
    public class WebApiConfig : IWebApiConfig
    {
        private readonly IHelper _helper;

        public WebApiConfig(IHelper helper)
        {
            _helper = helper;
        }

        public string WebApiUrl
        {
            get
            {
                Guid ripAppGuid = new Guid(Constants.IntegrationPoints.APPLICATION_GUID_STRING);
                Uri relativityUri = _helper.GetUrlHelper().GetApplicationURL(ripAppGuid);
                Uri webServiceUrl = new Uri($"{relativityUri.Scheme}://{relativityUri.Host}/Relativity.Rest/API/");
                return webServiceUrl.AbsoluteUri;
            }
        }
    }
}
