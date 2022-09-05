using System.Threading.Tasks;
using kCura.IntegrationPoints.Common.Interfaces;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.RelativitySync
{
    public class RelativitySyncAppIntegration : IRelativitySyncAppIntegration
    {
        private readonly IServicesMgr _servicesMgr;
        private readonly IAPILog _logger;

        public RelativitySyncAppIntegration(IServicesMgr servicesMgr, IAPILog logger)
        {
            _servicesMgr = servicesMgr;
            _logger = logger;
        }

        public Task SubmitSyncJobAsync(int workspaceArtifactId, int integrationPointArtifactId, int userId)
        {
            throw new System.NotImplementedException();
        }
    }
}
