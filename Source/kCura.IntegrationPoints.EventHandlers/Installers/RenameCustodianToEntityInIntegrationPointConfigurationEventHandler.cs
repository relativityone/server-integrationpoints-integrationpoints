using System;
using System.Runtime.InteropServices;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.EventHandlers.Commands;
using kCura.IntegrationPoints.EventHandlers.Commands.Context;
using kCura.IntegrationPoints.EventHandlers.Commands.RenameCustodianToEntity;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.Installers
{
    [Description("Rename Custodian to Entity in Integration Points Configuration")]
    [RunOnce(true)]
    [Guid("f21845a5-f061-4bea-b653-85bca1f33286")]
    public class RenameCustodianToEntityInIntegrationPointConfigurationEventHandler : PostInstallEventHandlerBase, IEventHandler
    {
        protected override string SuccessMessage => "Custodians successfully renamed to Entities in Integration Points configuration.";
        protected override string GetFailureMessage(Exception ex)
        {
            return "Unable to rename Custodians to Entities in Integration Points configuration.";
        }

        protected override void Run()
        {
            var executor = new EventHandlerExecutor();
            executor.Execute(this);
        }

        protected override IAPILog CreateLogger()
        {
            return Helper.GetLoggerFactory().GetLogger().ForContext<RenameCustodianToEntityInIntegrationPointConfigurationEventHandler>();
        }

        public IEHContext Context => new EHContext
        {
            Helper = Helper
        };

        public Type CommandType => typeof(RenameCustodianToEntityInIntegrationPointConfigurationCommand);
    }
}
