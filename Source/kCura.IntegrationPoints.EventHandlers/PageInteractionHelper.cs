using System;

namespace kCura.IntegrationPoints.EventHandlers
{
	internal static class PageInteractionHelper
	{
		/// <summary>
		/// This function will take the current request URL and get the path to an application's custom page
		/// </summary>
		/// <param name="currentUrl">The current http request url</param>
		/// <returns>The path of the application's custom page</returns>
		internal static string GetApplicationPath(string currentUrl)
		{
			string applicationPath = null;

			string[] urlSplit = System.Text.RegularExpressions.Regex.Split(currentUrl, "/Case/", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			applicationPath = $"{urlSplit[0]}/CustomPages/{Core.Constants.IntegrationPoints.APPLICATION_GUID_STRING}";

			return applicationPath;
		}
	}
}
