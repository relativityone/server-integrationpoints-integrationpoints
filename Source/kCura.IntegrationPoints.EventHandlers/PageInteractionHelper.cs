using System.Text.RegularExpressions;
using kCura.IntegrationPoints.Core;

namespace kCura.IntegrationPoints.EventHandlers
{
	internal static class PageInteractionHelper
	{
		/// <summary>
		///     This function will take the current request URL and get the path to an application's custom page
		/// </summary>
		/// <param name="currentUrl">The current http request url</param>
		/// <returns>The path of the application's custom page</returns>
		internal static string GetApplicationPath(string currentUrl)
		{
			string[] urlSplit = Regex.Split(currentUrl, "/Case/", RegexOptions.IgnoreCase);
			return $"{urlSplit[0]}/CustomPages/{Constants.IntegrationPoints.APPLICATION_GUID_STRING}";
		}
	}
}