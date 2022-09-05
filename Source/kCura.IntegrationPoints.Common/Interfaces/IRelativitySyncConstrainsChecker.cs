using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Common.Interfaces
{
    public interface IRelativitySyncConstrainsChecker
    {
        /// <summary>
        /// Checks if Integration Point configuration allows running using new Sync Application flow
        /// </summary>
        /// <param name="integrationPointId"></param>
        /// <returns></returns>
        Task<bool> ShouldUseRelativitySyncAppAsync(int integrationPointId);

        /// <summary>
        /// Checks if Integration Point configuration allows running using Sync DLL flow
        /// </summary>
        /// <returns></returns>
        bool ShouldUseRelativitySync(int integrationPointId);
    }
}
