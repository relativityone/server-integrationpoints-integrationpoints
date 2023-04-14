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

        /// <summary>
        /// Checks if Integration Point configuration allows running using Sync DLL flow
        /// </summary>
        /// <returns></returns>
        bool ShouldUseRelativitySync(int integrationPointId);

        /// <summary>
        /// Checks if Sync Application is enabled for use.
        /// </summary>
        /// <returns></returns>
        bool IsRelativitySyncAppEnabled();
    }
}
