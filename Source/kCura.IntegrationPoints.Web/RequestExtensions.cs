using System.Linq;
using System.Web;

namespace kCura.IntegrationPoints.Web
{
	public static class RequestExtensions
	{
		public static string GetRootApplicationPath(this HttpRequestBase request)
		{
			var appPath = request.ApplicationPath;
			var result = "/Relativity";
			if (appPath != null)
			{
				result = appPath.Split('/').First(x => !string.IsNullOrEmpty(x.Trim()));
			}
			return result;
		}

		public static string GetApplicationPath(this HttpRequestBase request)
		{
			return string.Format("{0}{1}", GetRootURL(request), request.ApplicationPath);
		}

		public static string GetRootURL(this HttpRequestBase request)
		{
			return string.Format("{0}://{1}{2}",
						request.Url.Scheme,
						request.Url.Host,
						request.Url.Port == 80 ? string.Empty : ":" + HttpContext.Current.Request.Url.Port);
		}

	}
}