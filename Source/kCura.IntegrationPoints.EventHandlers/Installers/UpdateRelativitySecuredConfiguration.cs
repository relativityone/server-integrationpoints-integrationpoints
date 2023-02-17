using System;
using System.Runtime.InteropServices;
using Relativity.API;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.EventHandlers.Commands;
using kCura.IntegrationPoints.EventHandlers.Commands.Context;

namespace kCura.IntegrationPoints.EventHandlers.Installers
{
    /// <summary>
    /// Removes Secured Configuration from source and destination fields.
    /// </summary>
    [Description("Update Relativity Configuration to use only Secret Store")]
    [RunOnce(true)]
    [Guid("2DAD0486-E3E1-4A3C-956D-FA45AB5E4717")]
    public class UpdateRelativitySecuredConfiguration : PostInstallEventHandlerBase, IEventHandler
    {
        protected override string SuccessMessage => "Updating Relativity Configuration completed";

        protected override string GetFailureMessage(Exception ex)
        {
            return "Updating Relativity Configuration failed";
        }

        public IEHContext Context => new EHContext
        {
            Helper = Helper
        };

        public Type CommandType => typeof(UpdateRelativityConfigurationCommand);

        protected override IAPILog CreateLogger()
        {
            return Helper.GetLoggerFactory().GetLogger().ForContext<UpdateRelativitySecuredConfiguration>();
        }

        protected override void Run()
        {
            var executor = new EventHandlerExecutor();
            executor.Execute(this);
        }
    }
}
