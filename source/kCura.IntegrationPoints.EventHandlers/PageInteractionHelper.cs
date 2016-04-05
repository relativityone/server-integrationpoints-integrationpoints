namespace kCura.IntegrationPoints.EventHandlers
{
	internal static class PageInteractionHelper
	{
		/// <summary>
		/// This function will take the current request URL and get the path to a custom page application so JavaScript and CSS files can be referenced
		/// </summary>
		/// <param name="currentUrl">The current http request url</param>
		/// <returns>Returns the path to the custom page application</returns>
		internal static string GetApplicationPath(string currentUrl)
		{
			string applicationPath = null;

			string[] urlSplit = System.Text.RegularExpressions.Regex.Split(currentUrl, "/Case/", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			applicationPath = urlSplit[0] + string.Format("/CustomPages/{0}", Core.Application.GUID);

			return applicationPath;
		}
	}
}
