using System;
using kCura.EventHandler;

// https://platform.kcura.com/9.3/Content/Customizing_workflows/Page_Interaction_event_handlers.htm
namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
	[System.Runtime.InteropServices.Guid("A65C496B-C885-4263-9E91-DC8308700DBA")]
	[kCura.EventHandler.CustomAttributes.Description("Removes ability to edit the Job History field on the Job History Error object")]
	public class JobHistoryErrorPageInteraction : kCura.EventHandler.PageInteractionEventHandler
	{
		public override Response PopulateScriptBlocks()
		{
			Response response = new Response { Success = true };

			String applicationPath = PageInteractionHelper.GetApplicationPath(this.Application.ApplicationUrl);
			this.RegisterLinkedClientScript(applicationPath + "/Scripts/EventHandlers/job-history.js");

			return response;
		}
	}
}
