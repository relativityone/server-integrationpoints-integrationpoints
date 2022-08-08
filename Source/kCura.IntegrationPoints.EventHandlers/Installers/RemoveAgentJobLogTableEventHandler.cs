using System;
using System.Runtime.InteropServices;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.EventHandlers.Commands;
using kCura.IntegrationPoints.EventHandlers.Commands.Context;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.Installers
{
    [Description("Removes AgentJobLog_08C0CE2D-8191-4E8F-B037-899CEAEE493D table")]
    [RunOnce(true)]
    [Guid("BE794C48-FB6E-4279-9483-D4D602278153")]
    public class RemoveAgentJobLogTableEventHandler : PostInstallEventHandlerBase, IEventHandler
    {
        public IEHContext Context => new EHContext
        {
            Helper = Helper
        };

        public Type CommandType => typeof(RemoveAgentJobLogTableCommand);

        protected override IAPILog CreateLogger()
        {
            return Helper.GetLoggerFactory().GetLogger().ForContext<RemoveAgentJobLogTableEventHandler>();
        }

        protected override string SuccessMessage => "Agent Job Log table succesfully deleted.";

        protected override string GetFailureMessage(Exception ex)
        {
            return $"Could not delete AgentJobLog_{ GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID} table";
        }

        protected override void Run()
        {
            var executor = new EventHandlerExecutor();
            executor.Execute(this);
        }
    }
}
