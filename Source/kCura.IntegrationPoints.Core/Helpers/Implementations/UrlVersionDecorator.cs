using System.Reflection;
using System.Diagnostics;

namespace kCura.IntegrationPoints.Core.Helpers.Implementations
{
    public static class UrlVersionDecorator
    {
        private static readonly string Version;

        static UrlVersionDecorator()
        {
            Version = FileVersionInfo.GetVersionInfo(Assembly.GetAssembly(typeof(UrlVersionDecorator)).Location).FileVersion;
        }

        public static string AppendVersion(string url)
        {
            bool alreadyContainsParameters = url.Contains("?") && url.Contains("=");

            string parameterCharacter = alreadyContainsParameters ? "&" : "?";

            return $"{url}{parameterCharacter}v={Version}";
        }
    }
}
