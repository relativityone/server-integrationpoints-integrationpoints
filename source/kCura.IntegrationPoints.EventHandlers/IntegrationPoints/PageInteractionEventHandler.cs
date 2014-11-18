using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.EventHandler;
//http://platform.kcura.com/9.0/index.htm#Customizing_workflows/Page_Interaction_event_handlers.htm?Highlight=javascript
namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
	[System.Runtime.InteropServices.Guid("d62ec71f-f8c1-4344-aabb-b23e376d93df")]
	[kCura.EventHandler.CustomAttributes.Description("This is a sample page interaction event handler")]
	public class PageInteractionEventHandler : kCura.EventHandler.PageInteractionEventHandler
	{
		public override Response PopulateScriptBlocks()
		{
			var appPath = GetApplicationPath(this.Application.ApplicationUrl);
			this.RegisterLinkedClientScript(appPath + "Scripts/EventHandlers/test.js");

			var response = new Response();
			response.Success = true;
			response.Message = string.Empty;
			return response;
		}

		/// <summary>
		/// This function will take the current request URL and get the path to a custom page application so JavaScript and CSS files can be referenced
		/// </summary>
		/// <param name="currentURL">The current http request url</param>
		/// <returns>Returns the path to the custom page application</returns>
		private string GetApplicationPath(string currentURL)
		{
			string retVal = null;

			string[] urlSplit = System.Text.RegularExpressions.Regex.Split(currentURL, "/Case/", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			retVal = urlSplit[0] + string.Format("/CustomPages/{0}/", Core.Application.GUID);
			return retVal;
		}

	}
}
