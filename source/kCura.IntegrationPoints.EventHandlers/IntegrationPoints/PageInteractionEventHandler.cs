using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Components.DictionaryAdapter.Xml;
using kCura.EventHandler;

//http://platform.kcura.com/9.0/index.htm#Customizing_workflows/Page_Interaction_event_handlers.htm?Highlight=javascript
namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
	[System.Runtime.InteropServices.Guid("d62ec71f-f8c1-4344-aabb-b23e376d93df")]
	[kCura.EventHandler.CustomAttributes.Description("This is a details page interaction event handler")]
	public class PageInteractionEventHandler : kCura.EventHandler.PageInteractionEventHandler
	{
		public override Response PopulateScriptBlocks()
		{
			var response = new Response();
			response.Success = true;
			response.Message = string.Empty;

			String applicationPath = GetApplicationPath(this.Application.ApplicationUrl);

			if (base.PageMode == EventHandler.Helper.PageMode.View)
			{
				this.RegisterLinkedClientScript(applicationPath + "/Scripts/route.js");
				this.RegisterLinkedCss(applicationPath + "/Content/jquery.jqGrid/ui.jqgrid.css");
				this.RegisterLinkedCss(applicationPath + "/Content/integration-points-fonts.css");
				this.RegisterLinkedCss(applicationPath + "/Content/legal-hold-fonts.css");
				this.RegisterLinkedCss(applicationPath + "/Content/themes/base/jquery.ui.dialog.css");
					this.RegisterLinkedCss(applicationPath + "/Content/integration-points-view.css");
				this.RegisterLinkedCss(applicationPath + "/Content/Site.css");
				this.RegisterLinkedCss(applicationPath + "/Content/controls.grid.css");
				this.RegisterLinkedCss(applicationPath + "/Content/controls-grid-pager.css");
				this.RegisterLinkedClientScript(applicationPath + "/Scripts/date.js");
				this.RegisterLinkedClientScript(applicationPath + "/Scripts/q.js");
				this.RegisterLinkedClientScript(applicationPath + "/Scripts/core/messaging.js");
				this.RegisterLinkedClientScript(applicationPath + "/Scripts/loading-modal.js");
				this.RegisterLinkedClientScript(applicationPath + "/Scripts/dragon/dragon-dialogs.js");
				this.RegisterLinkedClientScript(applicationPath + "/Scripts/core/data.js");
				this.RegisterLinkedClientScript(applicationPath + "/Scripts/core/utils.js");
				this.RegisterLinkedClientScript(applicationPath + "/Scripts/integration-point/time-utils.js");

				this.RegisterClientScriptBlock(new ScriptBlock { Key = "PageURL234324324", Script = "<script>var IP = IP ||{};IP.cpPath = '" + applicationPath + "';</script>" });
				var fieldID = base.GetArtifactIdByGuid(Guid.Parse(Data.IntegrationPointFieldGuids.NextScheduledRuntimeUTC));
				var destinationFieldID = base.GetArtifactIdByGuid(Guid.Parse(Data.IntegrationPointFieldGuids.DestinationConfiguration));
				var destinationProviderFieldID = base.GetArtifactIdByGuid(Guid.Parse(Data.IntegrationPointFieldGuids.DestinationProvider));
				var sourceProviderFieldID = base.GetArtifactIdByGuid(Guid.Parse(Data.IntegrationPointFieldGuids.DestinationProvider));


				var lastTimefieldID = base.GetArtifactIdByGuid(Guid.Parse(Data.IntegrationPointFieldGuids.LastRuntimeUTC));

				this.RegisterClientScriptBlock(new ScriptBlock { Key = "PageURL2343243453", Script = "<script>var IP = IP ||{};IP.nextTimeid= ['" + fieldID + "', '" + lastTimefieldID + "'] ;</script>" });
				this.RegisterClientScriptBlock(new ScriptBlock { Key = Guid.NewGuid().ToString(), Script = "<script>var IP = IP ||{}; IP.destinationid= '" + destinationFieldID + "';</script>" });
				this.RegisterClientScriptBlock(new ScriptBlock { Key = Guid.NewGuid().ToString(), Script = "<script>var IP = IP ||{}; IP.destinationProviderid= '" + destinationProviderFieldID + "';</script>" });
				this.RegisterClientScriptBlock(new ScriptBlock { Key = Guid.NewGuid().ToString(), Script = "<script>var IP = IP ||{}; IP.sourceProviderId= '" + sourceProviderFieldID + "';</script>" });
				this.RegisterClientScriptBlock(new ScriptBlock { Key = Guid.NewGuid().ToString(), Script = "<script>var IP = IP ||{}; IP.artifactid= '" + base.ActiveArtifact.ArtifactID + "';</script>" });
				this.RegisterClientScriptBlock(new ScriptBlock { Key = Guid.NewGuid().ToString(), Script = "<script>var IP = IP ||{}; IP.appid= '" + base.Application.ArtifactID + "';</script>" });
				


				this.RegisterLinkedClientScript(applicationPath + "/Scripts/EventHandlers/integration-points-grid.js");
			
				this.RegisterLinkedClientScript(applicationPath + "/Scripts/EventHandlers/integration-points-view.js");
				this.RegisterLinkedClientScript(applicationPath + "/Scripts/EventHandlers/integration-points-view-destination.js");
				this.RegisterLinkedClientScript(applicationPath + "/Scripts/i18n/grid.locale-en.js");
				this.RegisterLinkedClientScript(applicationPath + "/Scripts/jquery.jqGrid.min.js");
				this.RegisterLinkedClientScript(applicationPath + "/Scripts/select2.min.js");
				this.RegisterLinkedClientScript(applicationPath + "/Scripts/grid/dragon-grid.js");
				this.RegisterLinkedClientScript(applicationPath + "/Scripts/grid/dragon-grid-pager.js");
				this.RegisterLinkedClientScript(applicationPath + "/Scripts/grid/dragon-utils.js");


				
			}
			

			return response;
		}

		/// <summary>
		/// This function will take the current request URL and get the path to a custom page application so JavaScript and CSS files can be referenced
		/// </summary>
		/// <param name="currentUrl">The current http request url</param>
		/// <returns>Returns the path to the custom page application</returns>
		private string GetApplicationPath(string currentUrl)
		{
			string retVal = null;

			string[] urlSplit = System.Text.RegularExpressions.Regex.Split(currentUrl, "/Case/", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			retVal = urlSplit[0] + string.Format("/CustomPages/{0}", Core.Application.GUID);
			return retVal;

		}

	}
}
