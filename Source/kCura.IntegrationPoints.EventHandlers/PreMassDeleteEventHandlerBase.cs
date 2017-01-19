using System;
using System.Collections.Generic;
using kCura.Apps.Common.Data;
using kCura.EventHandler;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;

namespace kCura.IntegrationPoints.EventHandlers
{
    public abstract class PreMassDeleteEventHandlerBase : PreMassDeleteEventHandler
    {
        private IRepositoryFactory _repositoryFactory;
        public PreMassDeleteEventHandlerBase()
        {
            //cant be initialized in constractor. IHelper is not initialized at that time and equals null.
            //_repositoryFactory = new RepositoryFactory(this.Helper);
        }

        internal PreMassDeleteEventHandlerBase(IRepositoryFactory repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
        }

        internal IRepositoryFactory RepositoryFactory
        {
            get
            {
                if (_repositoryFactory == null)
                {
                    _repositoryFactory = new RepositoryFactory(this.Helper, this.Helper.GetServicesManager());
                }
                return _repositoryFactory;
            }
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
            return _massAction ?? (_massAction = new GetArtifactForMassAction(RepositoryFactory));
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
            Apps.Common.Config.Manager.Settings.Factory = new HelperConfigSqlServiceFactory(Helper);
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
