using kCura.IntegrationPoints.Core;

namespace kCura.IntegrationPoints.EventHandlers
{
    internal static class PageInteractionHelper
    {
        /// <summary>
        /// Gets relative Uri of the application' custom page
        /// </summary>
        /// <returns>The path of the application's custom page</returns>
        internal static string GetApplicationRelativeUri()
        {
            return $"/Relativity/CustomPages/{Constants.IntegrationPoints.APPLICATION_GUID_STRING}";
        }
    }
}
