using System.Web;
using kCura.IntegrationPoints.Core.Services;

namespace kCura.IntegrationPoints.Web
{
	public class RelativityUrlHelper : IRelativityUrlHelper
	{ 

		public const string VIEW_URL_TEMPLATE =
			"/{0}/Case/Mask/View.aspx?AppID={1}&ArtifactID={2}&ArtifactTypeID={3}";

		private readonly ObjectTypeService _service;
		public RelativityUrlHelper(ObjectTypeService service)
		{
			_service = service;
		}

		public string GetRelativityViewUrl(int workspaceID, int artifactID, string objectTypeName)
		{
			var applicationRoot = new HttpContextWrapper(System.Web.HttpContext.Current).Request.GetRootApplicationPath();

			var typeID = _service.GetObjectTypeID(objectTypeName);
			return string.Format(VIEW_URL_TEMPLATE,
				applicationRoot, workspaceID, artifactID, typeID);
		}


	}
}
