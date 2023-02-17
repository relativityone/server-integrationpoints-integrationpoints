using System.Linq;
using System.Web;

namespace kCura.IntegrationPoints.Web.Extensions
{
    public static class RequestExtensions
    {
        private const string _DEFAULT_APPLICATION_PATH = "/Relativity";

        public static string GetApplicationRootPath(this HttpRequestBase request)
        {
            string applicationPath = request.ApplicationPath;
            return GetFirstApplicationPath(applicationPath) ?? _DEFAULT_APPLICATION_PATH;
        }

        private static string GetFirstApplicationPath(string applicationPath)
        {
            return applicationPath
                ?.Split('/')
                ?.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
        }
    }
}
