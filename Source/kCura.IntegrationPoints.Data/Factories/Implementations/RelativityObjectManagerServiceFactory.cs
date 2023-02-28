using Relativity.API;

namespace kCura.IntegrationPoints.Data.Factories.Implementations
{
    public class RelativityObjectManagerServiceFactory : IRelativityObjectManagerServiceFactory
    {
        private readonly IHelper _helper;

        public RelativityObjectManagerServiceFactory(IHelper helper)
        {
            _helper = helper;
        }

        public IRelativityObjectManagerService Create(int workspaceArtifactId)
        {
            return new RelativityObjectManagerService(_helper, workspaceArtifactId);
        }
    }
}
