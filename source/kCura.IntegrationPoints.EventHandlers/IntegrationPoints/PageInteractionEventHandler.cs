using System;
using System.Collections.Generic;
using System.Text;
using kCura.EventHandler;

//http://platform.kcura.com/9.0/index.htm#Customizing_workflows/Page_Interaction_event_handlers.htm?Highlight=javascript
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data.Extensions;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
	[System.Runtime.InteropServices.Guid("d62ec71f-f8c1-4344-aabb-b23e376d93df")]
	[kCura.EventHandler.CustomAttributes.Description("This is a details page interaction event handler")]
	public class PageInteractionEventHandler : kCura.EventHandler.PageInteractionEventHandler
	{
		private ICaseServiceContext _context;
		public ICaseServiceContext ServiceContext
		{
			get
			{
				return _context ?? (_context = ServiceContextFactory.CreateCaseServiceContext(base.Helper, this.Application.ArtifactID));
			}
			set { _context = value; }
		}

		public override Response PopulateScriptBlocks()
		{
			Response response = new Response
			{
				Success = true,
				Message = string.Empty
			};

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
				int nextScheduledRuntimeFieldId = base.GetArtifactIdByGuid(Guid.Parse(Data.IntegrationPointFieldGuids.NextScheduledRuntimeUTC));
				int destinationFieldId = base.GetArtifactIdByGuid(Guid.Parse(Data.IntegrationPointFieldGuids.DestinationConfiguration));
				int destinationProviderFieldId = base.GetArtifactIdByGuid(Guid.Parse(Data.IntegrationPointFieldGuids.DestinationProvider));
				int sourceProviderFieldId = base.GetArtifactIdByGuid(Guid.Parse(Data.IntegrationPointFieldGuids.SourceProvider));
				int lastRuntimeFieldId = base.GetArtifactIdByGuid(Guid.Parse(Data.IntegrationPointFieldGuids.LastRuntimeUTC));
				int sourceConfigurationFieldId = base.GetArtifactIdByGuid(Guid.Parse(Data.IntegrationPointFieldGuids.SourceConfiguration));

				this.RegisterClientScriptBlock(new ScriptBlock { Key = "PageURL2343243453", Script = "<script>var IP = IP ||{};IP.nextTimeid= ['" + nextScheduledRuntimeFieldId + "', '" + lastRuntimeFieldId + "'] ;</script>" });
				this.RegisterClientScriptBlock(new ScriptBlock { Key = Guid.NewGuid().ToString(), Script = "<script>var IP = IP ||{}; IP.destinationid= '" + destinationFieldId + "';</script>" });
				this.RegisterClientScriptBlock(new ScriptBlock { Key = Guid.NewGuid().ToString(), Script = "<script>var IP = IP ||{}; IP.destinationProviderid= '" + destinationProviderFieldId + "';</script>" });
				this.RegisterClientScriptBlock(new ScriptBlock { Key = Guid.NewGuid().ToString(), Script = "<script>var IP = IP ||{}; IP.sourceProviderId= '" + sourceProviderFieldId + "';</script>" });
				this.RegisterClientScriptBlock(new ScriptBlock { Key = Guid.NewGuid().ToString(), Script = "<script>var IP = IP ||{}; IP.artifactid= '" + base.ActiveArtifact.ArtifactID + "';</script>" });
				this.RegisterClientScriptBlock(new ScriptBlock { Key = Guid.NewGuid().ToString(), Script = "<script>var IP = IP ||{}; IP.appid= '" + base.Application.ArtifactID + "';</script>" });
				this.RegisterClientScriptBlock(new ScriptBlock { Key = Guid.NewGuid().ToString(), Script = "<script>var IP = IP ||{}; IP.sourceConfiguration= '" + sourceConfigurationFieldId + "';</script>" });
				this.RegisterClientScriptBlock(new kCura.EventHandler.ScriptBlock() { Key = "refreshFunc", Script = "<script type=\"text/javascript\"> function refreshList(){ $('.associative-list').load(document.URL +  ' .associative-list'); setTimeout(refreshList, 5000);};</script>" });

				StringBuilder script = new StringBuilder();
				script.Append("<script>");
				script.Append("var IP = IP || {};");
				script.Append("IP.params = IP.params || {};");
				foreach (KeyValuePair<string,object> keyValuePair in GetParams())
				{
					script.AppendFormat("IP.params['{0}'] = '{1}';", keyValuePair.Key, keyValuePair.Value);
				}
				script.Append("</script>");

				this.RegisterClientScriptBlock(new ScriptBlock { Key = Guid.NewGuid().ToString(), Script = script.ToString() });

				this.RegisterLinkedClientScript(applicationPath + "/Scripts/EventHandlers/integration-points-view.js");
				this.RegisterLinkedClientScript(applicationPath + "/Scripts/EventHandlers/integration-points-view-destination.js");

				//this.RegisterLinkedClientScript(applicationPath + "/Scripts/EventHandlers/integration-points-grid.js");
				//this.RegisterLinkedClientScript(applicationPath + "/Scripts/i18n/grid.locale-en.js");
				//this.RegisterLinkedClientScript(applicationPath + "/Scripts/jquery.jqGrid.min.js");
				//this.RegisterLinkedClientScript(applicationPath + "/Scripts/select2.min.js");
				//this.RegisterLinkedClientScript(applicationPath + "/Scripts/grid/dragon-grid.js");
				//this.RegisterLinkedClientScript(applicationPath + "/Scripts/grid/dragon-grid-pager.js");
				//this.RegisterLinkedClientScript(applicationPath + "/Scripts/grid/dragon-utils.js");
				
				this.RegisterStartupScriptBlock(new kCura.EventHandler.ScriptBlock() { Key = "refreshKey", Script = "<script type=\"text/javascript\"> refreshList();</script>" });
			}
			return response;
		}


		public IEnumerable<KeyValuePair<string, object>> GetParams()
		{
			yield return new KeyValuePair<string, object>("scheduleRuleId", base.GetArtifactIdByGuid(Guid.Parse(Data.IntegrationPointFieldGuids.ScheduleRule)));
			yield return new KeyValuePair<string, object>("sourceUrl", GetSourceViewUrl());
		}

		public virtual string GetSourceViewUrl()
		{
			var ip = this.ServiceContext.RsapiService.IntegrationPointLibrary.Read(this.ActiveArtifact.ArtifactID,
				Guid.Parse(Data.IntegrationPointFieldGuids.SourceProvider));
			if (!ip.SourceProvider.HasValue)
			{
				throw new ArgumentException(string.Format("Source provider for integration point: {0} is not valid.", this.ActiveArtifact.ArtifactID));
			}
			var provider = this.ServiceContext.RsapiService.SourceProviderLibrary.Read(ip.SourceProvider.Value, Guid.Parse(Data.SourceProviderFieldGuids.ViewConfigurationUrl));
			return provider.ViewConfigurationUrl;
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
		
		public override FieldCollection RequiredFields
		{
			get { return new FieldCollection(); }
		}
	}
}
