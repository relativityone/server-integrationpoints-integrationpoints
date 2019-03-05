using System.Linq;
using System.Web;

namespace kCura.IntegrationPoints.Web.Extensions
{
	public static class RequestExtensions
	{
		private const string _DEFAULT_APPLICATION_PATH = "/Relativity";

		public static string GetRootApplicationPath(this HttpRequestBase request)
		{
			string applicationPath = request.ApplicationPath;
			return applicationPath != null
				? GetFirstApplicationPath(applicationPath)
				: _DEFAULT_APPLICATION_PATH;
		}

		private static string GetFirstApplicationPath(string applicationPath)
		{
			return applicationPath
				.Split('/')
				.First(x => !string.IsNullOrWhiteSpace(x));
		}
	}
}