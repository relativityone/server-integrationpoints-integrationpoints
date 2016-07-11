using System;
using System.Collections.Generic;
using System.Text;
using kCura.EventHandler;

//http://platform.kcura.com/9.0/index.htm#Customizing_workflows/Page_Interaction_event_handlers.htm?Highlight=javascript
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
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

			String applicationPath = PageInteractionHelper.GetApplicationPath(this.Application.ApplicationUrl);

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

			    this.RegisterLinkedClientScript(applicationPath + "/Scripts/jquery.signalR-2.2.0.js");
			    this.RegisterLinkedClientScript(applicationPath + "/signalr/hubs");
			    this.RegisterLinkedClientScript(applicationPath + "/Scripts/hubs/integrationPointHub.js");



                this.RegisterClientScriptBlock(new ScriptBlock { Key = "PageURL234324324", Script = "<script>var IP = IP ||{};IP.cpPath = '" + applicationPath + "';</script>" });
				int nextScheduledRuntimeFieldId = base.GetArtifactIdByGuid(Guid.Parse(Data.IntegrationPointFieldGuids.NextScheduledRuntimeUTC));
				int destinationFieldId = base.GetArtifactIdByGuid(Guid.Parse(Data.IntegrationPointFieldGuids.DestinationConfiguration));
				int destinationProviderFieldId = base.GetArtifactIdByGuid(Guid.Parse(Data.IntegrationPointFieldGuids.DestinationProvider));
				int sourceProviderFieldId = base.GetArtifactIdByGuid(Guid.Parse(Data.IntegrationPointFieldGuids.SourceProvider));
				int lastRuntimeFieldId = base.GetArtifactIdByGuid(Guid.Parse(Data.IntegrationPointFieldGuids.LastRuntimeUTC));
				int sourceConfigurationFieldId = base.GetArtifactIdByGuid(Guid.Parse(Data.IntegrationPointFieldGuids.SourceConfiguration));

				int overwriteFieldsId = base.GetArtifactIdByGuid(Guid.Parse(Data.IntegrationPointFieldGuids.OverwriteFields));
				int hasErrorsId = base.GetArtifactIdByGuid(Guid.Parse(Data.IntegrationPointFieldGuids.HasErrors));
				int logErrorsId = base.GetArtifactIdByGuid(Guid.Parse(Data.IntegrationPointFieldGuids.LogErrors));
				int nameId = base.GetArtifactIdByGuid(Guid.Parse(Data.IntegrationPointFieldGuids.Name));
				int emailNotificationId = base.GetArtifactIdByGuid(Guid.Parse(Data.IntegrationPointFieldGuids.EmailNotificationRecipients));

				this.RegisterClientScriptBlock(new ScriptBlock { Key = "PageURL2343243453", Script = "<script>var IP = IP ||{};IP.nextTimeid= ['" + nextScheduledRuntimeFieldId + "', '" + lastRuntimeFieldId + "'] ;</script>" });
				this.RegisterClientScriptBlock(new ScriptBlock { Key = Guid.NewGuid().ToString(), Script = "<script>var IP = IP ||{}; IP.destinationid= '" + destinationFieldId + "';</script>" });
				this.RegisterClientScriptBlock(new ScriptBlock { Key = Guid.NewGuid().ToString(), Script = "<script>var IP = IP ||{}; IP.destinationProviderid= '" + destinationProviderFieldId + "';</script>" });
				this.RegisterClientScriptBlock(new ScriptBlock { Key = Guid.NewGuid().ToString(), Script = "<script>var IP = IP ||{}; IP.sourceProviderId= '" + sourceProviderFieldId + "';</script>" });
				this.RegisterClientScriptBlock(new ScriptBlock { Key = Guid.NewGuid().ToString(), Script = "<script>var IP = IP ||{}; IP.artifactid= '" + base.ActiveArtifact.ArtifactID + "';</script>" });
				this.RegisterClientScriptBlock(new ScriptBlock { Key = Guid.NewGuid().ToString(), Script = "<script>var IP = IP ||{}; IP.appid= '" + base.Application.ArtifactID + "';</script>" });
				this.RegisterClientScriptBlock(new ScriptBlock { Key = Guid.NewGuid().ToString(), Script = "<script>var IP = IP ||{}; IP.sourceConfiguration= '" + sourceConfigurationFieldId + "';</script>" });

				this.RegisterClientScriptBlock(new ScriptBlock { Key = Guid.NewGuid().ToString(), Script = "<script>var IP = IP ||{}; IP.overwriteFieldsId= '" + overwriteFieldsId + "';</script>" });
				this.RegisterClientScriptBlock(new ScriptBlock { Key = Guid.NewGuid().ToString(), Script = "<script>var IP = IP ||{}; IP.hasErrorsId= '" + hasErrorsId + "';</script>" });
				this.RegisterClientScriptBlock(new ScriptBlock { Key = Guid.NewGuid().ToString(), Script = "<script>var IP = IP ||{}; IP.logErrorsId= '" + logErrorsId + "';</script>" });
				this.RegisterClientScriptBlock(new ScriptBlock { Key = Guid.NewGuid().ToString(), Script = "<script>var IP = IP ||{}; IP.nameId= '" + nameId + "';</script>" });
				this.RegisterClientScriptBlock(new ScriptBlock { Key = Guid.NewGuid().ToString(), Script = "<script>var IP = IP ||{}; IP.emailNotificationId= '" + emailNotificationId + "';</script>" });

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
				
				int sourceProvider = (int)this.ActiveArtifact.Fields[IntegrationPointFields.SourceProvider].Value.Value;
				this.RegisterLinkedClientScript(applicationPath + "/Scripts/EventHandlers/integration-points-view-destination.js");

				if (ServiceContext.RsapiService.SourceProviderLibrary.Read(Int32.Parse(sourceProvider.ToString())).Name == Core.Constants.IntegrationPoints.RELATIVITY_PROVIDER_NAME) 
				{
					this.RegisterLinkedClientScript(applicationPath + "/Scripts/EventHandlers/relativity-provider-view.js");

					int destinationProvider = (int)this.ActiveArtifact.Fields[IntegrationPointFields.DestinationProvider].Value.Value;
					if (ServiceContext.RsapiService.DestinationProviderLibrary.Read(Int32.Parse(destinationProvider.ToString())).Name == Core.Constants.IntegrationPoints.FILESHARE_PROVIDER_NAME)
					{
						this.RegisterLinkedClientScript(applicationPath + "/Scripts/EventHandlers/export-details-view.js");


					}
				}
				else
				{
					this.RegisterLinkedClientScript(applicationPath + "/Scripts/EventHandlers/integration-points-view.js");
				}
				
				

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
		
		public override FieldCollection RequiredFields
		{
			get
			{
				var fieldCollection = new FieldCollection {new Field(IntegrationPointFields.DestinationProvider)};
				return fieldCollection;
			}
		}
	}
}
