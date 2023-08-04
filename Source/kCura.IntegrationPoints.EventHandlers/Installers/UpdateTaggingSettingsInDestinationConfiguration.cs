using System;
using System.Runtime.InteropServices;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.EventHandlers.Commands;
using kCura.IntegrationPoints.EventHandlers.Commands.Context;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.Installers
{
    [RunOnce(true)]
    [Description("Update tagging settings in destination configuration for integration point and integration point profile")]
    [Guid("5E0E6953-42AF-41A5-B6E5-222643541F1E")]
    public class UpdateTaggingSettingsInDestinationConfiguration : PostInstallEventHandlerBase, IEventHandler
    {
        public IEHContext Context => new EHContext
        {
            Helper = Helper
        };

        public Type CommandType => typeof(UpdateDestinationConfigurationTaggingSettingsCommand);

        protected override IAPILog CreateLogger()
        {
            return Helper.GetLoggerFactory().GetLogger().ForContext<UpdateTaggingSettingsInDestinationConfiguration>();
        }

        protected override string SuccessMessage => "Tagging option configuration updated successfully for all integration points and integration point profiles.";

        protected override string GetFailureMessage(Exception ex)
        {
            return "Tagging option configuration update in integration point and integration point profile configuration setting failed";
        }

        protected override void Run()
        {
            var executor = new EventHandlerExecutor();
            executor.Execute(this);
        }
    }
}
