using System;
using System.Runtime.InteropServices;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.EventHandlers.Commands;
using kCura.IntegrationPoints.EventHandlers.Commands.Factories;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.Installers
{
    [Guid("B8284448-09BC-4BCA-B81A-6FE154DA42D7")]
    [Description("Updates the Has Errors field on existing Integration Points.")]
    [RunOnce(true)]
    public class SetTypeOfExportDefaultValue : PostInstallEventHandlerBase
    {
        protected override IAPILog CreateLogger()
        {
            return Helper.GetLoggerFactory().GetLogger().ForContext<SetTypeOfExportDefaultValue>();
        }

        protected override string SuccessMessage => 
            "'TypeOfExport' setting has been updated successfully in Relativity provider 'SourceConfiguration' column (IntegrationPoint/IntegrationPointProfile tables)";

        protected override string GetFailureMessage(Exception ex)
        {
            return "Adding 'TypeOfExport' setting has been failed in Relativity provider 'SourceConfiguration' column (IntegrationPoint/IntegrationPointProfile tables)";
        }

        protected override void Run()
        {
            ICommand command = SetTypeOfExportDefaultValueCommandFactory.Create(Helper, Helper.GetActiveCaseID());
            command.Execute();
        }
    }
}
