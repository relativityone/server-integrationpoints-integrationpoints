using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Services.ServiceContext
{
    public static class ServiceContextFactory
    {
        public static ICaseServiceContext CreateCaseServiceContext(IEHHelper helper, int workspaceID)
        {
            return new CaseServiceContext(new ServiceContextHelperForEventHandlers(helper, workspaceID));
        }

        public static IRelativityObjectManagerService CreateRelativityObjectManagerService(IHelper helper, int workspaceArtifactId)
        {
            return new RelativityObjectManagerServiceFactory(helper).Create(workspaceArtifactId);
        }
    }
}
