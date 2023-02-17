using System.Runtime.InteropServices;
using kCura.EventHandler;
using kCura.EventHandler.CustomAttributes;

// https://platform.kcura.com/9.3/Content/Customizing_workflows/Page_Interaction_event_handlers.htm

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
    [Guid("A65C496B-C885-4263-9E91-DC8308700DBA")]
    [Description("Removes ability to edit the Job History field on the Job History Error object")]
    public class JobHistoryErrorPageInteraction : EventHandler.PageInteractionEventHandler
    {
        public override Response PopulateScriptBlocks()
        {
            Response response = new Response {Success = true};

            string applicationPath = PageInteractionHelper.GetApplicationRelativeUri();
            RegisterLinkedClientScript(applicationPath + "/Scripts/EventHandlers/job-history.js");

            return response;
        }
    }
}
