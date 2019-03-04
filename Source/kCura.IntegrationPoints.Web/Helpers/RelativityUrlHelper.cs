using kCura.IntegrationPoints.Core.Services;
using System.Web;

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
			string applicationRoot = _httpRequest.GetRootApplicationPath();
			int typeID = _service.GetObjectTypeID(objectTypeName);

			return string.Format(
				VIEW_URL_TEMPLATE,
				applicationRoot,
				workspaceID,
				artifactID,
				typeID
			);
		}
	}
}
