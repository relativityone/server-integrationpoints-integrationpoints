using System;
using kCura.EventHandler;
using kCura.IntegrationPoints.EventHandlers.Commands;
using kCura.IntegrationPoints.EventHandlers.Commands.Context;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
    /// <summary>
    /// May have your attention please!
    /// This EH seems to be useless, but nothing more wrong - do not be fooled by the lack of attributes and any references!
    /// Implementation of <see cref="kCura.EventHandler.PreCascadeDeleteEventHandler"/> is required and the instance is created from outside via reflection.
    /// </summary>
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
            // Do nothing
        }

        public override void Commit()
        {
            // Do nothing
        }
    }
}
