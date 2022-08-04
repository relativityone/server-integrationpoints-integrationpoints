using System.Reflection;
using System.Resources;
using kCura.IntegrationPoints.Core.Services;
using System.Web;
using kCura.IntegrationPoints.Core.Helpers.Implementations;
using kCura.IntegrationPoints.Web.Extensions;

namespace kCura.IntegrationPoints.Web.Helpers
{
    public class RelativityUrlHelper : IRelativityUrlHelper
    {
        private readonly HttpRequestBase _httpRequest;
        private readonly ObjectTypeService _service;

        public const string VIEW_URL_TEMPLATE =
            "/{0}/Case/Mask/View.aspx?AppID={1}&ArtifactID={2}&ArtifactTypeID={3}";

        public RelativityUrlHelper(HttpRequestBase httpRequest, ObjectTypeService service)
        {
            _httpRequest = httpRequest;
            _service = service;
        }

        public string GetRelativityViewUrl(int workspaceID, int artifactID, string objectTypeName)
        {
            string applicationRoot = _httpRequest.GetApplicationRootPath();
            int typeID = _service.GetObjectTypeID(objectTypeName);

            return UrlVersionDecorator.AppendVersion(string.Format(
                VIEW_URL_TEMPLATE,
                applicationRoot,
                workspaceID,
                artifactID,
                typeID
            ));
        }
    }
}
