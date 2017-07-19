using System.Runtime.InteropServices;
using kCura.EventHandler;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.EventHandlers.Commands;
using kCura.IntegrationPoints.EventHandlers.Commands.Factories;

namespace kCura.IntegrationPoints.EventHandlers.Installers
{
    [Guid("B8284448-09BC-4BCA-B81A-6FE154DA42D7")]
    [Description("Updates the Has Errors field on existing Integration Points.")]
    [RunOnce(true)]
    public class SetTypeOfExportDefaultValue : PostInstallEventHandler
    {
        public override Response Execute()
        {
            var eventHandlerCommandExecutor = new EventHandlerCommandExecutor(Helper.GetLoggerFactory().GetLogger());
            return eventHandlerCommandExecutor.Execute(SetTypeOfExportDefaultValueCommandFactory.Create(Helper, Helper.GetActiveCaseID()));
        }
    }
}
