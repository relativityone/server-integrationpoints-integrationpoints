using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using Relativity.API;

namespace kCura.IntegrationPoints.Data
{
    public class RelativityObjectManagerService : IRelativityObjectManagerService
    {
        private readonly IHelper _helper;
        private readonly int _workspaceArtifactId;

        protected ExecutionIdentity ExecutionIdentity = ExecutionIdentity.CurrentUser;

        public virtual IRelativityObjectManager RelativityObjectManager => GetRelativityObjectManager();
        
        public RelativityObjectManagerService(IHelper helper, int workspaceArtifactId)
        {
            _helper = helper;
            _workspaceArtifactId = workspaceArtifactId;
        }
        
        public IRelativityObjectManager GetRelativityObjectManager()
        {
            var factory = new RelativityObjectManagerFactory(_helper);
            return factory.CreateRelativityObjectManager(_workspaceArtifactId);
        }
    }
}