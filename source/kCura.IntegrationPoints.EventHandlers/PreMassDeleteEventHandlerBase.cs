using System;
using System.Collections.Generic;
using kCura.EventHandler;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;

namespace kCura.IntegrationPoints.EventHandlers
{
    public abstract class PreMassDeleteEventHandlerBase : PreMassDeleteEventHandler
    {
        private readonly IRepositoryFactory _repositoryFactory;
        public PreMassDeleteEventHandlerBase()
        {
            _repositoryFactory = new RepositoryFactory(this.Helper);
        }

        internal PreMassDeleteEventHandlerBase(IRepositoryFactory repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
        }

        private IWorkspaceDBContext _workspaceDbContext;
        public IWorkspaceDBContext GetWorkspaceDbContext()
        {
            return _workspaceDbContext ??
                   (_workspaceDbContext = new WorkspaceContext(base.Helper.GetDBContext(base.Helper.GetActiveCaseID())));
        }

        private GetArtifactForMassAction _massAction;

        public GetArtifactForMassAction MassAction()
        {
            return _massAction ?? (_massAction = new GetArtifactForMassAction(_repositoryFactory));
        }

        public override void Commit()
        { }

        public override void Rollback()
        { }

        public sealed override Response Execute()
        {
            List<int> ids = GetIds();
            return ExecutePreDelete(ids);
        }

        private List<int> GetIds()
        {
            //Get a dbContext for the current workspace
            Int32 currentWorkspaceArtifactId = this.Helper.GetActiveCaseID();
            //Get the temp table name of the artifactIDs to be deleted
            String tempTableName = this.TempTableNameWithParentArtifactsToDelete;
            //Get a list of the artifactIDs to be deleted
            List<Int32> artifactIDsToBeDeleted = MassAction().GetArtifactsToBeDeleted(_workspaceDbContext, tempTableName, currentWorkspaceArtifactId);

            return artifactIDsToBeDeleted;
        }

        public abstract Response ExecutePreDelete(List<int> artifactIDs);

        public override FieldCollection RequiredFields
        {
            get { return new FieldCollection(); }
        }
    }
}
