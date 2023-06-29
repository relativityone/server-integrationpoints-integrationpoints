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
    [Guid("8763DB48-7B5E-4D2C-9A17-532A03A88C8C")]
    public class UpdateTaggingInDestinationConfiguration : PostInstallEventHandlerBase, IEventHandler
    {
        public IEHContext Context => new EHContext
        {
            Helper = Helper
        };

        public Type CommandType => typeof(UpdateDestinationConfigurationTaggingSettingsCommand);

        protected override IAPILog CreateLogger()
        {
            return Helper.GetLoggerFactory().GetLogger().ForContext<UpdateTaggingInDestinationConfiguration>();
        }

        protected override string SuccessMessage => "Tagging option configuration updated successfuly for all integration points and integration point profiles.";

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
