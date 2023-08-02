using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Common.RelativitySync
{
    public interface IRelativitySyncConstrainsChecker
    {
        /// <summary>
        /// Checks if Integration Point configuration allows running using new Sync Application flow
        /// </summary>
        /// <param name="integrationPointId"></param>
        /// <returns></returns>
        bool ShouldUseRelativitySyncApp(int integrationPointId);
    }
}
