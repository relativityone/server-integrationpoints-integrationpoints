using System;
using System.Runtime.InteropServices;
using kCura.EventHandler;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.EventHandlers.Commands;
using kCura.IntegrationPoints.EventHandlers.Commands.Context;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
    [Description("Deletes any corresponding jobs")]
    [Guid("5EA14201-EEBE-4D1D-99FA-2E28C9FAB7F4")]
    public class DeleteEventHandler : PreDeleteEventHandler, IEventHandlerEx
    {
        public override FieldCollection RequiredFields => null;

        public override Response Execute()
        {
            var executor = new EventHandlerExecutorExHandler();
            return executor.Execute(this);
        }

        public IEHContext Context => new EHContext
        {
            Helper = Helper,
            ActiveArtifact = ActiveArtifact
        };

        public string SuccessMessage => "Integration Point successfully deleted.";
        public string FailureMessage => "Failed to delete corresponding secret.";
        public Type CommandType => typeof(DeleteIntegrationPointCommand);

        public override void Rollback()
        {
            //Do nothing
        }

        public override void Commit()
        {
            //Do nothing
        }
    }
}