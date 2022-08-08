using System;
using kCura.EventHandler;
using kCura.IntegrationPoints.EventHandlers.Commands;
using kCura.IntegrationPoints.EventHandlers.Commands.Context;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
    public class PreCascadeDeleteEventHandler : EventHandler.PreCascadeDeleteEventHandler, IEventHandlerEx
    {
        public override Response Execute()
        {
            var executor = new EventHandlerExecutorExHandler();
            return executor.Execute(this);
        }

        public override FieldCollection RequiredFields => new FieldCollection();

        public IEHContext Context => new EHContext
        {
            Helper = Helper,
            ActiveArtifact = ActiveArtifact,
            TempTableNameWithParentArtifactsToDelete = TempTableNameWithParentArtifactsToDelete
        };

        public string SuccessMessage => "Associated Job Histories successfully deleted.";

        public string FailureMessage => "An error occurred while executing the Mass Delete operation.";

        public Type CommandType => typeof(PreCascadeDeleteIntegrationPointCommand);

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