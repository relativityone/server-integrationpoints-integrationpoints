using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Services.ServiceContext;

namespace kCura.IntegrationPoints.Web
{
	public class RelativityUrlHelper
	{
		public const string VIEW_URL_TEMPLATE =
			"{0}/Case/Mask/View.aspx?AppID=1025258&ArtifactID=1037537&ArtifactTypeID=1000028";

		public string GetRelativityViewUrl(int artifactID, int artifactTypeID)
		{
			var applicationRoot = new HttpContextWrapper(System.Web.HttpContext.Current).Request.GetRootApplicationPath();

			return string.Format(VIEW_URL_TEMPLATE,
				applicationRoot);
		}


	}
}
