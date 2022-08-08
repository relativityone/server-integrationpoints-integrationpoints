using System;
using System.Runtime.InteropServices;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.EventHandlers.Commands;
using kCura.IntegrationPoints.EventHandlers.Commands.Context;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.Installers
{
    [RunOnce(true)]
    [Description("Sets ImportNativeFile flag depending on source and destination configuration")]
    [Guid("8DA02F00-480C-437B-BACF-9839CE83042D")]
    public class SetImportNativeFileCopyMode : PostInstallEventHandlerBase, IEventHandler
    {
        public IEHContext Context => new EHContext
        {
            Helper = Helper
        };

        public Type CommandType => typeof(SetImportNativeFileCopyModeCommand);

        protected override IAPILog CreateLogger()
        {
            return Helper.GetLoggerFactory().GetLogger().ForContext<SetImportNativeFileCopyMode>();
        }

        protected override string SuccessMessage => "ImportNativeFilesCopyMode flag set successfuly for all integration points and integration point profiles.";

        protected override string GetFailureMessage(Exception ex)
        {
            return "Adding ImportNativesFilesCopyMode flag into integration point configuration setting failed";
        }

        protected override void Run()
        {
            var executor = new EventHandlerExecutor();
            executor.Execute(this);
        }
    }
}
